using UnityEngine;

namespace Fifbox.UI.GameplayUI
{
    public class Crosshair : MonoBehaviour, IGameplayUIElement
    {
        public string ID => "crosshair";
        public GameObject GameObject => gameObject;

        public void ResetElement() { }
    }
}