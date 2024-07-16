using Fifbox.Input;

namespace Fifbox.UI.Gameplay
{
    public class GameplayUIController : UIController<GameplayUIView>
    {
        protected override void Init()
        {
            FifboxActions.Asset.GameplayUI.Enable();
        }

        protected override void Uninit()
        {
            FifboxActions.Asset.GameplayUI.Disable();
        }
    }
}