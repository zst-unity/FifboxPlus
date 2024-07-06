using UnityEngine;

namespace Fifbox.MiddleEnd
{
    public static class FifboxGlobal
    {
        public static FifboxActions ActionMap { get; private set; }
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
            Debug.Log("Initializing action map");
            ActionMap = new();
            ActionMap.Enable();
        }
    }
}