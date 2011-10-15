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
