using Fifbox.InputActions;
using UnityEngine;

namespace Fifbox.Player
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

        private FifboxActions _actions;
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

            // меняем слой с Player на LocalPlayer
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = 6;
            }

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
            _actions = new();
            _actions.Enable();

            _actions.Player.Move.performed += ctx => RawMovementInput = ctx.ReadValue<Vector2>();
            _actions.Player.Move.canceled += ctx => RawMovementInput = Vector2.zero;

            _actions.Player.Run.performed += ctx => WantsToRun = true;
            _actions.Player.Run.canceled += ctx => WantsToRun = false;

            _actions.Player.Crouch.performed += ctx => WantsToCrouch = true;
            _actions.Player.Crouch.canceled += ctx => WantsToCrouch = false;

            _actions.Player.Jump.performed += ctx =>
            {
                _holdingJump = true;

                if (!_autoBHop) TryJump();
            };

            _actions.Player.Jump.canceled += ctx => _holdingJump = false;
        }

        protected override void OnUpdate()
        {
            if (_autoBHop && _holdingJump) TryJump();
            var cameraInput = _actions.Player.Look.ReadValue<Vector2>();

            if (_actions.Player.Look.activeControl != null)
            {
                var deviceName = _actions.Player.Look.activeControl.device.name.ToLower();
                if (deviceName.Contains("controller") || deviceName.Contains("gameped") || deviceName.Contains("joystick"))
                {
                    cameraInput *= Time.deltaTime * 80;
                }
            }

            _cameraRotY += cameraInput.x;
            _cameraRotX = Mathf.Clamp(_cameraRotX - cameraInput.y, -90f, 90f);

            Orientation.localRotation = Quaternion.Euler(0f, _cameraRotY, 0f);
            Camera.main.transform.localRotation = Quaternion.Euler(_cameraRotX, 0f, _cameraRotZ);

            var targetCameraHeight = Grounded && Crouching ? _cameraCrouchHeight : _cameraDefaultHeight;
            _cameraHeight = Mathf.Lerp(_cameraHeight, targetCameraHeight, Time.deltaTime * _cameraHeightTransitionSpeed);
            Camera.main.transform.position = transform.position + Vector3.up * _cameraHeight;
        }

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;
            _actions.Disable();
        }
    }
}