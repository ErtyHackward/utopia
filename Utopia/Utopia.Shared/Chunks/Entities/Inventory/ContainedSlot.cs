using System.IO;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a contained slot that can be stored in inventory. Each slot has an entity and number of entities count.
    /// </summary>
    public class ContainedSlot : IBinaryStorable
    {
        /// <summary>
        /// Gets or sets slot position in container grid
        /// </summary>
        public Location2<byte> GridPosition { get; set; }

        /// <summary>
        /// Gets or sets items count
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Gets or sets entity
        /// </summary>
        public Item Item { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(ItemsCount);
            writer.Write(GridPosition.X);
            writer.Write(GridPosition.Z);
            Item.Save(writer);
        }

        public void Load(BinaryReader reader)
        {
            ItemsCount = reader.ReadInt32();
            Location2<byte> location;
            location.X = reader.ReadByte();
            location.Z = reader.ReadByte();
            GridPosition = location;
            Item = (Item)EntityFactory.Instance.CreateFromBytes(reader);
        }

    }
}
