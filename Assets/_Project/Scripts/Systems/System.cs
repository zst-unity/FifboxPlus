using Mirror;

namespace Fifbox.Systems
{
    public abstract class FifboxSystem<T> : NetworkBehaviour where T : FifboxSystem<T>
    {
        private static FifboxSystem<T> _singleton;
        public static T Singleton => (T)_singleton;

        protected virtual void OnStartup() { }
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
                OnStartup();
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