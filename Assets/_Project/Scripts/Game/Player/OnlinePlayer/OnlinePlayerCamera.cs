using Fifbox.Input;
using UnityEngine;

namespace Fifbox.Game.Player.OnlinePlayer
{
    public class OnlinePlayerCamera : MonoBehaviour
    {
        private OnlinePlayer _player;
        private bool _initialized;

        [Header("Height properties")]
        [SerializeField] private float _cameraDefaultHeight;
        [SerializeField] private float _cameraCrouchHeight;
        [SerializeField] private float _cameraHeightTransitionSpeed;

        private float _currentCameraHeight;
        private Vector3 _cameraEulerAngles;

        private void Awake()
        {
            _currentCameraHeight = _cameraDefaultHeight;
            gameObject.tag = "MainCamera";
            transform.localPosition = Vector3.up * _cameraDefaultHeight;
        }

        public void Init(OnlinePlayer player)
        {
            if (_initialized) return;
            _player = player;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            var cameraInput = FifboxActions.Asset.Player.Look.ReadValue<Vector2>();

            if (FifboxActions.Asset.Player.Look.activeControl != null)
            {
                var deviceName = FifboxActions.Asset.Player.Look.activeControl.device.name.ToLower();
                if (deviceName.Contains("controller") || deviceName.Contains("gameped") || deviceName.Contains("joystick"))
                {
                    cameraInput *= Time.deltaTime * 80;
                }
            }

            _cameraEulerAngles.y += cameraInput.x;
            _cameraEulerAngles.x = Mathf.Clamp(_cameraEulerAngles.x - cameraInput.y, -90f, 90f);

            _player.Inputs.setOrientationEulerAngles(new(_cameraEulerAngles.x, _cameraEulerAngles.y, 0f));
            transform.localRotation = Quaternion.Euler(_cameraEulerAngles.x, 0f, _cameraEulerAngles.z);

            var targetCameraHeight = _player.Grounded && _player.Crouching ? _cameraCrouchHeight : _cameraDefaultHeight;
            _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, targetCameraHeight, Time.deltaTime * _cameraHeightTransitionSpeed);
            transform.position = _player.transform.position + Vector3.up * _currentCameraHeight;
        }
    }
}