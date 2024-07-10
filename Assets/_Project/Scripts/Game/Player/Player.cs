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

        [field: Header("References")]
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Orientation { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PlayerConfig _config;

        public PlayerConfig Config
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

                if (Config) Rigidbody.mass = Config.mass;
            }

            if (Collider)
            {
                Collider.isTrigger = false;

                if (Config)
                {
                    Collider.center = PlayerUtility.GetColliderCenter(Config.maxStepHeight);
                    Collider.size = PlayerUtility.GetColliderSize(Config.width, Config.fullHeight, Config.maxStepHeight);
                }
            }

            if (Center)
            {
                if (Config) Center.localPosition = PlayerUtility.GetCenterPosition(Config.fullHeight);
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
            Data.currentMaxStepHeight = Config.maxStepHeight;
            Data.currentHeight = Config.fullHeight;
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

            CeilCheck();
            GroundCheck();

            StateMachine.Update();
        }

        protected virtual void OnPlayerLateUpdate()
        {
            if (!ShouldProcessPlayer) return;

            GroundCheck();
            StateMachine.LateUpdate();
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

        private void CeilCheck()
        {
            if (!ShouldProcessPlayer) return;

            var ceiledCheckSize = new Vector3(Config.width, 0.02f, Config.width);
            var ceiledCheckPosition = transform.position + Vector3.up * Data.currentHeight;
            Data.touchingCeiling = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }

        private void GroundCheck()
        {
            if (!ShouldProcessPlayer) return;

            var useBuffer = Rigidbody.linearVelocity.Round(0.001f).y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (Data.currentMaxStepHeight - Config.stepDownBufferHeight) / 2
                : transform.position + Vector3.up * Data.currentMaxStepHeight / 2;

            Data.groundCheckSizeY = useBuffer ? Data.currentMaxStepHeight + Config.stepDownBufferHeight : Data.currentMaxStepHeight + 0.05f;
            var groundedCheckSize = new Vector3(Config.width, Data.groundCheckSizeY, Config.width);
            Data.touchingGround = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);

            var groundInfoCheckPosition = transform.position + (Data.currentHeight - Data.currentMaxStepHeight / 2) * Vector3.up;
            var groundInfoCheckSize = new Vector3(Config.width + 0.1f, Data.currentMaxStepHeight, Config.width + 0.1f);
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
            Data.groundNormal = hit.normal.Round(0.001f);
            Data.groundAngle = Vector3.Angle(Data.groundNormal, Vector3.up);
        }

        public void UpdateColliderAndCenter()
        {
            Collider.center = PlayerUtility.GetColliderCenter(Data.currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(Config.width, Data.currentHeight, Data.currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(Data.currentHeight);
        }

        public void ApplyGravity()
        {
            Rigidbody.linearVelocity += Config.gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        public (Vector3 wishVel, float wishSpeed, Vector2 wishDir) GetWishValues(float wishVelMultiplier = 1f)
        {
            var wishVel = (Orientation.right * Inputs.moveVector.x + Orientation.forward * Inputs.moveVector.y) * wishVelMultiplier;
            var wishSpeed = wishVel.magnitude;
            var wishDir = new Vector2(wishVel.x, wishVel.z).normalized;

            if (wishSpeed != 0f && (wishSpeed > Config.maxSpeed))
            {
                wishVel *= _config.maxSpeed / wishSpeed;
                wishSpeed = Config.maxSpeed;
            }

            return (wishVel, wishSpeed, wishDir);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Data.currentMaxStepHeight / 2, new(Config.width, Data.currentMaxStepHeight, Config.width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * Config.stepDownBufferHeight / 2, new(Config.width, Config.stepDownBufferHeight, Config.width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Data.currentHeight, new(Config.width, 0.02f, Config.width));

            Gizmos.color = Color.green;

            if (Application.isPlaying) return;
            var position = transform.position + Vector3.up * (Config.maxStepHeight / 2 + Config.crouchHeight / 2);
            var size = new Vector3(Config.width, Config.crouchHeight - Config.maxStepHeight, Config.width);
            Gizmos.DrawWireCube(position, size);
        }
    }
}