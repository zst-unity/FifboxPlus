using Mirror;
using UnityEngine;

namespace Fifbox.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Player : NetworkBehaviour
    {
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        public Vector2 WishDirection { get => _rawMovementInput; protected set => _rawMovementInput = value; }
        public bool WantsToRun { get => _wantsToRun; protected set => _wantsToRun = value; }
        public bool WantsToCrouch { get => _wantsToCrouch; protected set => _wantsToCrouch = value; }

        private Vector2 _rawMovementInput;
        private bool _wantsToRun;
        private bool _wantsToCrouch;

        protected void TryJump()
        {

        }

        [field: Header("Objects")]
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Orientation { get; private set; }

        [field: Header("Body properties")]
        [field: SerializeField] public float Mass { get; private set; }
        [field: SerializeField] public float Width { get; private set; }
        [field: SerializeField] public float Height { get; private set; }

        [field: Header("Move properties")]
        [field: SerializeField] public float WalkSpeed { get; private set; }

        [field: Header("Stair handling")]
        [field: SerializeField] public float MaxStepHeight { get; private set; }

        [field: Header("Map detection")]
        [field: SerializeField] public LayerMask MapLayers { get; private set; }

        [field: Space(9)]

        [field: SerializeField, ReadOnly] public bool Grounded { get; private set; }
        [field: SerializeField, ReadOnly] public Vector3 GroundNormal { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundAngle { get; private set; }
        [field: SerializeField, ReadOnly] public float GroundHeight { get; private set; }

        [Header("Velocities")]
        [ReadOnly] public Vector3 velocity;

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

        private void Update()
        {
            if (!isLocalPlayer) return;
            GroundCheck();
            HandleStairs();

            HandleGravity();
            OnUpdate();
        }

        private void GroundCheck()
        {
            var position = transform.position + 2f * MaxStepHeight * Vector3.up;
            var size = new Vector3(Width - 0.01f, MaxStepHeight, Width - 0.01f);
            Grounded = Physics.BoxCast(position, size / 2, Vector3.down, out var hit, Quaternion.identity, MaxStepHeight * 1.5f, MapLayers, QueryTriggerInteraction.Ignore);

            if (Grounded)
            {
                GroundNormal = hit.normal;
                GroundAngle = Vector3.Angle(GroundNormal, Vector3.up);
                GroundHeight = hit.point.y;
            }
            else
            {
                GroundNormal = Vector3.zero;
                GroundAngle = 0;
                GroundHeight = 0f;
            }
        }

        private void HandleGravity()
        {
            if (Grounded && velocity.y <= 0) velocity.y = 0;
            else velocity += Physics.gravity * Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            var direction = Orientation.right * WishDirection.x + Orientation.forward * WishDirection.y;
            Rigidbody.linearVelocity = new Vector3
            (
                direction.x * WalkSpeed,
                0,
                direction.z * WalkSpeed
            ) + velocity;
        }

        private void HandleStairs()
        {
            if (!Grounded) return;
            Rigidbody.MovePosition(new(Rigidbody.position.x, GroundHeight, Rigidbody.position.z));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * MaxStepHeight / 2, new(Width - 0.01f, MaxStepHeight, Width - 0.01f));
        }
    }
}