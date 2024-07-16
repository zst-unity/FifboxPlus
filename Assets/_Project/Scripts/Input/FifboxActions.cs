using System.Collections.Generic;

namespace Fifbox.Input
{
    public static class FifboxActions
    {
        public static FifboxActionsAsset Asset { get; private set; }

        private static List<string> _playerActionsInterruptions = new();
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;

            Asset = new();
            Asset.Enable();

            _initialized = true;
        }

        private static bool _playerActionsEnabled;

        public static void EnablePlayerActions()
        {
            _playerActionsEnabled = true;

            if (_playerActionsInterruptions.Count == 0) Asset.Player.Enable();
        }

        public static void DisablePlayerActions()
        {
            _playerActionsEnabled = false;

            if (_playerActionsInterruptions.Count == 0) Asset.Player.Disable();
        }

        public static void InterruptPlayerActions(string sourceGuid)
        {
            if (_playerActionsInterruptions.Contains(sourceGuid)) return;

            _playerActionsInterruptions.Add(sourceGuid);
            Asset.Player.Disable();
        }

        public static void StopInterruptingPlayerActions(string sourceGuid)
        {
            if (!_playerActionsInterruptions.Contains(sourceGuid)) return;

            _playerActionsInterruptions.Remove(sourceGuid);
            if (_playerActionsInterruptions.Count == 0)
            {
                if (_playerActionsEnabled) Asset.Player.Enable();
                else Asset.Player.Disable();
            }
        }

        public static void StopAllPlayerActionsInterruptions()
        {
            _playerActionsInterruptions.Clear();
            if (_playerActionsEnabled) Asset.Player.Enable();
            else Asset.Player.Disable();
        }
    }
}