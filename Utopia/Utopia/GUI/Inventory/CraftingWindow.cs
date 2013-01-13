using System.Drawing;
using S33M3CoreComponents.Inputs;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.Inventory
{
    public class CraftingWindow : InventoryWindow
    {
        public CraftingWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition, Point gridOffset, InputsManager inputManager) : 
            base(container, iconFactory, windowStartPosition, gridOffset, inputManager)
        {

        }
    }
}
