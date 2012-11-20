using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a contained slot that can be stored into the inventory grid. 
    /// </summary>
    public class ContainedSlot : Slot
    {
        public ContainedSlot()
        {
            ItemsCount = 1;
        }

        public ContainedSlot(Vector2I from, int quantity)
        {
            GridPosition = from;
            ItemsCount = quantity;
        }

        /// <summary>
        /// Gets or sets slot position in container grid
        /// </summary>
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

        public override void LoadSlot(BinaryReader reader, EntityFactory factory)
        {
            base.LoadSlot(reader, factory);

            if (!IsEmpty)
            {
                GridPosition = reader.ReadVector2I();
            }
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            if (!IsEmpty)
            {
                writer.Write(GridPosition);
            }
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
