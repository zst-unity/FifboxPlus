using Mirror;
using UnityEngine;
using NaughtyAttributes;
using Fifbox.ScriptableObjects.Configs;
using Fifbox.ScriptableObjects;

using ReadOnlyAttribute = NaughtyAttributes.ReadOnlyAttribute;
using ZSToolkit.ZSTUtility.Extensions;
using Fifbox.Game.Player.StateMachine;
using Fifbox.Game.Player.StateMachine.States;
using System;

namespace Fifbox.Game.Player
{
    [Serializable]
    public struct PlayerMapInfo
    {
        public Vector3 normal;
        public float angle;
        public float height;
    }

    [RequireComponent(typeof(Rigidbody))]
    public abstract class Player : NetworkBehaviour
    {
        public const float MAX_GROUND_INFO_CHECK_DISTANCE = 100f;
        public float WidthForChecking => Config.width - 0.001f;

        public PlayerStateMachine<PlayerState, OnGroundState, PlayerStatesData> StateMachine { get; private set; }

        [Header("References")]
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
        protected readonly PlayerInputsController inputs = new();

        [SerializeField, ReadOnly, AllowNesting] private PlayerHeightsInfo _heightsInfo;
        public PlayerHeightsInfo HeightsInfo => heights.Info;
        protected readonly PlayerHeightsController heights = new();

        [field: Space(9)]

        [field: SerializeField, ReadOnly, AllowNesting] public PlayerMapInfo GroundInfo { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool TouchingGround { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool TouchingCeiling { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public float GroundCheckSizeY { get; private set; }

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
            StateMachine = new(this, inputs, heights);

            heights.CurrentHeight = Config.fullHeight;
            heights.CurrentMaxStepHeight = Config.maxStepHeight;
            heights.CurrentStepDownBufferHeight = Config.stepDownBufferHeight;
        }

        protected virtual void OnPlayerStart()
        {
            gameObject.SetLayerForChildren(DefaultLayer);

            if (!ShouldProcessPlayer) return;

            inputs.OnOrientationEulerAnglesChanged += OnOrientationChanged;
            StateMachine.Start();
        }

        protected virtual void OnPlayerDestroy()
        {
            if (!ShouldProcessPlayer) return;

            inputs.OnOrientationEulerAnglesChanged -= OnOrientationChanged;
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

        private void OnOrientationChanged(Vector3 eulerAngles)
        {
            if (!ShouldProcessPlayer) return;

            Orientation.localRotation = inputs.FlatOrientation.quaternion;
        }

        private void CeilCheck()
        {
            if (!ShouldProcessPlayer) return;

            var ceiledCheckSize = new Vector3(WidthForChecking, 0.02f, WidthForChecking);
            var ceiledCheckPosition = transform.position + Vector3.up * HeightsInfo.currentHeight;
            TouchingCeiling = Physics.CheckBox(ceiledCheckPosition, ceiledCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);
        }

        private void GroundCheck()
        {
            if (!ShouldProcessPlayer) return;

            var useBuffer = Rigidbody.linearVelocity.Round(0.001f).y == 0f;
            var groundedCheckPosition = useBuffer
                ? transform.position + Vector3.up * (HeightsInfo.currentMaxStepHeight - HeightsInfo.currentStepDownBufferHeight) / 2
                : transform.position + Vector3.up * HeightsInfo.currentMaxStepHeight / 2;

            GroundCheckSizeY = useBuffer ? HeightsInfo.currentMaxStepHeight + HeightsInfo.currentStepDownBufferHeight : HeightsInfo.currentMaxStepHeight + 0.05f;
            var groundedCheckSize = new Vector3(WidthForChecking, GroundCheckSizeY, WidthForChecking);
            TouchingGround = Physics.CheckBox(groundedCheckPosition, groundedCheckSize / 2f, Quaternion.identity, FifboxLayers.GroundLayers);

            var groundInfoCheckPosition = transform.position + (HeightsInfo.currentHeight - HeightsInfo.currentMaxStepHeight / 2) * Vector3.up;
            var groundInfoCheckSize = new Vector3(WidthForChecking, HeightsInfo.currentMaxStepHeight, WidthForChecking);
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
            GroundInfo = new()
            {
                normal = normal,
                angle = Vector3.Angle(Vector3.up, normal),
                height = hit.point.y
            };
        }

        public void UpdateColliderAndCenter()
        {
            Collider.center = PlayerUtility.GetColliderCenter(HeightsInfo.currentMaxStepHeight);
            Collider.size = PlayerUtility.GetColliderSize(Config.width, HeightsInfo.currentHeight, HeightsInfo.currentMaxStepHeight);
            Center.localPosition = PlayerUtility.GetCenterPosition(HeightsInfo.currentHeight);
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
            Gizmos.DrawWireCube
            (
                transform.position + Vector3.up * HeightsInfo.currentMaxStepHeight / 2,
                new(Config.width, HeightsInfo.currentMaxStepHeight, Config.width)
            );

            Gizmos.color = Color.blue - Color.black * 0.65f;
            Gizmos.DrawWireCube
            (
                transform.position - Vector3.up * HeightsInfo.currentStepDownBufferHeight / 2,
                new(Config.width, HeightsInfo.currentStepDownBufferHeight, Config.width)
            );

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube
            (
                transform.position + Vector3.up * HeightsInfo.currentHeight,
                new(Config.width, 0.02f, Config.width)
            );

            if (Application.isPlaying) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube
            (
                transform.position + Vector3.up * (Config.maxStepHeight / 2 + Config.crouchHeight / 2),
                new(Config.width, Config.crouchHeight - Config.maxStepHeight, Config.width)
            );
        }
    }
}