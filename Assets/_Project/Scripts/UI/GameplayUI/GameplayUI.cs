using System.Collections.Generic;
using Fifbox.Systems;
using UnityEngine;

namespace Fifbox.UI.GameplayUI
{
    public class GameplayUI : FifboxSystem<GameplayUI>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        public readonly Dictionary<string, IGameplayUIElement> Elements = new();

        public bool Visible { get; private set; }
        public bool Active { get; private set; }
        public override string ID => "gameplay_ui";

        protected override void OnStart()
        {
            SetVisible(false);

            foreach (var element in GetComponentsInChildren<IGameplayUIElement>(true))
            {
                Elements.Add(element.ID, element);
                element.ResetElement();
                element.GameObject.SetActive(false);
            }
        }

        public void SetUIActive(bool active)
        {
            SetVisible(active);
            Active = active;

            foreach (var element in Elements.Values)
            {
                element.ResetElement();
                element.GameObject.SetActive(Active);
            }
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    public interface IGameplayUIElement
    {
        public string ID { get; }
        public GameObject GameObject { get; }
        public void ResetElement();
    }
}