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

        [field: Header("Physics properties")]
        [field: SerializeField] public float Friction { get; private set; }

        [field: Header("Move properties")]
        [field: SerializeField] public float WalkSpeed { get; private set; }
        [field: SerializeField] public float Acceleration { get; private set; }
        [field: SerializeField] public float Deceleration { get; private set; }
        [field: SerializeField] public float MaxSpeed { get; private set; }

        [field: Header("Jump properties")]
        [field: SerializeField] public float WalkJumpForce { get; private set; }
        [field: SerializeField] public float JumpBufferTime { get; private set; }

        private float _jumpBufferTimer;

        [field: Header("Air handling")]
        [field: SerializeField] public float AirAcceleration { get; private set; }
        [field: SerializeField] public float AirSpeedCap { get; private set; }

        [field: Header("Ground handling")]
        [field: SerializeField] public LayerMask MapLayers { get; private set; }
        [field: SerializeField] public float MaxStepHeight { get; private set; }
        [field: SerializeField] public float StepDownBufferHeight { get; private set; }
        [field: SerializeField] public float MaxGroundInfoCheckDistance { get; private set; }

        [field: Space(9)]

        [field: SerializeField, ReadOnly] public bool Grounded { get; private set; }
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
                Collider.size = new Vector3(Width, Height - MaxStepHeight, Width);
                Collider.center = new Vector3(0, MaxStepHeight / 2, 0);
            }

            if (Center)
            {
                Center.localPosition = new Vector3(0, Height / 2, 0);
            }
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
        [SerializeField, ReadOnly] private Vector2 _wishDirection;

        public Vector2 RawMovementInput { get => _rawMovementInput; protected set => _rawMovementInput = value; }
        public bool WantsToRun { get => _wantsToRun; protected set => _wantsToRun = value; }
        public bool WantsToCrouch { get => _wantsToCrouch; protected set => _wantsToCrouch = value; }

        protected void TryJump()
        {
            if (!Grounded) return;
            Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, WalkJumpForce, Rigidbody.linearVelocity.z);
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            GroundCheck();
            MoveToGround();
            ApplyGravity();
            ApplyFriction();
            Movement();

            if (Input.GetKey(KeyCode.Space)) TryJump();
            OnUpdate();
        }

        private void GroundCheck()
        {
            var width = Width - 0.01f;

            var useBuffer = Rigidbody.linearVelocity.y <= 0f && Rigidbody.linearVelocity.y > -0.1f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (MaxStepHeight - StepDownBufferHeight) / 2
                : transform.position + Vector3.up * MaxStepHeight / 2;

            var groundedCheckSize = new Vector3(width, useBuffer ? MaxStepHeight + StepDownBufferHeight : MaxStepHeight, width);
            Grounded = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, MapLayers, QueryTriggerInteraction.Ignore);

            var groundInfoCheckPosition = transform.position + 2f * MaxStepHeight * Vector3.up;
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

        private void MoveToGround()
        {
            if (!Grounded || Rigidbody.linearVelocity.y > 0f) return;
            Rigidbody.MovePosition(new(Rigidbody.position.x, GroundHeight, Rigidbody.position.z));
        }

        private void ApplyGravity()
        {
            if (Grounded && Rigidbody.linearVelocity.y <= 0) Rigidbody.linearVelocity = new(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
            else Rigidbody.linearVelocity += Physics.gravity * Time.deltaTime;
        }

        private void ApplyFriction()
        {
            var speed = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z).magnitude;
            var drop = 0f;

            if (Grounded)
            {
                var control = speed < Deceleration ? Deceleration : speed;
                drop = control * Friction * Time.deltaTime;
            }

            var newSpeed = Mathf.Max(speed - drop, 0f);
            if (speed > 0.0f) newSpeed /= speed;

            var newVelocity = new Vector3(Rigidbody.linearVelocity.x * newSpeed, Rigidbody.linearVelocity.y, Rigidbody.linearVelocity.z * newSpeed);
            Rigidbody.linearVelocity = newVelocity;
        }

        private void Movement()
        {
            var wishDir = (Orientation.right * RawMovementInput.x + Orientation.forward * RawMovementInput.y).normalized;
            _wishDirection = new(wishDir.x, wishDir.z);

            var horizontalMovement = new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z);

            if (Grounded)
            {
                var currentSpeed = Vector2.Dot(horizontalMovement, _wishDirection);

                var maxAccel = 10 * MaxSpeed;
                var addSpeed = Mathf.Clamp(MaxSpeed - currentSpeed, 0, maxAccel * Time.deltaTime);

                Rigidbody.linearVelocity += new Vector3(_wishDirection.x, 0, _wishDirection.y) * addSpeed;
            }
            else
            {
                var wishSpeed = Mathf.Min(horizontalMovement.magnitude, AirSpeedCap, MaxSpeed);
                var currentSpeed = Vector3.Dot(Rigidbody.linearVelocity, wishDir);
                var addSpeed = wishSpeed - currentSpeed;

                if (addSpeed <= 0) return;

                var accelSpeed = AirAcceleration * wishSpeed * Time.deltaTime;
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                Rigidbody.linearVelocity += wishDir * accelSpeed;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * MaxStepHeight / 2, new(Width - 0.01f, MaxStepHeight, Width - 0.01f));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * StepDownBufferHeight / 2, new(Width - 0.01f, StepDownBufferHeight, Width - 0.01f));
        }
    }
}