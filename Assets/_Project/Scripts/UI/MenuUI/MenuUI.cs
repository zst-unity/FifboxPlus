using Mirror;
using UnityEngine;

namespace Fifbox.UI
{
    public class MenuUI : MonoBehaviour
    {
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        public void SetIP(string text)
        {
            Debug.Log($"IP set to {text}");
            NetworkManager.singleton.networkAddress = text;
        }

        public void Connect()
        {
            Debug.Log($"Connecting to {NetworkManager.singleton.networkAddress}...");
            NetworkManager.singleton.StartClient();
        }

        public void Host()
        {
            Debug.Log($"Starting host...");
            NetworkManager.singleton.StartHost();
        }

        public void Server()
        {
            Debug.Log($"Starting server...");
            NetworkManager.singleton.StartServer();
        }
    }
}
