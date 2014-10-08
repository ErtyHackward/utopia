using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.States;
using Utopia.Components;
using Utopia.Entities.Managers;
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
            var playerEntityManager = _iocContainer.Get<PlayerEntityManager>();
            inventoryComponent.ShowInventory(playerEntityManager.LockedContainer);

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(true);

            var notice = _iocContainer.Get<InventoryEventComponent>();
            notice.DisableComponent();

            var toolBar = (SandboxToolBar)_iocContainer.Get<ToolBarUi>();
            toolBar.DisplayBackground = true;

            for (int i = 0; i < toolBar.Slots.Count; i++)
            {
                var inventoryCell = toolBar.Slots[i];
                inventoryCell.DrawCellBackground = true;
                inventoryCell.Color = new S33M3Resources.Structs.ByteColor(255, 255, 255, 255);
                toolBar.NumbersLabels[i].IsVisible = false;
            }

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
            guiManager.Screen.HideToolTip();

            var notice = _iocContainer.Get<InventoryEventComponent>();
            notice.EnableComponent();

            var toolBar = (SandboxToolBar)_iocContainer.Get<ToolBarUi>();
            toolBar.DisplayBackground = false;

            for (int i = 0; i < toolBar.Slots.Count; i++)
            {
                var inventoryCell = toolBar.Slots[i];
                inventoryCell.DrawCellBackground = false;
                inventoryCell.Color = new S33M3Resources.Structs.ByteColor(255, 255, 255, 120);
                toolBar.NumbersLabels[i].IsVisible = inventoryCell.Slot != null;
            }

            base.OnDisabled(nextState);
        }
    }
}
