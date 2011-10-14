using System;
using System.IO;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a contained slot that can be stored into the inventory grid. 
    /// </summary>
    public class ContainedSlot : Slot
    {
        public ContainedSlot(Vector2I from, int i=0)
        {
            GridPosition = from;
            ItemsCount = i;
        }

        public ContainedSlot()
        {           
        }

        /// <summary>
        /// Gets or sets slot position in container grid
        /// </summary>
        public Vector2I GridPosition { get; set; }

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
    }
}
