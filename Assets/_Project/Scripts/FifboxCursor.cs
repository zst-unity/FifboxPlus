using System.Collections.Generic;
using UnityEngine;

namespace Fifbox
{
    public static class FifboxCursor
    {
        private static List<string> _interruptions = new();
        private static bool _locked;

        public static void Lock()
        {
            _locked = true;

            if (_interruptions.Count == 0) Cursor.lockState = CursorLockMode.Locked;
        }

        public static void Unlock()
        {
            _locked = false;

            if (_interruptions.Count == 0) Cursor.lockState = CursorLockMode.None;
        }

        public static void Interrupt(string sourceGuid)
        {
            if (_interruptions.Contains(sourceGuid)) return;

            _interruptions.Add(sourceGuid);
            Cursor.lockState = CursorLockMode.None;
        }

        public static void StopInterrupting(string sourceGuid)
        {
            if (!_interruptions.Contains(sourceGuid)) return;

            _interruptions.Remove(sourceGuid);
            if (_interruptions.Count == 0)
            {
                if (_locked) Cursor.lockState = CursorLockMode.Locked;
                else Cursor.lockState = CursorLockMode.None;
            }
        }

        public static void StopAllPInterruptions()
        {
            _interruptions.Clear();
            if (_locked) Cursor.lockState = CursorLockMode.Locked;
            else Cursor.lockState = CursorLockMode.None;
        }
    }
}