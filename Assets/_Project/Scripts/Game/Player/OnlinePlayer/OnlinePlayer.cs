using Fifbox.ScriptableObjects;
using UnityEngine;

namespace Fifbox.Game.Player.OnlinePlayer
{
    public class OnlinePlayer : Player
    {
        protected override bool ShouldProcessPlayer => isLocalPlayer;

        protected override void OnPlayerStart()
        {
            if (isLocalPlayer)
            {
                Data.initialLayer = FifboxLayers.LocalPlayerLayer.Index;
                Cursor.lockState = CursorLockMode.Locked;
                Model.SetActive(false);
            }

            base.OnPlayerStart();
        }
    }
}