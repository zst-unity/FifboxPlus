using Mirror;
using TMPro;
using UnityEngine;

namespace Fifbox.UI
{
    public class SpeedDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _speedText;
        private Rigidbody _localPlayerRigidbody;

        private const float M2HU = 52.493438320209973753280839895013f;

        private void Update()
        {
            if (!NetworkClient.localPlayer) return;

            if (!_localPlayerRigidbody)
            {
                _localPlayerRigidbody = NetworkClient.localPlayer.GetComponent<Rigidbody>();
                if (!_localPlayerRigidbody) return;
            }

            var unitySpeed = new Vector2(_localPlayerRigidbody.linearVelocity.x, _localPlayerRigidbody.linearVelocity.z).magnitude;
            unitySpeed = Mathf.Round(unitySpeed * 100f) / 100f;

            var hammerSpeed = unitySpeed * M2HU;
            hammerSpeed = Mathf.Round(hammerSpeed * 100f) / 100f;

            _speedText.text = $"moving <color=yellow>{unitySpeed} m/s</color> or <color=yellow>{hammerSpeed} hu/s</color>";
        }
    }
}
