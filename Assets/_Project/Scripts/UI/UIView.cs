using UnityEngine;

namespace Fifbox.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        protected virtual void Init() { }
        protected virtual void Uninit() { }

        public CanvasGroup MasterCanvasGroup { get; private set; }

        protected void Awake()
        {
            MasterCanvasGroup = GetComponent<CanvasGroup>();
            Init();
        }

        protected void OnDestroy()
        {
            Uninit();
        }
    }
}