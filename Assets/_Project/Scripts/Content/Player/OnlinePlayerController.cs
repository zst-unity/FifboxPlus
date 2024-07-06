using Fifbox.FrontEnd;
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
            FifboxGlobal.ActionAsset.Player.Enable();

            FifboxGlobal.ActionAsset.Player.Move.performed += ctx => _player.Inputs.moveVector = ctx.ReadValue<Vector2>();
            FifboxGlobal.ActionAsset.Player.Move.canceled += ctx => _player.Inputs.moveVector = Vector2.zero;

            FifboxGlobal.ActionAsset.Player.Run.performed += ctx => _player.Inputs.wantsToRun = true;
            FifboxGlobal.ActionAsset.Player.Run.canceled += ctx => _player.Inputs.wantsToRun = false;

            FifboxGlobal.ActionAsset.Player.Crouch.performed += ctx => _player.Inputs.wantsToCrouch = true;
            FifboxGlobal.ActionAsset.Player.Crouch.canceled += ctx => _player.Inputs.wantsToCrouch = false;

            FifboxGlobal.ActionAsset.Player.Jump.performed += ctx =>
            {
                _holdingJump = true;
                if (!_autoBHop) _player.Inputs.tryJump();
            };

            FifboxGlobal.ActionAsset.Player.Jump.canceled += ctx => _holdingJump = false;

            FifboxGlobal.ActionAsset.Player.FastFly.performed += ctx => _player.Inputs.wantsToFlyFast = true;
            FifboxGlobal.ActionAsset.Player.FastFly.canceled += ctx => _player.Inputs.wantsToFlyFast = false;

            FifboxGlobal.ActionAsset.Player.Ascend.performed += ctx => _player.Inputs.wantsToAscend = true;
            FifboxGlobal.ActionAsset.Player.Ascend.canceled += ctx => _player.Inputs.wantsToAscend = false;

            FifboxGlobal.ActionAsset.Player.Descend.performed += ctx => _player.Inputs.wantsToDescend = true;
            FifboxGlobal.ActionAsset.Player.Descend.canceled += ctx => _player.Inputs.wantsToDescend = false;

            FifboxGlobal.ActionAsset.Player.Noclip.performed += ctx => _player.Inputs.toggleNoclip();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            if (_autoBHop && _holdingJump) _player.Inputs.tryJump();
            var cameraInput = FifboxGlobal.ActionAsset.Player.Look.ReadValue<Vector2>();

            if (FifboxGlobal.ActionAsset.Player.Look.activeControl != null)
            {
                var deviceName = FifboxGlobal.ActionAsset.Player.Look.activeControl.device.name.ToLower();
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
            FifboxGlobal.ActionAsset.Player.Disable();
        }
    }
}