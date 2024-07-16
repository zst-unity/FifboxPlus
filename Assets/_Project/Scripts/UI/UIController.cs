using UnityEngine;

namespace Fifbox.UI
{
    [RequireComponent(typeof(UIView))]
    public abstract class UIController<T> : MonoBehaviour where T : UIView
    {
        protected virtual void Init() { }
        protected virtual void Uninit() { }

        public T View { get; private set; }

        private void OnValidate()
        {
            if (!GetComponent<T>()) Debug.LogWarning($"There is no {typeof(T).Name} attached to {name}");
        }

        protected void Awake()
        {
            if (TryGetComponent(out T view))
            {
                View = view;
            }
            else throw new($"There is no {typeof(T).Name} attached to {name}");

            Init();
        }

        protected void OnDestroy()
        {
            Uninit();
        }
    }
}