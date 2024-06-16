using System;
using UnityEditor;
using UnityEngine;

namespace ZSToolkit.ConviUI
{
    [ExecuteAlways]
    public class UISplit : MonoBehaviour
    {
        [field: Header("Objects")]
        [field: SerializeField] public RectTransform First { get; private set; }
        [field: SerializeField] public RectTransform Second { get; private set; }

        [field: Header("Direction")]
        [field: SerializeField] public SplitDirection Direction { get; private set; }
        [field: SerializeField] public bool Invert { get; private set; }

        [field: Header("Properties")]
        [field: SerializeField, Range(0f, 1f)] public float Ratio { get; set; } = 0.5f;

        private void OnValidate()
        {
            if (!First) Debug.LogWarning("Missing first element");
            if (!Second) Debug.LogWarning("Missing second element");
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }

        private void OnDisable()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            UpdateComponent();
        }
#endif

        public void UpdateComponent()
        {
            if (!First || !Second) return;

            if (Direction == SplitDirection.Horizontal)
            {
                if (Invert)
                {
                    Second.pivot = new(0f, 0.5f);
                    First.pivot = new(1f, 0.5f);

                    Second.anchorMin = new(0f, 0f);
                    Second.anchorMax = new(Ratio, 1f);
                    Second.offsetMin = new(Second.offsetMin.x, 0f);
                    Second.offsetMax = new(Second.offsetMax.x, 0f);
                    Second.sizeDelta = Vector2.zero;

                    First.anchorMin = new(Ratio, 0f);
                    First.anchorMax = new(1f, 1f);
                    First.offsetMin = new(First.offsetMin.x, 0f);
                    First.offsetMax = new(First.offsetMax.x, 0f);
                    First.sizeDelta = Vector2.zero;
                }
                else
                {
                    First.pivot = new(0f, 0.5f);
                    Second.pivot = new(1f, 0.5f);

                    First.anchorMin = new(0f, 0f);
                    First.anchorMax = new(Ratio, 1f);
                    First.offsetMin = new(First.offsetMin.x, 0f);
                    First.offsetMax = new(First.offsetMax.x, 0f);
                    First.sizeDelta = Vector2.zero;

                    Second.anchorMin = new(Ratio, 0f);
                    Second.anchorMax = new(1f, 1f);
                    Second.offsetMin = new(Second.offsetMin.x, 0f);
                    Second.offsetMax = new(Second.offsetMax.x, 0f);
                    Second.sizeDelta = Vector2.zero;
                }
            }
            else if (Direction == SplitDirection.Vertical)
            {
                if (Invert)
                {
                    First.pivot = new(0.5f, 0f);
                    Second.pivot = new(0.5f, 1f);

                    First.anchorMin = new(0f, 0f);
                    First.anchorMax = new(1f, Ratio);
                    First.offsetMin = new(0f, First.offsetMax.y);
                    First.offsetMax = new(0f, First.offsetMin.y);
                    First.sizeDelta = Vector2.zero;

                    Second.anchorMin = new(0f, Ratio);
                    Second.anchorMax = new(1f, 1f);
                    Second.offsetMin = new(0f, Second.offsetMax.y);
                    Second.offsetMax = new(0f, Second.offsetMin.y);
                    Second.sizeDelta = Vector2.zero;
                }
                else
                {
                    Second.pivot = new(0.5f, 0f);
                    First.pivot = new(0.5f, 1f);

                    Second.anchorMin = new(0f, 0f);
                    Second.anchorMax = new(1f, Ratio);
                    Second.offsetMin = new(0f, Second.offsetMax.y);
                    Second.offsetMax = new(0f, Second.offsetMin.y);
                    Second.sizeDelta = Vector2.zero;

                    First.anchorMin = new(0f, Ratio);
                    First.anchorMax = new(1f, 1f);
                    First.offsetMin = new(0f, First.offsetMax.y);
                    First.offsetMax = new(0f, First.offsetMin.y);
                    First.sizeDelta = Vector2.zero;
                }
            }
        }
    }

    public enum SplitDirection
    {
        Horizontal,
        Vertical
    }
}