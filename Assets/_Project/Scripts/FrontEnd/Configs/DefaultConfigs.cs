using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fifbox.FrontEnd.Configs
{
    [CreateAssetMenu(fileName = "DefaultConfigs", menuName = "Fifbox/Configs/Default Configs", order = 0)]
    public class DefaultConfigs : ScriptableObject
    {
        private static DefaultConfigs _singleton;
        public static DefaultConfigs Singleton
        {
            get
            {
                if (!_singleton)
                {
                    var assets = Resources.LoadAll<DefaultConfigs>("Configs");
                    if (assets.Length == 0)
                    {
                        throw new("DefaultConfigs asset not found");
                    }
                    else if (assets.Length > 1)
                    {
                        Debug.LogWarning("There are multiple DefaultConfigs found");
                    }

                    foreach (var item in assets)
                    {
                        Debug.Log(item.name);
                    }

                    _singleton = assets[0];
                }

                return _singleton;
            }
        }

        public List<ConfigBase> configs = new();

        public static bool TryGetDefaultConfig<T>(out T config) where T : Config<T>
        {
            config = Singleton.configs.OfType<T>().First();
            return config;
        }
    }
}