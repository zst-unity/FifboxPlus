using Fifbox.UI.GameplayUI;
using Mirror;
using UnityEngine;

namespace Fifbox.Systems
{
    public class Functions : FifboxSystem<Functions>
    {
        public override string ID => "functions";

        protected override void OnUpdate()
        {
            if (!NetworkClient.active) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameplayUI.Singleton.SetVisible(!GameplayUI.Singleton.Visible);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                // TODO: Screenshot
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                DebugStatistics.Singleton.SetVisible(!DebugStatistics.Singleton.Visible);
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                // TODO: POV switch
            }
        }
    }
}