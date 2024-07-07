using Mirror;
using UnityEngine;
using NaughtyAttributes;
using Fifbox.ScriptableObjects.Configs;
using Fifbox.ScriptableObjects;

using ReadOnlyAttribute = NaughtyAttributes.ReadOnlyAttribute;
using ZSToolkit.ZSTUtility.Extensions;
using System;

namespace Fifbox.Game.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Player : NetworkBehaviour
    {
        public const float MAX_GROUND_INFO_CHECK_DISTANCE = 100f;

        [field: Header("References")]
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Orientation { get; private set; }

        [field: Header("Configuration")]
        [field: SerializeField] public PlayerConfig Config { get; private set; }

        [field: Header("Ground info")]
        [field: SerializeField, ReadOnly, AllowNesting] public bool Grounded { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool Ceiled { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool CanStandUp { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Vector3 GroundNormal { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public float GroundAngle { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public float GroundHeight { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public float PreviousGroundHeight { get; private set; }

        [field: Header("Inputs")]
        [field: SerializeField, ReadOnly, AllowNesting] public PlayerInputs Inputs { get; private set; } = new();

        [field: Header("States")]
        [field: SerializeField, ReadOnly, AllowNesting] public bool Crouching { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool WasCrouchingLastFrame { get; private set; }
        [SerializeField, ReadOnly, AllowNesting] private bool _nocliping;

        protected int _initialLayer;
        protected float _currentHeight;
        protected float _currentMaxStepHeight;
        protected float _jumpBufferTimer;
        protected Vector2 _lastGroundedVelocity;
        protected float _lastMovingDeceleration;
        protected Vector3 _fullOrientationEulerAngles;

        public float WidthForChecking => _currentConfig.width - 0.001f;
        protected PlayerConfig _currentConfig;

        protected bool TryUpdateCurrentConfig()
        {
            var got = ConfigUtility.TryGetOptimalConfig(Config, out _currentConfig);
            if (!got) Debug.LogWarning("Player cant get optimal config");
            return got;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            TryUpdateCurrentConfig();

            if (TryGetComponent(out Rigidbody rb))
            {
                Rigidbody = rb;
                Rigidbody.useGravity = false;
                Rigidbody.isKinematic = false;
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                Rigidbody.freezeRotation = true;

                if (_currentConfig) Rigidbody.mass = _currentConfig.mass;
            }

            if (Collider)
            {
                Collider.isTrigger = false;

                if (_currentConfig)
                {
                    Collider.center = PlayerUtility.GetColliderCenter(_currentConfig.maxStepHeight);
                    Collider.size = PlayerUtility.GetColliderSize(_currentConfig.width, _currentConfig.fullHeight, _currentConfig.maxStepHeight);
                }
            }

            if (Center)
            {
                if (_currentConfig) Center.localPosition = PlayerUtility.GetCenterPosition(_currentConfig.fullHeight);
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
            if (!TryUpdateCurrentConfig()) return;

            _initialLayer = FifboxLayers.PlayerLayer.Index;
            _currentMaxStepHeight = _currentConfig.maxStepHeight;
            _currentHeight = _currentConfig.fullHeight;
        }

        protected virtual void OnPlayerStart()
        {
            gameObject.SetLayerForChildren(_initialLayer);

            if (!ShouldProcessPlayer) return;

            Inputs.setOrientationEulerAngles += SetOrientation;
            Inputs.tryJump += TryJump;
            Inputs.toggleNoclip += ToggleNoclip;
        }

        protected virtual void OnPlayerDestroy()
        {
            if (!ShouldProcessPlayer) return;

            Inputs.setOrientationEulerAngles -= SetOrientation;
            Inputs.tryJump -= TryJump;
            Inputs.toggleNoclip -= ToggleNoclip;
        }

        protected virtual void OnPlayerUpdate()
        {
            if (!ShouldProcessPlayer || !TryUpdateCurrentConfig()) return;

            HandleCrouching();

            CeilCheck();
            GroundCheck();

            UpdateColliderAndCenter();

            ApplyGravity();
            ApplyFriction();

            HandleJump();
            HandleMoving();
        }

        protected virtual void OnPlayerLateUpdate()
        {
            if (!ShouldProcessPlayer || !TryUpdateCurrentConfig()) return;

            GroundCheck();
            MoveToGround();
        }

        private void SetOrientation(Vector3 eulerAngles)
        {
            if (!ShouldProcessPlayer) return;

            _fullOrientationEulerAngles = eulerAngles;
            Orientation.localRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
        }

        private void TryJump()
        {
            if (!ShouldProcessPlayer || !TryUpdateCurrentConfig()) return;

            _jumpBufferTimer = _currentConfig.jumpBufferTime;
        }

        private void ToggleNoclip()
        {
            if (!ShouldProcessPlayer) return;

            _nocliping = !_nocliping;
            gameObject.SetLayerForChildren(_nocliping ? FifboxLayers.NoclipingPlayerLayer.Index : _initialLayer);

            _currentMaxStepHeight = _nocliping ? 0f : _currentConfig.maxStepHeight;
            Collider.center = PlayerUtility.GetColliderCenter(_currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(_currentConfig.width, _currentHeight, _currentMaxStepHeight);
        }

        private void HandleCrouching()
        {
            if (!ShouldProcessPlayer) return;

            if (_nocliping)
            {
                CanStandUp = false;
                Crouching = false;
                return;
            }

            var canStandUpCheckSize = new Vector3(WidthForChecking, _currentConfig.fullHeight - _currentConfig.crouchHeight, WidthForChecking);
            var canStandUpCheckPosition = transform.position + Vector3.up * (_currentConfig.fullHeight + _currentConfig.crouchHeight) / 2;
            CanStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);

            WasCrouchingLastFrame = Crouching;
            var crouching = Inputs.wantsToCrouch;
            if (WasCrouchingLastFrame && !crouching && !CanStandUp) crouching = true;
            Crouching = crouching;
        }

        private void CeilCheck()
        {
            if (!ShouldProcessPlayer) return;

            var ceiledCheckSize = new Vector3(WidthForChecking, 0.02f, WidthForChecking);
            var ceiledCheckPosition = transform.position + Vector3.up * _currentHeight;
            Ceiled = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void GroundCheck()
        {
            if (!ShouldProcessPlayer) return;

            var useBuffer = Rigidbody.linearVelocity.y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (_currentMaxStepHeight - _currentConfig.stepDownBufferHeight) / 2
                : transform.position + Vector3.up * _currentMaxStepHeight / 2;

            var groundedCheckSize = new Vector3(WidthForChecking, useBuffer ? _currentMaxStepHeight + _currentConfig.stepDownBufferHeight : _currentMaxStepHeight, WidthForChecking);
            Grounded = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);

            var groundInfoCheckPosition = transform.position + 2f * _currentMaxStepHeight * Vector3.up;
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

            if (GroundHeight != hit.point.y)
            {
                PreviousGroundHeight = GroundHeight;
            }
            GroundHeight = hit.point.y;

            GroundNormal = hit.normal;
            GroundAngle = Vector3.Angle(GroundNormal, Vector3.up);
        }

        private void UpdateColliderAndCenter()
        {
            if (!ShouldProcessPlayer || _nocliping) return;

            if (Grounded)
            {
                _currentHeight = Crouching ? _currentConfig.crouchHeight : _currentConfig.fullHeight;
                _currentMaxStepHeight = _currentConfig.maxStepHeight;
            }
            else
            {
                _currentHeight = _currentConfig.fullHeight;
                _currentMaxStepHeight = Crouching ? _currentConfig.maxStepHeight / 2 + _currentConfig.fullHeight - _currentConfig.crouchHeight : _currentConfig.maxStepHeight;
            }

            if (Ceiled) _currentMaxStepHeight = 0f;

            Collider.center = PlayerUtility.GetColliderCenter(_currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(_currentConfig.width, _currentHeight, _currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(_currentHeight);
        }

        private void HandleJump()
        {
            if (!ShouldProcessPlayer) return;

            if (_jumpBufferTimer > 0)
            {
                if (Grounded)
                {
                    Jump();
                    _jumpBufferTimer = 0;
                }
                else
                {
                    _jumpBufferTimer -= Time.deltaTime;
                }
            }
        }

        private void Jump()
        {
            if (!ShouldProcessPlayer) return;

            float targetForce;
            if (Crouching) targetForce = CanStandUp ? _currentConfig.crouchJumpForce : 0f;
            else if (Inputs.wantsToRun) targetForce = _currentConfig.runJumpForce;
            else targetForce = _currentConfig.walkJumpForce;

            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, targetForce, Rigidbody.linearVelocity.z);
        }

        private void ApplyGravity()
        {
            if (!ShouldProcessPlayer) return;

            if (_nocliping) return;

            if (Grounded && Rigidbody.linearVelocity.y <= 0) Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
            else Rigidbody.linearVelocity += _currentConfig.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void ApplyFriction()
        {
            if (!ShouldProcessPlayer) return;

            float speed, newSpeed, control, drop;

            speed = Rigidbody.linearVelocity.magnitude;
            drop = 0f;

            if (Grounded)
            {
                if (Inputs.moveVector.magnitude > 0f)
                {
                    if (Crouching) _lastMovingDeceleration = _currentConfig.crouchDeceleration;
                    else if (Inputs.wantsToRun) _lastMovingDeceleration = _currentConfig.runDeceleration;
                    else _lastMovingDeceleration = _currentConfig.walkDeceleration;
                }
                var deceleration = _lastMovingDeceleration;

                control = speed < deceleration ? deceleration : speed;
                drop += control * _currentConfig.friction * Time.deltaTime;
            }

            newSpeed = Mathf.Max(speed - drop, 0f);

            if (newSpeed != speed)
            {
                newSpeed /= speed;
                Rigidbody.linearVelocity *= newSpeed;
            }
        }

        private void HandleMoving()
        {
            if (!ShouldProcessPlayer) return;

            if (_nocliping) NoclipMovement();
            else Accelerate();
        }

        private void NoclipMovement()
        {
            if (!ShouldProcessPlayer) return;

            var targetSpeed = Inputs.wantsToRun ? _currentConfig.noclipFastFlySpeed : _currentConfig.noclipNormalFlySpeed;

            var fullOrientation = Quaternion.Euler(_fullOrientationEulerAngles.x, _fullOrientationEulerAngles.y, 0f);
            var forward = fullOrientation * Vector3.forward;
            var right = fullOrientation * Vector3.right;
            var direction = right * Inputs.moveVector.x + forward * Inputs.moveVector.y;

            var verticalModifierDirection = 0f;
            if (Inputs.wantsToCrouch) verticalModifierDirection -= 1f;
            if (Inputs.wantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * _currentConfig.noclipVerticalModifierSpeed;

            Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }

        private void Accelerate()
        {
            if (!ShouldProcessPlayer) return;

            var velocity = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z);

            float targetSpeed;
            if (Grounded)
            {
                _lastGroundedVelocity = velocity;

                if (Crouching) targetSpeed = _currentConfig.crouchSpeed;
                else if (Inputs.wantsToRun) targetSpeed = _currentConfig.runSpeed;
                else targetSpeed = _currentConfig.walkSpeed;
            }
            else targetSpeed = _lastGroundedVelocity.magnitude;

            var wishVel = (Orientation.right * Inputs.moveVector.x + Orientation.forward * Inputs.moveVector.y) * targetSpeed;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if ((wishSpeed != 0f) && (wishSpeed > _currentConfig.maxSpeed))
            {
                wishSpeed = _currentConfig.maxSpeed;
            }

            var currentSpeed = Vector2.Dot(velocity, wishDir);
            if (Grounded) velocity += GroundAccelerate(wishDir, wishSpeed, currentSpeed);
            else velocity += AirAccelerate(wishDir, wishSpeed, currentSpeed, velocity);

            Rigidbody.linearVelocity = new(velocity.x, Rigidbody.linearVelocity.y, velocity.y);
        }

        private Vector2 GroundAccelerate(Vector2 wishDir, float wishSpeed, float currentSpeed)
        {
            var addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return Vector2.zero;

            float acceleration;
            if (Crouching) acceleration = _currentConfig.crouchAcceleration;
            else if (Inputs.wantsToRun) acceleration = _currentConfig.runAcceleration;
            else acceleration = _currentConfig.walkAcceleration;

            var accelSpeed = acceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            return wishDir * accelSpeed;
        }

        private Vector2 AirAccelerate(Vector2 wishDir, float wishSpeed, float currentSpeed, Vector2 velocity)
        {
            var airWishSpeed = Mathf.Min(wishSpeed, _currentConfig.airSpeedCap);
            var addSpeed = airWishSpeed - currentSpeed;

            if (addSpeed <= 0) return Vector2.zero;

            var accelSpeed = _currentConfig.airAcceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            return wishDir * accelSpeed;
        }

        private void MoveToGround()
        {
            if (!Grounded || Ceiled || Rigidbody.linearVelocity.y > 0f || _nocliping || !ShouldProcessPlayer) return;

            transform.position = new(transform.position.x, GroundHeight, transform.position.z);
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (!TryUpdateCurrentConfig()) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _currentMaxStepHeight / 2, new(_currentConfig.width, _currentMaxStepHeight, _currentConfig.width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * _currentConfig.stepDownBufferHeight / 2, new(_currentConfig.width, _currentConfig.stepDownBufferHeight, _currentConfig.width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _currentHeight, new(_currentConfig.width, 0.02f, _currentConfig.width));

            if (!Crouching)
            {
                Gizmos.color = Color.green;

                var position = transform.position + Vector3.up * (_currentConfig.maxStepHeight / 2 + _currentConfig.crouchHeight / 2);
                var size = new Vector3(_currentConfig.width, _currentConfig.crouchHeight - _currentConfig.maxStepHeight, _currentConfig.width);
                Gizmos.DrawWireCube(position, size);
            }
        }
    }
}