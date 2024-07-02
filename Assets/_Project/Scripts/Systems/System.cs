using Mirror;
using UnityEngine;

namespace Fifbox.Systems
{
    [RequireComponent(typeof(NetworkIdentity))]
    public abstract class FifboxSystem<T> : NetworkBehaviour where T : FifboxSystem<T>
    {
        private static FifboxSystem<T> _singleton;
        public static T Singleton => (T)_singleton;

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnUpdate() { }

        public abstract string ID { get; }

        private void Awake()
        {
            if (_singleton != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _singleton = this;
                OnStart();
            }
        }

        private void OnDestroy()
        {
            OnStop();
        }

        private void Update()
        {
            OnUpdate();
        }
    }
}