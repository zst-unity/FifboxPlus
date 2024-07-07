using UnityEngine;

namespace Fifbox.ScriptableObjects
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _singleton;
        public static T Singleton
        {
            get
            {
                if (!_singleton)
                {
                    var assets = Resources.LoadAll<T>("ScriptableObjects");
                    if (assets.Length == 0)
                    {
                        throw new($"{typeof(T).Name} asset not found");
                    }
                    else if (assets.Length > 1)
                    {
                        Debug.LogWarning($"There are multiple {typeof(T).Name} found");
                    }

                    _singleton = assets[0];
                }

                return _singleton;
            }
        }
    }
}