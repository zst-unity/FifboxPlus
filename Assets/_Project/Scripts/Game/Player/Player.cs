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
        public float WidthForChecking => Config.width - 0.01f;

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

        [Header("Info")]
        [SerializeField, ReadOnly, AllowNesting] private PlayerInputsInfo _inputsInfo;
        public PlayerInputsInfo InputsInfo => inputs.Info;
        protected readonly PlayerInputs inputs = new();

        [field: SerializeField, ReadOnly, AllowNesting] public PlayerInfo Info { get; private set; } = new();

        public PlayerStateMachine<PlayerState, OnGroundState> StateMachine { get; private set; }

        protected abstract bool ShouldProcessPlayer { get; }
        public abstract int DefaultLayer { get; }

        private void Awake() => OnPlayerAwake();
        private void Start() => OnPlayerStart();
        private void OnDestroy() => OnPlayerDestroy();
        private void Update()
        {
            OnPlayerUpdate();
            _inputsInfo = InputsInfo;
        }

        private void LateUpdate() => OnPlayerLateUpdate();

        protected virtual void OnPlayerAwake()
        {
            StateMachine = new(this, inputs);

            Info.currentHeight = Config.fullHeight;
            Info.currentMaxStepHeight = Config.maxStepHeight;
            Info.currentStepDownBufferHeight = Config.stepDownBufferHeight;
        }

        protected virtual void OnPlayerStart()
        {
            gameObject.SetLayerForChildren(DefaultLayer);

            if (!ShouldProcessPlayer) return;

            inputs.setOrientationEulerAngles += SetOrientation;
            StateMachine.Start();
        }

        protected virtual void OnPlayerDestroy()
        {
            if (!ShouldProcessPlayer) return;

            inputs.setOrientationEulerAngles -= SetOrientation;
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

        private void SetOrientation(Vector3 eulerAngles)
        {
            if (!ShouldProcessPlayer) return;

            Info.fullOrientation = new(eulerAngles);
            Info.flatOrientation = new(new(0f, eulerAngles.y, 0f));
            Orientation.localRotation = Info.flatOrientation.quaternion;
        }

        private void CeilCheck()
        {
            if (!ShouldProcessPlayer) return;

            var ceiledCheckSize = new Vector3(WidthForChecking, 0.02f, WidthForChecking);
            var ceiledCheckPosition = transform.position + Vector3.up * Info.currentHeight;
            Info.touchingCeiling = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }

        private void GroundCheck()
        {
            if (!ShouldProcessPlayer) return;

            var useBuffer = Rigidbody.linearVelocity.Round(0.001f).y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (Info.currentMaxStepHeight - Info.currentStepDownBufferHeight) / 2
                : transform.position + Vector3.up * Info.currentMaxStepHeight / 2;

            Info.groundCheckSizeY = useBuffer ? Info.currentMaxStepHeight + Info.currentStepDownBufferHeight : Info.currentMaxStepHeight + 0.05f;
            var groundedCheckSize = new Vector3(WidthForChecking, Info.groundCheckSizeY, WidthForChecking);
            Info.touchingGround = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);

            var groundInfoCheckPosition = transform.position + (Info.currentHeight - Info.currentMaxStepHeight / 2) * Vector3.up;
            var groundInfoCheckSize = new Vector3(Config.width + 0.1f, Info.currentMaxStepHeight, Config.width + 0.1f);
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

            var normal = hit.normal.Round(0.001f);
            Info.ground = new(normal, Vector3.Angle(normal, Vector3.up), hit.point.y);
        }

        public void UpdateColliderAndCenter()
        {
            Collider.center = PlayerUtility.GetColliderCenter(Info.currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(Config.width, Info.currentHeight, Info.currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(Info.currentHeight);
        }

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

                Rigidbody.mass = Config.mass;
            }

            if (Collider)
            {
                Collider.isTrigger = false;

                Collider.center = PlayerUtility.GetColliderCenter(Config.maxStepHeight);
                Collider.size = PlayerUtility.GetColliderSize(Config.width, Config.fullHeight, Config.maxStepHeight);
            }

            if (Center)
            {
                Center.localPosition = PlayerUtility.GetCenterPosition(Config.fullHeight);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Info.currentMaxStepHeight / 2, new(Config.width, Info.currentMaxStepHeight, Config.width));

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube(transform.position - Vector3.up * Info.currentStepDownBufferHeight / 2, new(Config.width, Info.currentStepDownBufferHeight, Config.width));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * Info.currentHeight, new(Config.width, 0.02f, Config.width));

            Gizmos.color = Color.green;

            if (Application.isPlaying) return;
            var position = transform.position + Vector3.up * (Config.maxStepHeight / 2 + Config.crouchHeight / 2);
            var size = new Vector3(Config.width, Config.crouchHeight - Config.maxStepHeight, Config.width);
            Gizmos.DrawWireCube(position, size);
        }
    }
}