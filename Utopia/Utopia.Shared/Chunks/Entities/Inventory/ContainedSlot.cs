using System.IO;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a contained slot that can be stored into the inventory grid. 
    /// </summary>
    public class ContainedSlot : Slot
    {
        /// <summary>
        /// Gets or sets slot position in container grid
        /// </summary>
        public Location2<byte> GridPosition { get; set; }

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);

            if (!IsEmpty)
            {
                Location2<byte> location;
                location.X = reader.ReadByte();
                location.Z = reader.ReadByte();
                GridPosition = location;
            }
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            if (!IsEmpty)
            {
                writer.Write(GridPosition.X);
                writer.Write(GridPosition.Z);
            }
        }
    }
}
