using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.ConviUI
{
    [ExecuteAlways]
    [AddComponentMenu("UI/TextMeshPro - Text Fitter (UI)", 11)]
    public class TMP_TextFitter : MonoBehaviour
    {
        public Vector2 MaxSize = Vector2.zero;
        public Vector2 MinSize;
        public Mode ControlAxes = Mode.Horizontal | Mode.Vertical;
        public bool ResizeTextObject = true;

        [Flags]
        public enum Mode
        {
            None = 0,
            Horizontal = 0x1,
            Vertical = 0x2,
        }

        private RectTransform _rectTransform;

        protected virtual float MinX
        {
            get
            {
                if ((ControlAxes & Mode.Horizontal) != 0) return MinSize.x;
                return _rectTransform.rect.width;
            }
        }

        protected virtual float MinY
        {
            get
            {
                if ((ControlAxes & Mode.Vertical) != 0) return MinSize.y;
                return _rectTransform.rect.height;
            }
        }

        protected virtual float MaxX
        {
            get
            {
                if ((ControlAxes & Mode.Horizontal) != 0) return MaxSize.x != 0 ? MaxSize.x : float.PositiveInfinity;
                return _rectTransform.rect.width;
            }
        }

        protected virtual float MaxY
        {
            get
            {
                if ((ControlAxes & Mode.Vertical) != 0) return MaxSize.y != 0 ? MaxSize.y : float.PositiveInfinity;
                return _rectTransform.rect.height;
            }
        }

        protected virtual void UpdateElement()
        {
            var texts = GetComponentsInChildren<TMP_Text>().Where(text => text.gameObject != gameObject && text.transform.parent == transform).ToArray();

            var widestPreferredSize = Vector2.zero;
            foreach (var text in texts)
            {
                var preferredSize = text.GetPreferredValues(MaxX, MaxY);
                preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
                preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);

                if (preferredSize.x > widestPreferredSize.x) widestPreferredSize.x = preferredSize.x;
                if (preferredSize.y > widestPreferredSize.y) widestPreferredSize.y = preferredSize.y;

                if ((ControlAxes & Mode.Horizontal) != 0)
                {
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widestPreferredSize.x);
                    if (ResizeTextObject)
                    {
                        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    }
                }
                if ((ControlAxes & Mode.Vertical) != 0)
                {
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, widestPreferredSize.y);
                    if (ResizeTextObject)
                    {
                        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    }
                }
            }
        }

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();

#if UNITY_EDITOR
            ObjectChangeEvents.changesPublished += ChangesPublished;
#endif
        }

#if UNITY_EDITOR
        private void OnDisable()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                UpdateElement();
            }
        }
#endif
    }
}