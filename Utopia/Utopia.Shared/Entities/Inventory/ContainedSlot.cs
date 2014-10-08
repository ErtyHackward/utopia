using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a contained slot that can be stored into the inventory grid. 
    /// </summary>
    [ProtoContract]
    public class ContainedSlot : Slot
    {
        public ContainedSlot()
        {

        }

        public ContainedSlot(Vector2I from, int quantity)
        {
            GridPosition = from;
            ItemsCount = quantity;
        }

        /// <summary>
        /// Gets or sets slot position in container grid
        /// </summary>
        [ProtoMember(1)]
        public Vector2I GridPosition { get; set; }

        /// <summary>
        /// Detects whether the slots can be stacked
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool CanStackWith(ContainedSlot slot)
        {
            return CanStackWith(slot.Item, slot.ItemsCount);
        }

        /// <summary>
        /// Detects whether the slots can be stacked
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemsCount"> </param>
        /// <returns></returns>
        public bool CanStackWith(IItem item, int itemsCount)
        {
            if (item != null && Item != null && item.StackType == Item.StackType && itemsCount + ItemsCount <= Item.MaxStackSize)
                return true;
            return false;
        }

        public override object Clone()
        {
            var slot = new ContainedSlot {
                Item = Item, 
                ItemsCount = ItemsCount, 
                GridPosition = GridPosition
            };
            
            return slot;
        }
    }
}
