using Fifbox.MiddleEnd;
using UnityEngine;

namespace Fifbox.FrontEnd.Player
{
    public sealed class OnlinePlayer : Player
    {
        [Header("Objects")]
        [SerializeField] private GameObject _mainCameraPrefab;
        [SerializeField] private GameObject[] _playerModels;

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
        private float _cameraRotZ;

        protected override void OnStart()
        {
            Cursor.lockState = CursorLockMode.Locked;

            foreach (var model in _playerModels)
            {
                model.SetActive(false);
            }

            _initialLayer = 6;

            // удаляем все камеры на сцене чтобы приколов не было
            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                Destroy(camera.gameObject);
            }

            InitializeCameras();
            InitializeInputs();
        }

        private void InitializeCameras()
        {
            Instantiate(_mainCameraPrefab, Orientation);
            Camera.main.transform.localPosition = Vector3.up * _cameraDefaultHeight;
        }

        private void InitializeInputs()
        {
            FifboxGlobal.ActionAsset.Player.Enable();

            FifboxGlobal.ActionAsset.Player.Move.performed += ctx => MoveVector = ctx.ReadValue<Vector2>();
            FifboxGlobal.ActionAsset.Player.Move.canceled += ctx => MoveVector = Vector2.zero;

            FifboxGlobal.ActionAsset.Player.Run.performed += ctx => WantsToRun = true;
            FifboxGlobal.ActionAsset.Player.Run.canceled += ctx => WantsToRun = false;

            FifboxGlobal.ActionAsset.Player.Crouch.performed += ctx => WantsToCrouch = true;
            FifboxGlobal.ActionAsset.Player.Crouch.canceled += ctx => WantsToCrouch = false;

            FifboxGlobal.ActionAsset.Player.Jump.performed += ctx =>
            {
                _holdingJump = true;
                if (!_autoBHop) TryJump();
            };

            FifboxGlobal.ActionAsset.Player.Jump.canceled += ctx => _holdingJump = false;

            FifboxGlobal.ActionAsset.Player.FastFly.performed += ctx => WantsToFlyFast = true;
            FifboxGlobal.ActionAsset.Player.FastFly.canceled += ctx => WantsToFlyFast = false;

            FifboxGlobal.ActionAsset.Player.Ascend.performed += ctx => WantsToAscend = true;
            FifboxGlobal.ActionAsset.Player.Ascend.canceled += ctx => WantsToAscend = false;

            FifboxGlobal.ActionAsset.Player.Descend.performed += ctx => WantsToDescend = true;
            FifboxGlobal.ActionAsset.Player.Descend.canceled += ctx => WantsToDescend = false;

            FifboxGlobal.ActionAsset.Player.Noclip.performed += ctx => ToggleNoclip();
        }

        protected override void OnUpdate()
        {
            if (_autoBHop && _holdingJump) TryJump();
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
            VerticalOrientation = _cameraRotX;

            Orientation.localRotation = Quaternion.Euler(0f, _cameraRotY, 0f);
            Camera.main.transform.localRotation = Quaternion.Euler(_cameraRotX, 0f, _cameraRotZ);

            var targetCameraHeight = Grounded && Crouching ? _cameraCrouchHeight : _cameraDefaultHeight;
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