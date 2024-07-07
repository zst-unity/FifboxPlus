namespace Fifbox.Input
{
    public partial class FifboxActions
    {
        public static FifboxActions Asset { get; private set; }
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;

            Asset = new();
            Asset.Enable();

            _initialized = true;
        }
    }
}