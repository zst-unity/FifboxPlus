using UnityEngine;

namespace Fifbox.ScriptableObjects.Configs
{
    public static class ConfigUtility
    {
        public static T Create<T>() where T : Config<T>
        {
            return ScriptableObject.CreateInstance<T>();
        }
    }
}