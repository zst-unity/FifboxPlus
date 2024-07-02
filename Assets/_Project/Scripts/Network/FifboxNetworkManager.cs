using Fifbox.UI.GameplayUI;
using Mirror;

namespace Fifbox.Network
{
    public class FifboxNetworkManager : NetworkManager
    {
        public override void OnClientConnect()
        {
            base.OnClientConnect();

            GameplayUI.Singleton.SetUIActive(true);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            GameplayUI.Singleton.SetUIActive(false);
        }
    }
}