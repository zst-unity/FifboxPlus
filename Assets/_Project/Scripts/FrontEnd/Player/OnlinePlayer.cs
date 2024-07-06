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

            FifboxGlobal.ActionAsset.Player.Move.performed += ctx => Inputs.moveVector = ctx.ReadValue<Vector2>();
            FifboxGlobal.ActionAsset.Player.Move.canceled += ctx => Inputs.moveVector = Vector2.zero;

            FifboxGlobal.ActionAsset.Player.Run.performed += ctx => Inputs.wantsToRun = true;
            FifboxGlobal.ActionAsset.Player.Run.canceled += ctx => Inputs.wantsToRun = false;

            FifboxGlobal.ActionAsset.Player.Crouch.performed += ctx => Inputs.wantsToCrouch = true;
            FifboxGlobal.ActionAsset.Player.Crouch.canceled += ctx => Inputs.wantsToCrouch = false;

            FifboxGlobal.ActionAsset.Player.Jump.performed += ctx =>
            {
                _holdingJump = true;
                if (!_autoBHop) Inputs.tryJump();
            };

            FifboxGlobal.ActionAsset.Player.Jump.canceled += ctx => _holdingJump = false;

            FifboxGlobal.ActionAsset.Player.FastFly.performed += ctx => Inputs.wantsToFlyFast = true;
            FifboxGlobal.ActionAsset.Player.FastFly.canceled += ctx => Inputs.wantsToFlyFast = false;

            FifboxGlobal.ActionAsset.Player.Ascend.performed += ctx => Inputs.wantsToAscend = true;
            FifboxGlobal.ActionAsset.Player.Ascend.canceled += ctx => Inputs.wantsToAscend = false;

            FifboxGlobal.ActionAsset.Player.Descend.performed += ctx => Inputs.wantsToDescend = true;
            FifboxGlobal.ActionAsset.Player.Descend.canceled += ctx => Inputs.wantsToDescend = false;

            FifboxGlobal.ActionAsset.Player.Noclip.performed += ctx => Inputs.toggleNoclip();
        }

        protected override void OnUpdate()
        {
            if (_autoBHop && _holdingJump) Inputs.tryJump();
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

            Inputs.orientationEulerAngles = new(_cameraRotX, _cameraRotY, 0f);
            Camera.main.transform.localRotation = Quaternion.Euler(_cameraRotX, 0f, 0f);

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