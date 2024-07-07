using Mirror;
using UnityEngine;
using NaughtyAttributes;
using ReadOnlyAttribute = NaughtyAttributes.ReadOnlyAttribute;
using Fifbox.API.ScriptableObjects.Configs;
using Fifbox.API.ScriptableObjects;

namespace Fifbox.Content.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player : NetworkBehaviour
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
        [field: SerializeField, ReadOnly, AllowNesting] public PlayerInputs Inputs { get; private set; }

        [field: Header("States")]
        [field: SerializeField, ReadOnly, AllowNesting] public PlayerState State { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public MovementState MoveState { get; private set; }

        [field: Space(9)]

        [field: SerializeField, ReadOnly, AllowNesting] public MovementState LastMovingMoveState { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool Crouching { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool WasCrouchingLastFrame { get; private set; }
        [SerializeField, ReadOnly, AllowNesting] private bool _nocliping;

        public enum PlayerState
        {
            OnGround,
            InAir,
            Nocliping
        }

        public enum MovementState
        {
            None,
            Walk,
            Run,
            Crouch
        }

        private int _initialLayer;
        private float _height;
        private float _jumpBufferTimer;
        private float _maxStepHeight;
        private float _stepDownBufferHeight;
        private Vector2 _lastGroundedVelocity;

        public float WidthForChecking => _currentConfig.width - 0.001f;
        private PlayerConfig _currentConfig;

        private bool TryGetOptimalConfig()
        {
            var got = ConfigUtility.TryGetOptimalConfig(Config, out _currentConfig);
            if (!got) Debug.LogWarning("Player cant get optimal config");
            return got;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!TryGetOptimalConfig()) return;

            if (_currentConfig)
            {
                _stepDownBufferHeight = _currentConfig.stepDownBufferHeight;
                _maxStepHeight = _currentConfig.maxStepHeight;
                _height = _currentConfig.fullHeight;
            }

            if (TryGetComponent(out Rigidbody rb))
            {
                Rigidbody = rb;
                if (_currentConfig) Rigidbody.mass = _currentConfig.mass;
                Rigidbody.useGravity = false;
                Rigidbody.isKinematic = false;
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                Rigidbody.freezeRotation = true;
            }

            if (Collider)
            {
                Collider.isTrigger = false;
                UpdatePlayerCollider();
            }

            if (Center)
            {
                UpdatePlayerCenter();
            }
        }

        private void UpdatePlayerCollider()
        {
            Collider.size = new Vector3(_currentConfig.width, _height - _maxStepHeight, _currentConfig.width);
            Collider.center = new Vector3(0, _maxStepHeight / 2, 0);
        }

        private void UpdatePlayerCenter()
        {
            Center.localPosition = new Vector3(0, _height / 2, 0);
        }

        private void Awake()
        {
            if (!TryGetOptimalConfig()) return;
            _stepDownBufferHeight = _currentConfig.stepDownBufferHeight;
            _maxStepHeight = _currentConfig.maxStepHeight;
            _height = _currentConfig.fullHeight;
        }

        private void Start()
        {
            if (isLocalPlayer)
            {
                _initialLayer = FifboxLayers.LocalPlayerLayer.LayerIndex;
                LocalStart();
            }
            else _initialLayer = FifboxLayers.PlayerLayer.LayerIndex;

            SetLayer(_initialLayer);
        }

        private void LocalStart()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Model.SetActive(false);

            Inputs.tryJump += TryJump;
            Inputs.toggleNoclip += ToggleNoclip;
        }

        private void SetLayer(int layer)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = layer;
            }
        }

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;

            Inputs.tryJump -= TryJump;
            Inputs.toggleNoclip -= ToggleNoclip;
        }

        private void TryJump()
        {
            if (!TryGetOptimalConfig()) return;
            _jumpBufferTimer = _currentConfig.jumpBufferTime;
        }

        private void ToggleNoclip()
        {
            _nocliping = !_nocliping;
            SetLayer(_nocliping ? FifboxLayers.NoclipingPlayerLayer.LayerIndex : _initialLayer);
        }

        private void Update()
        {
            if (!isLocalPlayer || !TryGetOptimalConfig()) return;

            HandleCrouching();

            CeilCheck();
            GroundCheck();
            UpdateStates();

            ApplyGravity();
            ApplyFriction();

            Orientation.localRotation = Quaternion.Euler(0, Inputs.orientationEulerAngles.y, 0);

            HandleJump();
            HandleMoving();
        }

        private void LateUpdate()
        {
            if (!isLocalPlayer || !TryGetOptimalConfig()) return;

            GroundCheck();
            MoveToGround();
        }

        private void HandleCrouching()
        {
            if (State == PlayerState.Nocliping)
            {
                CanStandUp = false;
                Crouching = false;
                return;
            }

            var canStandUpCheckSize = new Vector3(WidthForChecking, _currentConfig.fullHeight - _currentConfig.crouchHeight, WidthForChecking);
            var canStandUpCheckPosition = transform.position + Vector3.up * (_currentConfig.fullHeight + _currentConfig.crouchHeight) / 2;
            CanStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);

            WasCrouchingLastFrame = Crouching;
            var crouching = Inputs.wantsToCrouch && MoveState != MovementState.Run;
            if (WasCrouchingLastFrame && !crouching && !CanStandUp) crouching = true;
            Crouching = crouching;

            if (Grounded)
            {
                _height = Crouching ? _currentConfig.crouchHeight : _currentConfig.fullHeight;
                _maxStepHeight = _currentConfig.maxStepHeight;
            }
            else
            {
                _height = _currentConfig.fullHeight;
                _maxStepHeight = Crouching ? _currentConfig.maxStepHeight / 2 + _currentConfig.fullHeight - _currentConfig.crouchHeight : _currentConfig.maxStepHeight;
            }

            UpdatePlayerCollider();
            UpdatePlayerCenter();
        }

        private void CeilCheck()
        {
            var ceiledCheckSize = new Vector3(WidthForChecking, 0.02f, WidthForChecking);
            var ceiledCheckPosition = transform.position + Vector3.up * _height;
            Ceiled = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);

            if (Grounded && Ceiled && !_nocliping)
            {
                _maxStepHeight = 0f;
            }

            UpdatePlayerCollider();
        }

        private void GroundCheck()
        {
            var useBuffer = Rigidbody.linearVelocity.y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (_maxStepHeight - _stepDownBufferHeight) / 2
                : transform.position + Vector3.up * _maxStepHeight / 2;

            var groundedCheckSize = new Vector3(WidthForChecking, useBuffer ? _maxStepHeight + _stepDownBufferHeight : _maxStepHeight, WidthForChecking);
            Grounded = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers, QueryTriggerInteraction.Ignore);

            var groundInfoCheckPosition = transform.position + 2f * _maxStepHeight * Vector3.up;
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

        private void UpdateStates()
        {
            UpdatePlayerState();
            UpdateMovementState();
        }

        private void UpdatePlayerState()
        {
            if (_nocliping)
            {
                State = PlayerState.Nocliping;
            }
            else
            {
                if (Grounded) State = PlayerState.OnGround;
                else State = PlayerState.InAir;
            }
        }

        private void UpdateMovementState()
        {
            if (State == PlayerState.Nocliping)
            {
                MoveState = MovementState.None;
                return;
            }

            if (Inputs.moveVector.magnitude > 0)
            {
                if (Inputs.wantsToRun) MoveState = MovementState.Run;
                else if (Inputs.wantsToCrouch) MoveState = MovementState.Crouch;
                else MoveState = MovementState.Walk;
            }
            else MoveState = MovementState.None;

            if (MoveState != MovementState.None) LastMovingMoveState = MoveState;
        }

        private void HandleJump()
        {
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
            var targetForce = MoveState switch
            {
                MovementState.Walk => _currentConfig.walkJumpForce,
                MovementState.Run => _currentConfig.runJumpForce,
                _ => _currentConfig.walkJumpForce
            };

            if (Crouching) targetForce = CanStandUp ? _currentConfig.crouchJumpForce : 0f;

            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, targetForce, Rigidbody.linearVelocity.z);
        }

        private void ApplyGravity()
        {
            if (_nocliping) return;

            if (Grounded && Rigidbody.linearVelocity.y <= 0) Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
            else Rigidbody.linearVelocity += _currentConfig.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void ApplyFriction()
        {
            float speed, newSpeed, control, drop;

            speed = Rigidbody.linearVelocity.magnitude;
            drop = 0f;

            if (Grounded)
            {

                var deceleration = LastMovingMoveState switch
                {
                    MovementState.Walk => _currentConfig.walkDeceleration,
                    MovementState.Run => _currentConfig.runDeceleration,
                    _ => 0f
                };

                if (Crouching) deceleration = _currentConfig.crouchDeceleration;

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
            if (_nocliping) NoclipMovement();
            else Accelerate();
        }

        private void NoclipMovement()
        {
            _maxStepHeight = 0f;
            UpdatePlayerCollider();

            var targetSpeed = Inputs.wantsToRun ? _currentConfig.noclipFastFlySpeed : _currentConfig.noclipNormalFlySpeed;

            var fullOrientation = Quaternion.Euler(Inputs.orientationEulerAngles.x, Inputs.orientationEulerAngles.y, 0f);
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
            var velocity = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z);

            float targetSpeed;
            if (Grounded)
            {
                _lastGroundedVelocity = velocity;

                targetSpeed = MoveState switch
                {
                    MovementState.Walk => _currentConfig.walkSpeed,
                    MovementState.Run => _currentConfig.runSpeed,
                    _ => 0f
                };
                if (Crouching) targetSpeed = _currentConfig.crouchSpeed;
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

            var acceleration = MoveState switch
            {
                MovementState.Walk => _currentConfig.walkAcceleration,
                MovementState.Run => _currentConfig.runAcceleration,
                _ => 0f
            };

            if (Crouching) acceleration = _currentConfig.crouchAcceleration;

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
            if (!Grounded || Ceiled || Rigidbody.linearVelocity.y > 0f || _nocliping) return;

            transform.position = new(transform.position.x, GroundHeight, transform.position.z);
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (!TryGetOptimalConfig()) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _maxStepHeight / 2, new(_currentConfig.width, _maxStepHeight, _currentConfig.width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * _stepDownBufferHeight / 2, new(_currentConfig.width, _stepDownBufferHeight, _currentConfig.width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _height, new(_currentConfig.width, 0.02f, _currentConfig.width));

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