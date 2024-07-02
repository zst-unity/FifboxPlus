using UnityEngine;

namespace Fifbox.Systems
{
    public class SystemsContainer : MonoBehaviour
    {
        public static SystemsContainer Singleton { get; private set; }

        private void Awake()
        {
            if (!Singleton)
            {
                DontDestroyOnLoad(gameObject);
                Singleton = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}