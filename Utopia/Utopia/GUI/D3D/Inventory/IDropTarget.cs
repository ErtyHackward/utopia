using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Inventory;


namespace Utopia.GUI.D3D.Inventory
{
    public interface IDropTarget
    {
        bool MouseHovering { get; set; }
        bool IsLink { get; set; }
        IItem Item { get; set; }

        EquipmentSlotType InventorySlot { get; set; }
        void Link(IItem item);
    }
}
