using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fifbox.ScriptableObjects.Configs
{
    [CreateAssetMenu(fileName = "DefaultConfigs", menuName = "Fifbox/Configs/Default Configs", order = 0)]
    public class DefaultConfigs : SingletonScriptableObject<DefaultConfigs>
    {
        public List<ConfigBase> configs = new();

        public static bool TryGetDefaultConfig<T>(out T config) where T : Config<T>
        {
            config = Singleton.configs.OfType<T>().First();
            return config;
        }

        public static T TryGetDefaultConfigOrCreateNew<T>() where T : Config<T>
        {
            if (TryGetDefaultConfig(out T config)) return config;
            else return ConfigUtility.Create<T>();
        }
    }
}