using Fifbox.Input;
using Mirror;
using UnityEngine;
using ZSToolkit.ZSTUtility;
using UnityEngine.InputSystem;

namespace Fifbox.Game.Player.OnlinePlayer
{
    public class OnlinePlayerController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField, WithComponent(typeof(OnlinePlayerCamera))] private GameObject _mainCameraPrefab;
        [SerializeField] private OnlinePlayer _player;
        [SerializeField] private Transform _orientation;

        [Header("Inputs")]
        [SerializeField] private bool _autoBHop;

        private bool _holdingJump;

        private void Start()
        {
            if (!isLocalPlayer) return;

            InitializeCameras();
            InitializeInputs();
        }

        private void InitializeCameras()
        {
            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                Destroy(camera.gameObject);
            }

            var onlinePlayerCamera = Instantiate(_mainCameraPrefab, _orientation).GetComponent<OnlinePlayerCamera>();
            onlinePlayerCamera.Init(_player);
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            if (_autoBHop && _holdingJump) _player.Inputs.tryJump();
        }

        private void InitializeInputs()
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

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;

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

        private void MovePerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.moveVector = ctx.ReadValue<Vector2>();
        }

        private void MoveCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.moveVector = Vector2.zero;
        }

        private void RunPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToRun = true;
        }

        private void RunCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToRun = false;
        }

        private void CrouchPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToCrouch = true;
        }

        private void CrouchCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToCrouch = false;
        }

        private void JumpPerformed(InputAction.CallbackContext ctx)
        {
            _holdingJump = true;
            if (!_autoBHop) _player.Inputs.tryJump();
        }

        private void JumpCanceled(InputAction.CallbackContext ctx)
        {
            _holdingJump = false;
        }

        private void FastFlyPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToFlyFast = true;
        }

        private void FastFlyCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToFlyFast = false;
        }

        private void AscendPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToAscend = true;
        }

        private void AscendCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToAscend = false;
        }

        private void DescendPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToDescend = true;
        }

        private void DescendCanceled(InputAction.CallbackContext ctx)
        {
            _player.Inputs.wantsToDescend = false;
        }

        private void NoclipPerformed(InputAction.CallbackContext ctx)
        {
            _player.Inputs.nocliping = !_player.Inputs.nocliping;
        }
    }
}