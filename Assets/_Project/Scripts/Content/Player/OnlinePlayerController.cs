using Fifbox.Input;
using Mirror;
using UnityEngine;

namespace Fifbox.Content.Player
{
    public class OnlinePlayerController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Player _player;
        [SerializeField] private Transform _orientation;
        [SerializeField] private GameObject _mainCameraPrefab;

        [Header("Camera")]
        [SerializeField] private float _cameraDefaultHeight;
        [SerializeField] private float _cameraCrouchHeight;
        [SerializeField] private float _cameraHeightTransitionSpeed;

        private float _cameraHeight;

        [Header("Inputs")]
        [SerializeField] private bool _autoBHop;

        private bool _holdingJump;

        private float _cameraRotX;
        private float _cameraRotY;

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
            Instantiate(_mainCameraPrefab, _orientation);
            Camera.main.transform.localPosition = Vector3.up * _cameraDefaultHeight;
        }

        private void InitializeInputs()
        {
            FifboxActions.Asset.Player.Enable();

            FifboxActions.Asset.Player.Move.performed += ctx => _player.Inputs.moveVector = ctx.ReadValue<Vector2>();
            FifboxActions.Asset.Player.Move.canceled += ctx => _player.Inputs.moveVector = Vector2.zero;

            FifboxActions.Asset.Player.Run.performed += ctx => _player.Inputs.wantsToRun = true;
            FifboxActions.Asset.Player.Run.canceled += ctx => _player.Inputs.wantsToRun = false;

            FifboxActions.Asset.Player.Crouch.performed += ctx => _player.Inputs.wantsToCrouch = true;
            FifboxActions.Asset.Player.Crouch.canceled += ctx => _player.Inputs.wantsToCrouch = false;

            FifboxActions.Asset.Player.Jump.performed += ctx =>
            {
                _holdingJump = true;
                if (!_autoBHop) _player.Inputs.tryJump();
            };

            FifboxActions.Asset.Player.Jump.canceled += ctx => _holdingJump = false;

            FifboxActions.Asset.Player.FastFly.performed += ctx => _player.Inputs.wantsToFlyFast = true;
            FifboxActions.Asset.Player.FastFly.canceled += ctx => _player.Inputs.wantsToFlyFast = false;

            FifboxActions.Asset.Player.Ascend.performed += ctx => _player.Inputs.wantsToAscend = true;
            FifboxActions.Asset.Player.Ascend.canceled += ctx => _player.Inputs.wantsToAscend = false;

            FifboxActions.Asset.Player.Descend.performed += ctx => _player.Inputs.wantsToDescend = true;
            FifboxActions.Asset.Player.Descend.canceled += ctx => _player.Inputs.wantsToDescend = false;

            FifboxActions.Asset.Player.Noclip.performed += ctx => _player.Inputs.toggleNoclip();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            if (_autoBHop && _holdingJump) _player.Inputs.tryJump();
            var cameraInput = FifboxActions.Asset.Player.Look.ReadValue<Vector2>();

            if (FifboxActions.Asset.Player.Look.activeControl != null)
            {
                var deviceName = FifboxActions.Asset.Player.Look.activeControl.device.name.ToLower();
                if (deviceName.Contains("controller") || deviceName.Contains("gameped") || deviceName.Contains("joystick"))
                {
                    cameraInput *= Time.deltaTime * 80;
                }
            }

            _cameraRotY += cameraInput.x;
            _cameraRotX = Mathf.Clamp(_cameraRotX - cameraInput.y, -90f, 90f);

            _player.Inputs.orientationEulerAngles = new(_cameraRotX, _cameraRotY, 0f);
            Camera.main.transform.localRotation = Quaternion.Euler(_cameraRotX, 0f, 0f);

            var targetCameraHeight = _player.Grounded && _player.Crouching ? _cameraCrouchHeight : _cameraDefaultHeight;
            _cameraHeight = Mathf.Lerp(_cameraHeight, targetCameraHeight, Time.deltaTime * _cameraHeightTransitionSpeed);
            Camera.main.transform.position = transform.position + Vector3.up * _cameraHeight;
        }

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;
            FifboxActions.Asset.Player.Disable();
        }
    }
}