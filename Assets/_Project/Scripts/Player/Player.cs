using Mirror;
using UnityEngine;

namespace Fifbox.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Player : NetworkBehaviour
    {
        [field: Header("Objects")]
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Orientation { get; private set; }

        [field: Header("Body properties")]
        [field: SerializeField] public float Mass { get; private set; }
        [field: SerializeField] public float Width { get; private set; }
        [field: SerializeField] public float Height { get; private set; }
        [field: SerializeField] public float CrouchHeight { get; private set; }

        private float _height;

        [field: Header("Physics properties")]
        [field: SerializeField] public float Friction { get; private set; }
        [field: SerializeField] public float GravityMultiplier { get; private set; }

        [field: Header("Move properties")]
        [field: SerializeField] public float WalkSpeed { get; private set; }
        [field: SerializeField] public float WalkAcceleration { get; private set; }
        [field: SerializeField] public float WalkDeceleration { get; private set; }

        [field: Space(9)]

        [field: SerializeField] public float RunSpeed { get; private set; }
        [field: SerializeField] public float RunAcceleration { get; private set; }
        [field: SerializeField] public float RunDeceleration { get; private set; }

        [field: Space(9)]

        [field: SerializeField] public float CrouchSpeed { get; private set; }
        [field: SerializeField] public float CrouchAcceleration { get; private set; }
        [field: SerializeField] public float CrouchDeceleration { get; private set; }

        [field: Space(9)]

        [field: SerializeField] public float MaxSpeed { get; private set; }

        [field: Header("Jump properties")]
        [field: SerializeField] public float WalkJumpForce { get; private set; }
        [field: SerializeField] public float RunJumpForce { get; private set; }
        [field: SerializeField] public float CrouchJumpForce { get; private set; }

        [field: Space(9)]

        [field: SerializeField] public float JumpBufferTime { get; private set; }

        private float _jumpBufferTimer;

        [field: Header("Air handling")]
        [field: SerializeField] public float AirAccelerationGain { get; private set; }
        [field: SerializeField] public float MaxAirAcceleration { get; private set; }
        [field: SerializeField] public float AirSpeedCap { get; private set; }

        [field: Header("Noclip")]
        [field: SerializeField] public float NoclipNormalFlySpeed { get; private set; }
        [field: SerializeField] public float NoclipFastFlySpeed { get; private set; }
        [field: SerializeField] public float NoclipVerticalModifierSpeed { get; private set; }

        [field: Header("Ground handling")]
        [field: SerializeField] public LayerMask MapLayers { get; private set; }
        [field: SerializeField] public float MaxStepHeight { get; private set; }
        [field: SerializeField] public float StepDownBufferHeight { get; private set; }
        [field: SerializeField] public float MaxGroundInfoCheckDistance { get; private set; }

        private float _maxStepHeight;
        private float _stepDownBufferHeight;

        [field: Space(9)]

        [field: SerializeField, ReadOnly] public bool Grounded { get; private set; }
        [field: SerializeField, ReadOnly] public bool Ceiled { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanStandUp { get; private set; }
        [field: SerializeField, ReadOnly] public Vector3 GroundNormal { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundAngle { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundHeight { get; private set; }
        [field: SerializeField, ReadOnly] public float PreviousGroundHeight { get; private set; }

        protected override void OnValidate()
        {
            base.OnValidate();

            _stepDownBufferHeight = StepDownBufferHeight;
            _maxStepHeight = MaxStepHeight;
            _height = Height;

            if (TryGetComponent(out Rigidbody rb))
            {
                Rigidbody = rb;
                Rigidbody.mass = Mass;
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
            Collider.size = new Vector3(Width, _height - _maxStepHeight, Width);
            Collider.center = new Vector3(0, _maxStepHeight / 2, 0);
        }

        private void UpdatePlayerCenter()
        {
            Center.localPosition = new Vector3(0, _height / 2, 0);
        }

        protected int _initialLayer;

        protected void SetLayer(int layer)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = layer;
            }
        }

        private void Awake()
        {
            _stepDownBufferHeight = StepDownBufferHeight;
            _maxStepHeight = MaxStepHeight;
            _height = Height;
            _initialLayer = 5;
        }

        private void Start()
        {
            if (!isLocalPlayer) return;
            OnStart();
            SetLayer(_initialLayer);
        }

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        [Header("Inputs")]
        [SerializeField, ReadOnly] private Vector2 _rawMovementInput;
        [SerializeField, ReadOnly] private bool _wantsToRun;
        [SerializeField, ReadOnly] private bool _wantsToCrouch;
        [SerializeField, ReadOnly] private bool _nocliping;
        [SerializeField, ReadOnly] private bool _wantsToAscend;
        [SerializeField, ReadOnly] private float _verticalOrientation;

        public Vector2 RawMovementInput { get => _rawMovementInput; protected set => _rawMovementInput = value; }
        public bool WantsToRun { get => _wantsToRun; protected set => _wantsToRun = value; }
        public bool WantsToCrouch { get => _wantsToCrouch; protected set => _wantsToCrouch = value; }
        public bool WantsToNoclip { get => _nocliping; protected set => _nocliping = value; }
        public bool WantsToAscend { get => _wantsToAscend; protected set => _wantsToAscend = value; }
        public float VerticalOrientation { get => _verticalOrientation; protected set => _verticalOrientation = value; }

        protected void TryJump()
        {
            ResetJumpBuffer();
        }

        private void ResetJumpBuffer()
        {
            _jumpBufferTimer = JumpBufferTime;
        }

        [field: Header("States")]
        [field: SerializeField, ReadOnly] public PlayerState State { get; private set; }
        [field: SerializeField, ReadOnly] public MovementState MoveState { get; private set; }

        [field: Space(9)]

        [field: SerializeField, ReadOnly] public MovementState LastMovingMoveState { get; private set; }
        [field: SerializeField, ReadOnly] public bool Crouching { get; private set; }
        [field: SerializeField, ReadOnly] public bool WasCrouchingLastFrame { get; private set; }

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

        private void Update()
        {
            if (!isLocalPlayer) return;
            HandleCrouching();

            CeilCheck();
            GroundCheck();

            OnUpdate();
            UpdateStates();

            ApplyGravity();
            ApplyFriction();

            HandleJump();
            HandleMoving();
        }

        private void LateUpdate()
        {
            if (!isLocalPlayer) return;
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

            var width = Width - 0.001f;

            var canStandUpCheckSize = new Vector3(width, Height - CrouchHeight, width);
            var canStandUpCheckPosition = transform.position + Vector3.up * (Height + CrouchHeight) / 2;
            CanStandUp = !Physics.CheckBox(canStandUpCheckPosition, canStandUpCheckSize / 2f, Quaternion.identity, MapLayers, QueryTriggerInteraction.Ignore);

            WasCrouchingLastFrame = Crouching;
            var crouching = _wantsToCrouch && MoveState != MovementState.Run;
            if (WasCrouchingLastFrame && !crouching && !CanStandUp) crouching = true;
            Crouching = crouching;

            if (Grounded)
            {
                _height = Crouching ? CrouchHeight : Height;
                _maxStepHeight = MaxStepHeight;
            }
            else
            {
                _height = Height;
                _maxStepHeight = Crouching ? MaxStepHeight / 2 + Height - CrouchHeight : MaxStepHeight;
            }

            UpdatePlayerCollider();
            UpdatePlayerCenter();
        }

        private void CeilCheck()
        {
            var width = Width - 0.001f;
            var ceiledCheckSize = new Vector3(width, 0.02f, width);
            var ceiledCheckPosition = transform.position + Vector3.up * _height;
            Ceiled = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, MapLayers, QueryTriggerInteraction.Ignore);

            if (Grounded && Ceiled && !_nocliping)
            {
                _maxStepHeight = 0f;
            }

            UpdatePlayerCollider();
        }

        private void GroundCheck()
        {
            var width = Width - 0.001f;

            var useBuffer = Rigidbody.linearVelocity.y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (_maxStepHeight - _stepDownBufferHeight) / 2
                : transform.position + Vector3.up * _maxStepHeight / 2;

            var groundedCheckSize = new Vector3(width, useBuffer ? _maxStepHeight + _stepDownBufferHeight : _maxStepHeight, width);
            Grounded = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, MapLayers, QueryTriggerInteraction.Ignore);

            var groundInfoCheckPosition = transform.position + 2f * _maxStepHeight * Vector3.up;
            var groundInfoCheckSize = new Vector3(width, 0.1f, width);
            Physics.BoxCast
            (
                groundInfoCheckPosition,
                groundInfoCheckSize / 2,
                Vector3.down,
                out var hit,
                Quaternion.identity,
                MaxGroundInfoCheckDistance,
                MapLayers
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
                SetLayer(8);
                State = PlayerState.Nocliping;
            }
            else
            {
                SetLayer(_initialLayer);
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

            if (_rawMovementInput.magnitude > 0)
            {
                if (_wantsToRun) MoveState = MovementState.Run;
                else if (_wantsToCrouch) MoveState = MovementState.Crouch;
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
                MovementState.Walk => WalkJumpForce,
                MovementState.Run => RunJumpForce,
                _ => WalkJumpForce
            };

            if (Crouching) targetForce = CanStandUp ? CrouchJumpForce : 0f;

            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, targetForce, Rigidbody.linearVelocity.z);
        }

        private void ApplyGravity()
        {
            if (_nocliping) return;

            if (Grounded && Rigidbody.linearVelocity.y <= 0) Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
            else Rigidbody.linearVelocity += GravityMultiplier * Time.deltaTime * Physics.gravity;
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
                    MovementState.Walk => WalkDeceleration,
                    MovementState.Run => RunDeceleration,
                    _ => 0f
                };

                if (Crouching) deceleration = CrouchDeceleration;

                control = speed < deceleration ? deceleration : speed;
                drop += control * Friction * Time.deltaTime;
            }

            newSpeed = Mathf.Max(speed - drop, 0f);

            if (newSpeed != speed)
            {
                newSpeed /= speed;
                Rigidbody.linearVelocity *= newSpeed;
            }
        }

        private float _lastGroundedTargetSpeed;

        private void HandleMoving()
        {
            if (_nocliping) NoclipMovement();
            else Accelerate();
        }

        private void NoclipMovement()
        {
            _maxStepHeight = 0f;
            UpdatePlayerCollider();

            var targetSpeed = _wantsToRun ? NoclipFastFlySpeed : NoclipNormalFlySpeed;

            var fullOrientation = Quaternion.Euler(_verticalOrientation, Orientation.eulerAngles.y, 0f);
            var forward = fullOrientation * Vector3.forward;
            var right = fullOrientation * Vector3.right;
            var direction = right * RawMovementInput.x + forward * RawMovementInput.y;

            var verticalModifierDirection = 0f;
            if (_wantsToCrouch) verticalModifierDirection -= 1f;
            if (_wantsToAscend) verticalModifierDirection += 1f;
            var verticalModifierForce = verticalModifierDirection * NoclipVerticalModifierSpeed;

            Rigidbody.linearVelocity = (targetSpeed * direction) + Vector3.up * verticalModifierForce;
        }

        private void Accelerate()
        {
            var targetSpeed = MoveState switch
            {
                MovementState.Walk => WalkSpeed,
                MovementState.Run => RunSpeed,
                _ => 0f
            };
            if (Crouching) targetSpeed = CrouchSpeed;

            if (Grounded) _lastGroundedTargetSpeed = targetSpeed;

            var velocity = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z);
            var wishVel = (Orientation.right * RawMovementInput.x + Orientation.forward * RawMovementInput.y) * _lastGroundedTargetSpeed;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if ((wishSpeed != 0f) && (wishSpeed > MaxSpeed))
            {
                wishSpeed = MaxSpeed;
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
                MovementState.Walk => WalkAcceleration,
                MovementState.Run => RunAcceleration,
                _ => 0f
            };

            if (Crouching) acceleration = CrouchAcceleration;

            var accelSpeed = acceleration * Time.deltaTime * wishSpeed;
            accelSpeed = Mathf.Min(accelSpeed, addSpeed);

            return wishDir * accelSpeed;
        }

        private Vector2 AirAccelerate(Vector2 wishDir, float wishSpeed, float currentSpeed, Vector2 velocity)
        {
            var airWishSpeed = Mathf.Min(wishSpeed, AirSpeedCap);
            var addSpeed = airWishSpeed - currentSpeed;

            if (addSpeed <= 0) return Vector2.zero;

            var acceleration = Mathf.Min(velocity.magnitude * AirAccelerationGain, MaxAirAcceleration);
            var accelSpeed = acceleration * Time.deltaTime * airWishSpeed;
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _maxStepHeight / 2, new(Width, _maxStepHeight, Width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * _stepDownBufferHeight / 2, new(Width, _stepDownBufferHeight, Width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * _height, new(Width, 0.02f, Width));

            if (!Crouching)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + Vector3.up * (MaxStepHeight / 2 + CrouchHeight / 2), new(Width, CrouchHeight - MaxStepHeight, Width));
            }
        }
    }
}