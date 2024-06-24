using Fifbox.InputActions;
using UnityEngine;

namespace Fifbox.Player
{
    public class OnlinePlayer : Player
    {
        [Header("Objects")]
        [SerializeField] private GameObject _mainCameraPrefab;
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private GameObject[] _playerModels;

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
            Instantiate(_mainCameraPrefab, _cameraHolder);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        private void InitializeInputs()
        {
            _actions = new();
            _actions.Enable();

            _actions.Player.Move.performed += ctx => WishDirection = ctx.ReadValue<Vector2>();
            _actions.Player.Move.canceled += ctx => WishDirection = Vector2.zero;

            _actions.Player.Run.performed += ctx => WantsToRun = true;
            _actions.Player.Run.canceled += ctx => WantsToRun = false;

            _actions.Player.Jump.performed += ctx => TryJump();
        }

        protected override void OnUpdate()
        {
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
        }

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;
            _actions.Disable();
        }
    }
}