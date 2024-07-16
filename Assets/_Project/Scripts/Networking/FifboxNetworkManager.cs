using Mirror;
using UnityEngine;
using ZSToolkit.ZSTUtility;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox.Networking
{
    public class FifboxNetworkManager : NetworkManager
    {
        [Header("Custom properties")]
        [SerializeField, WithComponent(typeof(Canvas))] private GameObject _gameplayCanvas;

        public override void OnClientConnect()
        {
            Debug.Log("Client connected");

            Debug.Log("Spawning gameplay canvas");
            _gameplayCanvas.Spawn();

            base.OnClientConnect();

            FifboxCursor.StopAllPInterruptions();
            FifboxCursor.Lock();
        }

        public override void OnClientDisconnect()
        {
            Debug.Log("Client disconnect");

            base.OnClientDisconnect();

            FifboxCursor.Unlock();
        }
    }
}