namespace Fifbox.ScriptableObjects.Configs
{
    public static class ConfigUtility
    {
        public static bool TryGetOptimalConfig<T>(T config, out T optimalConfig) where T : Config<T>
        {
            if (config)
            {
                optimalConfig = config;
                return true;
            }

            DefaultConfigs.TryGetDefaultConfig(out optimalConfig);
            return optimalConfig;
        }
    }
}