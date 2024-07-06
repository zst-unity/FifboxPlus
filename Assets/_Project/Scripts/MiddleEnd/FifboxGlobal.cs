using UnityEngine;

namespace Fifbox.MiddleEnd
{
    public static class FifboxGlobal
    {
        public static FifboxActions ActionAsset { get; private set; }
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
            Debug.Log("FifboxGlobal init");

            InitActions();

            Debug.Log("FifboxGlobal init done");
            _initialized = true;
        }

        private static void InitActions()
        {
            Debug.Log("Initializing action asset singleton");
            ActionAsset = new();
            ActionAsset.Enable();
        }
    }
}