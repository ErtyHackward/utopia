using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.States;
using Utopia.Components;
using Utopia.GUI.Inventory;

namespace Realms.Client.States
{
    public class InGameInventoryState : GameState
    {
        private readonly IKernel _iocContainer;

        public InGameInventoryState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
        }

        public override string Name
        {
            get { return "Inventory"; }
        }

        public override void OnEnabled(GameState previousState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Color = new SharpDX.Color4(0, 0, 0, 0.85f);
            fadeComponent.Visible = true;

            var inventoryComponent = _iocContainer.Get<InventoryComponent>();
            inventoryComponent.ShowInventory();

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(true);

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Visible = false;

            var inventoryComponent = _iocContainer.Get<InventoryComponent>();
            inventoryComponent.HideInventory();

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(false);

            base.OnDisabled(nextState);
        }
    }
}
