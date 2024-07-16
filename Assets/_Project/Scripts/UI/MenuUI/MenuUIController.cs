using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZSToolkit.GlobalData;

namespace Fifbox.UI.Menu
{
    public class MenuUIController : UIController<MenuUIView>
    {
        [field: SerializeField] public Button ConnectButton { get; private set; }
        [field: SerializeField] public Button HostButton { get; private set; }
        [field: SerializeField] public Button ServerButton { get; private set; }
        [field: SerializeField] public TMP_InputField IPAddressInputField { get; private set; }

        protected override void Init()
        {
            IPAddressInputField.text = GlobalData.Load("MenuData", "IPAddress", "localhost");
            NetworkManager.singleton.networkAddress = IPAddressInputField.text;

            ConnectButton.onClick.AddListener(ConnectPressed);
            HostButton.onClick.AddListener(HostPressed);
            ServerButton.onClick.AddListener(ServerPressed);
            IPAddressInputField.onValueChanged.AddListener(IPAddressChanged);
        }

        protected override void Uninit()
        {
            ConnectButton.onClick.RemoveListener(ConnectPressed);
            HostButton.onClick.RemoveListener(HostPressed);
            ServerButton.onClick.RemoveListener(ServerPressed);
            IPAddressInputField.onValueChanged.RemoveListener(IPAddressChanged);
        }

        private void ConnectPressed()
        {
            NetworkManager.singleton.StartClient();
        }

        private void HostPressed()
        {
            NetworkManager.singleton.StartHost();
        }

        private void ServerPressed()
        {
            NetworkManager.singleton.StartServer();
        }

        private void IPAddressChanged(string newAddress)
        {
            var isValid = ValidateIPAddress(newAddress);
            ConnectButton.interactable = isValid;

            if (isValid)
            {
                GlobalData.Save("MenuData", "IPAddress", newAddress);
                NetworkManager.singleton.networkAddress = newAddress;
            }
        }

        private bool ValidateIPAddress(string address)
        {
            if (address == "localhost") return true;

            var numbers = address.Split('.');
            if (numbers.Length != 4) return false;

            foreach (var num in numbers)
            {
                if (!byte.TryParse(num, out _)) return false;
            }

            return true;
        }
    }
}