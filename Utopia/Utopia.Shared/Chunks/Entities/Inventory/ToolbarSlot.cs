using System.IO;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a toolbar slot, that available to contain a link to pair of items into the inventory or equipment
    /// </summary>
    public class ToolbarSlot : ContainedSlot
    {
        /// <summary>
        /// Left entity
        /// </summary>
        public uint Left { get; set; }

        /// <summary>
        /// Right entity
        /// </summary>
        public uint Right { get; set; }

        // overriding items and do whole writing by ourself
        public override void Save(BinaryWriter writer)
        {
            // saving toolbar slot position
            writer.Write(GridPosition);

            // saving values
            writer.Write(Left);
            writer.Write(Right);
        }

        public override void Load(BinaryReader reader)
        {
            // reading toolbar slot position
            GridPosition = reader.ReadVector2I();

            Left = reader.ReadUInt32();
            Right = reader.ReadUInt32();
        }
    }
}