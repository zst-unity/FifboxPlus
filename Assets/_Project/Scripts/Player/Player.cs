using Mirror;
using UnityEngine;
using ZSToolkit.ZSTUtility.Extensions;

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

        [field: SerializeField] public float MaxSpeed { get; private set; }

        [field: Header("Jump properties")]
        [field: SerializeField] public float WalkJumpForce { get; private set; }
        [field: SerializeField] public float RunJumpForce { get; private set; }

        [field: Space(9)]

        [field: SerializeField] public float JumpBufferTime { get; private set; }
        [field: SerializeField, Tooltip("Debug purpose")] public bool AutoBHop { get; private set; }

        private float _jumpBufferTimer;

        [field: Header("Air handling")]
        [field: SerializeField] public float AirAccelerationGain { get; private set; }
        [field: SerializeField] public float MaxAirAcceleration { get; private set; }
        [field: SerializeField] public float AirSpeedCap { get; private set; }

        [field: Header("Ground handling")]
        [field: SerializeField] public LayerMask MapLayers { get; private set; }
        [field: SerializeField] public float MaxStepHeight { get; private set; }
        [field: SerializeField] public float StepDownBufferHeight { get; private set; }
        [field: SerializeField] public float MaxGroundInfoCheckDistance { get; private set; }

        private float _maxStepHeight;

        [field: Space(9)]

        [field: SerializeField, ReadOnly] public bool Grounded { get; private set; }
        [field: SerializeField, ReadOnly] public bool Ceiled { get; private set; }
        [field: SerializeField, ReadOnly] public Vector3 GroundNormal { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundAngle { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundHeight { get; private set; }
        [field: SerializeField, ReadOnly] public float PreviousGroundHeight { get; private set; }

        protected override void OnValidate()
        {
            base.OnValidate();

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
                UpdatePlayerCollider(MaxStepHeight);
            }

            if (Center)
            {
                Center.localPosition = new Vector3(0, Height / 2, 0);
            }
        }

        private void UpdatePlayerCollider(float maxStepHeight)
        {
            Collider.size = new Vector3(Width, Height - maxStepHeight, Width);
            Collider.center = new Vector3(0, maxStepHeight / 2, 0);
        }

        private void Awake()
        {
            _maxStepHeight = MaxStepHeight;
        }

        private void Start()
        {
            if (!isLocalPlayer) return;
            OnStart();
        }

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        [Header("Inputs")]
        [SerializeField, ReadOnly] private Vector2 _rawMovementInput;
        [SerializeField, ReadOnly] private bool _wantsToRun;
        [SerializeField, ReadOnly] private bool _wantsToCrouch;

        public Vector2 RawMovementInput { get => _rawMovementInput; protected set => _rawMovementInput = value; }
        public bool WantsToRun { get => _wantsToRun; protected set => _wantsToRun = value; }
        public bool WantsToCrouch { get => _wantsToCrouch; protected set => _wantsToCrouch = value; }

        protected void TryJump()
        {
            if (AutoBHop) return;

            ResetJumpBuffer();
        }

        private void ResetJumpBuffer()
        {
            _jumpBufferTimer = JumpBufferTime;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            CeilCheck();
            GroundCheck();

            OnUpdate();

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

        private void GroundCheck()
        {
            var width = Width - 0.01f;

            var useBuffer = Rigidbody.linearVelocity.y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (_maxStepHeight - StepDownBufferHeight) / 2
                : transform.position + Vector3.up * _maxStepHeight / 2;

            var groundedCheckSize = new Vector3(width, useBuffer ? _maxStepHeight + StepDownBufferHeight : _maxStepHeight, width);
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

        private void CeilCheck()
        {
            var width = Width - 0.01f;
            var ceiledCheckSize = new Vector3(width, 0.02f, width);
            var ceiledCheckPosition = transform.position + Vector3.up * Height;

            Ceiled = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, MapLayers, QueryTriggerInteraction.Ignore);
            UpdatePlayerCollider(Ceiled ? 0f : MaxStepHeight);
        }

        private void HandleJump()
        {
            if (AutoBHop && Input.GetKey(KeyCode.Space) && Grounded)
            {
                Jump();
                return;
            }

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
            var targetForce = WantsToRun && _rawMovementInput.magnitude > 0 ? RunJumpForce : WalkJumpForce;
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, targetForce, Rigidbody.linearVelocity.z);
        }

        private void ApplyGravity()
        {
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
                var deceleration = WantsToRun ? RunDeceleration : WalkDeceleration;
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
            var targetSpeed = _wantsToRun ? RunSpeed : WalkSpeed;
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

            if (Grounded)
            {
                var addSpeed = wishSpeed - currentSpeed;
                if (addSpeed <= 0) return;

                var acceleration = WantsToRun ? RunAcceleration : WalkAcceleration;

                var accelSpeed = acceleration * Time.deltaTime * wishSpeed;
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                velocity += wishDir * accelSpeed;
            }
            else
            {
                var airWishSpeed = Mathf.Min(wishSpeed, AirSpeedCap);
                var addSpeed = airWishSpeed - currentSpeed;

                if (addSpeed <= 0) return;

                var acceleration = Mathf.Min(velocity.magnitude * AirAccelerationGain, MaxAirAcceleration);
                var accelSpeed = acceleration * Time.deltaTime * airWishSpeed;
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                velocity += wishDir * accelSpeed;
            }

            Rigidbody.linearVelocity = new(velocity.x, Rigidbody.linearVelocity.y, velocity.y);
        }

        private void MoveToGround()
        {
            if (!Grounded || Ceiled || Rigidbody.linearVelocity.y > 0f) return;
            transform.position = new(transform.position.x, GroundHeight, transform.position.z);
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * MaxStepHeight / 2, new(Width - 0.01f, MaxStepHeight, Width - 0.01f));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * StepDownBufferHeight / 2, new(Width - 0.01f, StepDownBufferHeight, Width - 0.01f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Height, new(Width - 0.01f, 0.02f, Width - 0.01f));
        }
    }
}