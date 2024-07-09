using Mirror;
using UnityEngine;
using NaughtyAttributes;
using Fifbox.ScriptableObjects.Configs;
using Fifbox.ScriptableObjects;

using ReadOnlyAttribute = NaughtyAttributes.ReadOnlyAttribute;
using ZSToolkit.ZSTUtility.Extensions;
using Fifbox.Game.Player.StateMachine;
using Fifbox.Game.Player.StateMachine.States;

namespace Fifbox.Game.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Player : NetworkBehaviour
    {
        public const float MAX_GROUND_INFO_CHECK_DISTANCE = 100f;
        public float WidthForChecking => ConfigToUse.width - 0.001f;

        [field: Header("References")]
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Orientation { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PlayerConfig _config;

        public PlayerConfig ConfigToUse
        {
            get
            {
                if (_config) return _config;
                else return DefaultConfigs.TryGetDefaultConfigOrCreateNew<PlayerConfig>();
            }
            set => _config = value;
        }

        [field: Header("Info")]
        [field: SerializeField, ReadOnly, AllowNesting] public PlayerInputs Inputs { get; private set; } = new();
        [field: SerializeField, ReadOnly, AllowNesting] public PlayerData Data { get; private set; } = new();

        public PlayerStateMachine<PlayerState, OnGroundState> StateMachine { get; private set; }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (TryGetComponent(out Rigidbody rb))
            {
                Rigidbody = rb;
                Rigidbody.useGravity = false;
                Rigidbody.isKinematic = false;
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                Rigidbody.freezeRotation = true;

                if (ConfigToUse) Rigidbody.mass = ConfigToUse.mass;
            }

            if (Collider)
            {
                Collider.isTrigger = false;

                if (ConfigToUse)
                {
                    Collider.center = PlayerUtility.GetColliderCenter(ConfigToUse.maxStepHeight);
                    Collider.size = PlayerUtility.GetColliderSize(ConfigToUse.width, ConfigToUse.fullHeight, ConfigToUse.maxStepHeight);
                }
            }

            if (Center)
            {
                if (ConfigToUse) Center.localPosition = PlayerUtility.GetCenterPosition(ConfigToUse.fullHeight);
            }
        }

        protected abstract bool ShouldProcessPlayer { get; }
        private void Awake() => OnPlayerAwake();
        private void Start() => OnPlayerStart();
        private void OnDestroy() => OnPlayerDestroy();
        private void Update() => OnPlayerUpdate();
        private void LateUpdate() => OnPlayerLateUpdate();

        protected virtual void OnPlayerAwake()
        {
            StateMachine = new(this);
            Data.initialLayer = FifboxLayers.PlayerLayer.Index;
            Data.currentMaxStepHeight = ConfigToUse.maxStepHeight;
            Data.currentHeight = ConfigToUse.fullHeight;
        }

        protected virtual void OnPlayerStart()
        {
            gameObject.SetLayerForChildren(Data.initialLayer);

            if (!ShouldProcessPlayer) return;

            Inputs.setOrientationEulerAngles += SetOrientation;
            Inputs.tryJump += TryJump;
            StateMachine.Start();
        }

        protected virtual void OnPlayerDestroy()
        {
            if (!ShouldProcessPlayer) return;

            Inputs.setOrientationEulerAngles -= SetOrientation;
            Inputs.tryJump -= TryJump;
            StateMachine.Stop();
        }

        protected virtual void OnPlayerUpdate()
        {
            if (!ShouldProcessPlayer) return;

            //HandleCrouching();

            CeilCheck();
            GroundCheck();

            //UpdateColliderAndCenter();
            StateMachine.Update();

            //ApplyGravity();
            //ApplyFriction();

            //HandleJump();
            //HandleMoving();
        }

        protected virtual void OnPlayerLateUpdate()
        {
            if (!ShouldProcessPlayer) return;

            GroundCheck();
            MoveToGround();
        }

        private void TryJump()
        {
            if (!ShouldProcessPlayer) return;

            StateMachine.CurrentState.TryJump();
        }

        private void SetOrientation(Vector3 eulerAngles)
        {
            if (!ShouldProcessPlayer) return;

            Data.fullOrientationEulerAngles = eulerAngles;
            Orientation.localRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
        }

        /* DONE
        private void HandleCrouching()
        {
            if (!ShouldProcessPlayer) return;

            if (Inputs.nocliping)
            {
                Data.canStandUp = false;
                Data.crouching = false;
                return;
            }

            var canStandUpCheckSize = new Vector3(WidthForChecking, ConfigToUse.fullHeight - ConfigToUse.crouchHeight, WidthForChecking);
            var canStandUpCheckPosition = transform.position + Vector3.up * (ConfigToUse.fullHeight + ConfigToUse.crouchHeight) / 2;
            Data.canStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);

            Data.wasCrouchingLastFrame = Data.crouching;
            var crouching = Inputs.wantsToCrouch;
            if (Data.wasCrouchingLastFrame && !crouching && !Data.canStandUp) crouching = true;
            Data.crouching = crouching;
        }
        */

        private void CeilCheck()
        {
            if (!ShouldProcessPlayer) return;

            var ceiledCheckSize = new Vector3(WidthForChecking, 0.02f, WidthForChecking);
            var ceiledCheckPosition = transform.position + Vector3.up * Data.currentHeight;
            Data.touchingCeiling = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }

        private void GroundCheck()
        {
            if (!ShouldProcessPlayer) return;

            var useBuffer = Rigidbody.linearVelocity.y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (Data.currentMaxStepHeight - ConfigToUse.stepDownBufferHeight) / 2
                : transform.position + Vector3.up * Data.currentMaxStepHeight / 2;

            var groundedCheckSize = new Vector3(WidthForChecking, useBuffer ? Data.currentMaxStepHeight + ConfigToUse.stepDownBufferHeight : Data.currentMaxStepHeight, WidthForChecking);
            Data.touchingGround = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);

            var groundInfoCheckPosition = transform.position + 2f * Data.currentMaxStepHeight * Vector3.up;
            var groundInfoCheckSize = new Vector3(WidthForChecking, 0.1f, WidthForChecking);
            Physics.BoxCast
            (
                groundInfoCheckPosition,
                groundInfoCheckSize / 2,
                Vector3.down,
                out var hit,
                Quaternion.identity,
                MAX_GROUND_INFO_CHECK_DISTANCE,
                FifboxLayers.GroundLayers
            );

            Data.groundHeight = hit.point.y;
            Data.groundNormal = hit.normal;
            Data.groundAngle = Vector3.Angle(Data.groundNormal, Vector3.up);
        }

        /* DONE
        private void UpdateColliderAndCenter()
        {
            if (!ShouldProcessPlayer || Inputs.nocliping) return;

            if (Data.touchingGround)
            {
                Data.currentHeight = Data.crouching ? ConfigToUse.crouchHeight : ConfigToUse.fullHeight;
                Data.currentMaxStepHeight = ConfigToUse.maxStepHeight;
            }
            else
            {
                Data.currentHeight = ConfigToUse.fullHeight;
                Data.currentMaxStepHeight = Data.crouching ? ConfigToUse.maxStepHeight / 2 + ConfigToUse.fullHeight - ConfigToUse.crouchHeight : ConfigToUse.maxStepHeight;
            }

            if (Data.touchingCeiling) Data.currentMaxStepHeight = 0f;

            Collider.center = PlayerUtility.GetColliderCenter(Data.currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(ConfigToUse.width, Data.currentHeight, Data.currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(Data.currentHeight);
        }
        */

        /* DONE
        private void HandleJump()
        {
            if (!ShouldProcessPlayer) return;

            if (Data.jumpBufferTimer > 0)
            {
                if (Data.touchingGround)
                {
                    Jump();
                    Data.jumpBufferTimer = 0;
                }
                else
                {
                    Data.jumpBufferTimer -= Time.deltaTime;
                }
            }
        }

        private void Jump()
        {
            if (!ShouldProcessPlayer) return;

            float targetForce;
            if (Data.crouching) targetForce = Data.canStandUp ? ConfigToUse.crouchJumpForce : 0f;
            else if (Inputs.wantsToRun) targetForce = ConfigToUse.runJumpForce;
            else targetForce = ConfigToUse.walkJumpForce;

            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, targetForce, Rigidbody.linearVelocity.z);
        }
        */

        /* DONE
        private void ApplyGravity()
        {
            if (!ShouldProcessPlayer) return;

            if (Inputs.nocliping) return;

            if (Data.touchingGround && Rigidbody.linearVelocity.y <= 0) Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
            else Rigidbody.linearVelocity += ConfigToUse.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }
        */

        /* DONE
        private void ApplyFriction()
        {
            if (!ShouldProcessPlayer) return;

            float speed, newSpeed, control, drop;

            speed = Rigidbody.linearVelocity.magnitude;
            drop = 0f;

            if (Data.touchingGround)
            {
                if (Inputs.moveVector.magnitude > 0f)
                {
                    if (Data.crouching) Data.lastMovingDeceleration = ConfigToUse.crouchDeceleration;
                    else if (Inputs.wantsToRun) Data.lastMovingDeceleration = ConfigToUse.runDeceleration;
                    else Data.lastMovingDeceleration = ConfigToUse.walkDeceleration;
                }
                var deceleration = Data.lastMovingDeceleration;

                control = speed < deceleration ? deceleration : speed;
                drop += control * ConfigToUse.friction * Time.deltaTime;
            }

            newSpeed = Mathf.Max(speed - drop, 0f);

            if (newSpeed != speed)
            {
                newSpeed /= speed;
                Rigidbody.linearVelocity *= newSpeed;
            }
        }
        */

        private void HandleMoving()
        {
            if (!ShouldProcessPlayer) return;

            // if (Inputs.nocliping) NoclipMovement();
            // else Accelerate();
        }

        /* DONE
        private void NoclipMovement()
        {
            if (!ShouldProcessPlayer) return;

            var targetSpeed = Inputs.wantsToRun ? ConfigToUse.noclipFastFlySpeed : ConfigToUse.noclipNormalFlySpeed;

            var fullOrientation = Quaternion.Euler(Data.fullOrientationEulerAngles.x, Data.fullOrientationEulerAngles.y, 0f);
            var forward = fullOrientation * Vector3.forward;
            var right = fullOrientation * Vector3.right;
            var direction = right * Inputs.moveVector.x + forward * Inputs.moveVector.y;

            var verticalModifierDirection = 0f;
            if (Inputs.wantsToCrouch) verticalModifierDirection -= 1f;
            if (Inputs.wantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * ConfigToUse.noclipVerticalModifierSpeed;

            Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }
        */

        /* DONE
        private void Accelerate()
        {
            if (!ShouldProcessPlayer) return;

            var velocity = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z);

            float targetSpeed;
            if (Data.touchingGround)
            {
                Data.lastGroundedVelocity = Rigidbody.linearVelocity;
                Data.lastGroundedVelocity.y = 0f;

                if (Data.crouching) targetSpeed = ConfigToUse.crouchSpeed;
                else if (Inputs.wantsToRun) targetSpeed = ConfigToUse.runSpeed;
                else targetSpeed = ConfigToUse.walkSpeed;
            }
            else targetSpeed = Data.lastGroundedVelocity.magnitude;

            var wishVel = (Orientation.right * Inputs.moveVector.x + Orientation.forward * Inputs.moveVector.y) * targetSpeed;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if ((wishSpeed != 0f) && (wishSpeed > ConfigToUse.maxSpeed))
            {
                wishSpeed = ConfigToUse.maxSpeed;
            }

            var currentSpeed = Vector2.Dot(velocity, wishDir);
            if (Data.touchingGround) velocity += GroundAccelerate(wishDir, wishSpeed, currentSpeed);
            else velocity += AirAccelerate(wishDir, wishSpeed, currentSpeed, velocity);

            Rigidbody.linearVelocity = new(velocity.x, Rigidbody.linearVelocity.y, velocity.y);
        }
        */

        // TODO: slope limit

        /* DONE
        private Vector2 GroundAccelerate(Vector2 wishDir, float wishSpeed, float currentSpeed)
        {
            var addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return Vector2.zero;

            float acceleration;
            if (Data.crouching) acceleration = ConfigToUse.crouchAcceleration;
            else if (Inputs.wantsToRun) acceleration = ConfigToUse.runAcceleration;
            else acceleration = ConfigToUse.walkAcceleration;

            var accelSpeed = acceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            return wishDir * accelSpeed;
        }
        */

        /* DONE
        private Vector2 AirAccelerate(Vector2 wishDir, float wishSpeed, float currentSpeed, Vector2 velocity)
        {
            var airWishSpeed = Mathf.Min(wishSpeed, ConfigToUse.airSpeedCap);
            var addSpeed = airWishSpeed - currentSpeed;

            if (addSpeed <= 0) return Vector2.zero;

            var accelSpeed = ConfigToUse.airAcceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            return wishDir * accelSpeed;
        }
        */

        private void MoveToGround()
        {
            if (!Data.touchingGround || Data.touchingCeiling || Rigidbody.linearVelocity.y > 0f || Inputs.nocliping || !ShouldProcessPlayer) return;

            transform.position = new(transform.position.x, Data.groundHeight, transform.position.z);
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
        }

        public void UpdateColliderAndCenter()
        {
            Collider.center = PlayerUtility.GetColliderCenter(Data.currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(ConfigToUse.width, Data.currentHeight, Data.currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(Data.currentHeight);
        }

        // TODO: ???
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Data.currentMaxStepHeight / 2, new(ConfigToUse.width, Data.currentMaxStepHeight, ConfigToUse.width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * ConfigToUse.stepDownBufferHeight / 2, new(ConfigToUse.width, ConfigToUse.stepDownBufferHeight, ConfigToUse.width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Data.currentHeight, new(ConfigToUse.width, 0.02f, ConfigToUse.width));

            Gizmos.color = Color.green;

            if (Application.isPlaying) return;
            var position = transform.position + Vector3.up * (ConfigToUse.maxStepHeight / 2 + ConfigToUse.crouchHeight / 2);
            var size = new Vector3(ConfigToUse.width, ConfigToUse.crouchHeight - ConfigToUse.maxStepHeight, ConfigToUse.width);
            Gizmos.DrawWireCube(position, size);
        }
    }
}