using Fifbox.Player;
using TMPro;
using UnityEngine;

namespace Fifbox.UI.GameplayUI
{
    public class DebugStatistics : MonoBehaviour, IGameplayUIElement
    {
        public static DebugStatistics Singleton { get; private set; }
        public bool Visible { get; private set; }

        [SerializeField] private TMP_Text _statisticsText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("FPS Counter")]
        [SerializeField, Range(0f, 1f)] private float _expSmoothingFactor;
        [SerializeField] private float _refreshFrequency;

        private float _timeSinceUpdate = 0f;
        private float _averageFps = 1f;
        private float _fps;

        public string ID => "debug_statistics";
        public GameObject GameObject => gameObject;

        public void ResetElement()
        {
            Singleton = this;
            _timeSinceUpdate = 0f;
            _averageFps = 60f;
            _fps = 60f;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void Update()
        {
            var statisticsText = "";
            statisticsText += GetSpeedCounters() + "\n";
            statisticsText += GetFPSCounter() + "\n";
            _statisticsText.text = statisticsText;
        }

        private string GetFPSCounter()
        {
            _averageFps = _expSmoothingFactor * _averageFps + (1f - _expSmoothingFactor) * 1f / Time.unscaledDeltaTime;

            if (_timeSinceUpdate < _refreshFrequency)
            {
                _timeSinceUpdate += Time.deltaTime;
            }
            else
            {
                _fps = Mathf.Round(_averageFps);
                _timeSinceUpdate = 0f;
            }

            return $"FPS: {_fps}";
        }

        private string GetSpeedCounters()
        {
            if (!OnlinePlayer.LocalPlayer) return "Player not spawned";

            var unitySpeed = OnlinePlayer.LocalPlayer.Rigidbody.linearVelocity.magnitude;
            unitySpeed = Mathf.Round(unitySpeed * 100f) / 100f;

            var hammerSpeed = unitySpeed / 0.01905f;
            hammerSpeed = Mathf.Round(hammerSpeed * 100f) / 100f;

            return $"Unity speed: {unitySpeed} m/s\nHammer speed: {hammerSpeed} hu/s";
        }
    }
}