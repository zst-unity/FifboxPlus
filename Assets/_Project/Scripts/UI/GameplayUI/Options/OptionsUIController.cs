using System;
using Fifbox.Input;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fifbox.UI.Gameplay.Options
{
    public class OptionsUIController : UIController<OptionsUIView>
    {
        [field: SerializeField] public Button DisconnectButton { get; private set; }

        private string _optionsInterruptionSource;

        protected override void Init()
        {
            _optionsInterruptionSource = Guid.NewGuid().ToString();

            DisconnectButton.onClick.AddListener(DisconnectPressed);
            FifboxActions.Asset.GameplayUI.ToggleOptions.performed += OnOptionsToggle;
        }

        protected override void Uninit()
        {
            DisconnectButton.onClick.RemoveListener(DisconnectPressed);
            FifboxActions.Asset.GameplayUI.ToggleOptions.performed -= OnOptionsToggle;
        }

        private void DisconnectPressed()
        {
            NetworkManager.singleton.StopHost();
        }

        private void OnOptionsToggle(InputAction.CallbackContext context)
        {
            View.SetShowing(!View.Showing);

            if (View.Showing)
            {
                FifboxActions.InterruptPlayerActions(_optionsInterruptionSource);
                FifboxCursor.Interrupt(_optionsInterruptionSource);
            }
            else
            {
                FifboxActions.StopInterruptingPlayerActions(_optionsInterruptionSource);
                FifboxCursor.StopInterrupting(_optionsInterruptionSource);
            }
        }
    }
}