using Fifbox.Input;
using Fifbox.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using ZSToolkit.ZSTUtility;

namespace Fifbox.Game.Player.OnlinePlayer
{
    public class OnlinePlayer : Player
    {
        protected override bool ShouldProcessPlayer => isLocalPlayer;
        public override int DefaultLayer => FifboxLayers.LocalPlayerLayer.Index;

        [Header("Online Player References")]
        [SerializeField, WithComponent(typeof(OnlinePlayerCamera))] private GameObject _mainCameraPrefab;

        [Header("Online Player Inputs")]
        [SerializeField] private bool _autoBHop;
        private bool _holdingJump;

        protected override void OnPlayerStart()
        {
            if (isLocalPlayer) LocalStart();

            base.OnPlayerStart();
        }

        private void LocalStart()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Model.SetActive(false);

            InitCameras();
            InitActions();
        }

        private void InitCameras()
        {
            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                Destroy(camera.gameObject);
            }

            var onlinePlayerCamera = Instantiate(_mainCameraPrefab, Orientation).GetComponent<OnlinePlayerCamera>();
            onlinePlayerCamera.Init(this, inputs);
        }

        private void InitActions()
        {
            FifboxActions.Asset.Player.Enable();

            FifboxActions.Asset.Player.Move.performed += MovePerformed;
            FifboxActions.Asset.Player.Move.canceled += MoveCanceled;

            FifboxActions.Asset.Player.Run.performed += RunPerformed;
            FifboxActions.Asset.Player.Run.canceled += RunCanceled;

            FifboxActions.Asset.Player.Crouch.performed += CrouchPerformed;
            FifboxActions.Asset.Player.Crouch.canceled += CrouchCanceled;

            FifboxActions.Asset.Player.Jump.performed += JumpPerformed;
            FifboxActions.Asset.Player.Jump.canceled += JumpCanceled;

            FifboxActions.Asset.Player.FastFly.performed += FastFlyPerformed;
            FifboxActions.Asset.Player.FastFly.canceled += FastFlyCanceled;

            FifboxActions.Asset.Player.Ascend.performed += AscendPerformed;
            FifboxActions.Asset.Player.Ascend.canceled += AscendCanceled;

            FifboxActions.Asset.Player.Descend.performed += DescendPerformed;
            FifboxActions.Asset.Player.Descend.canceled += DescendCanceled;

            FifboxActions.Asset.Player.Noclip.performed += NoclipPerformed;
        }

        protected override void OnPlayerDestroy()
        {
            if (isLocalPlayer) ResetActions();

            base.OnPlayerDestroy();
        }

        private void ResetActions()
        {
            FifboxActions.Asset.Player.Disable();

            FifboxActions.Asset.Player.Move.performed -= MovePerformed;
            FifboxActions.Asset.Player.Move.canceled -= MoveCanceled;

            FifboxActions.Asset.Player.Run.performed -= RunPerformed;
            FifboxActions.Asset.Player.Run.canceled -= RunCanceled;

            FifboxActions.Asset.Player.Crouch.performed -= CrouchPerformed;
            FifboxActions.Asset.Player.Crouch.canceled -= CrouchCanceled;

            FifboxActions.Asset.Player.Jump.performed -= JumpPerformed;
            FifboxActions.Asset.Player.Jump.canceled -= JumpCanceled;

            FifboxActions.Asset.Player.FastFly.performed -= FastFlyPerformed;
            FifboxActions.Asset.Player.FastFly.canceled -= FastFlyCanceled;

            FifboxActions.Asset.Player.Ascend.performed -= AscendPerformed;
            FifboxActions.Asset.Player.Ascend.canceled -= AscendCanceled;

            FifboxActions.Asset.Player.Descend.performed -= DescendPerformed;
            FifboxActions.Asset.Player.Descend.canceled -= DescendCanceled;

            FifboxActions.Asset.Player.Noclip.performed -= NoclipPerformed;
        }

        protected override void OnPlayerUpdate()
        {
            base.OnPlayerUpdate();

            if (isLocalPlayer && _autoBHop && _holdingJump) inputs.tryJump();
        }

        private void MovePerformed(InputAction.CallbackContext ctx)
        {
            inputs.MoveVector = ctx.ReadValue<Vector2>();
        }

        private void MoveCanceled(InputAction.CallbackContext ctx)
        {
            inputs.MoveVector = Vector2.zero;
        }

        private void RunPerformed(InputAction.CallbackContext ctx)
        {
            inputs.WantsToRun = true;
        }

        private void RunCanceled(InputAction.CallbackContext ctx)
        {
            inputs.WantsToRun = false;
        }

        private void CrouchPerformed(InputAction.CallbackContext ctx)
        {
            inputs.WantsToCrouch = true;
        }

        private void CrouchCanceled(InputAction.CallbackContext ctx)
        {
            inputs.WantsToCrouch = false;
        }

        private void JumpPerformed(InputAction.CallbackContext ctx)
        {
            _holdingJump = true;
            if (!_autoBHop) inputs.tryJump();
        }

        private void JumpCanceled(InputAction.CallbackContext ctx)
        {
            _holdingJump = false;
        }

        private void FastFlyPerformed(InputAction.CallbackContext ctx)
        {
            inputs.WantsToFlyFast = true;
        }

        private void FastFlyCanceled(InputAction.CallbackContext ctx)
        {
            inputs.WantsToFlyFast = false;
        }

        private void AscendPerformed(InputAction.CallbackContext ctx)
        {
            inputs.WantsToAscend = true;
        }

        private void AscendCanceled(InputAction.CallbackContext ctx)
        {
            inputs.WantsToAscend = false;
        }

        private void DescendPerformed(InputAction.CallbackContext ctx)
        {
            inputs.WantsToDescend = true;
        }

        private void DescendCanceled(InputAction.CallbackContext ctx)
        {
            inputs.WantsToDescend = false;
        }

        private void NoclipPerformed(InputAction.CallbackContext ctx)
        {
            inputs.Nocliping = !inputs.Nocliping;
        }
    }
}