using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Fifbox.UI.Gameplay.Options
{
    public class OptionsUIView : UIView
    {
        public bool Showing { get; private set; }

        [field: SerializeField] public RectTransform MainPanel { get; private set; }

        [field: Header("Tweening properties")]
        [field: SerializeField] public float ShowDuration { get; private set; }
        [field: SerializeField] public Ease ShowEase { get; private set; }
        [field: SerializeField] public float HideDuration { get; private set; }
        [field: SerializeField] public Ease HideEase { get; private set; }

        private TweenerCore<Vector2, Vector2, VectorOptions> _pivotTween;
        private TweenerCore<Vector3, Vector3, VectorOptions> _positionTween;

        private float _initialXPosition;

        protected override void Init()
        {
            _initialXPosition = MainPanel.position.x;
            MainPanel.pivot = new(1f, 0.5f);
            MainPanel.position = new(-_initialXPosition, MainPanel.position.y);
        }

        public void SetShowing(bool showing)
        {
            if (Showing == showing) return;

            Showing = showing;
            _pivotTween?.Kill();
            _positionTween?.Kill();

            var duration = showing ? ShowDuration : HideDuration;
            var ease = showing ? ShowEase : HideEase;

            _pivotTween = MainPanel.DOPivotX(Showing ? 0f : 1f, duration).SetEase(ease);
            _positionTween = MainPanel.DOMoveX(Showing ? _initialXPosition : -_initialXPosition, duration).SetEase(ease);
        }
    }
}