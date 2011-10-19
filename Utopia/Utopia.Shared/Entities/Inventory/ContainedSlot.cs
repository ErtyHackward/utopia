using System;
using System.IO;
using Utopia.Shared.Structs;

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
            if (slot != null && slot.Item != null && Item != null && slot.Item.StackType == Item.StackType && slot.ItemsCount + ItemsCount <= Item.MaxStackSize)
                return true;
            return false;
        }

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);

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
