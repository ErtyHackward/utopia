using System;
using System.IO;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a toolbar slot, that available to contain a link to pair of slots into the inventory
    /// </summary>
    public class ToolbarSlot : ContainedSlot
    {
        /// <summary>
        /// Inventory position of the left subslot
        /// </summary>
        public Vector2I? Left { get; set; }

        /// <summary>
        /// Inventory position of the right subslot
        /// </summary>
        public Vector2I? Right { get; set; }

        // overriding items and do whole writing by ourself

        public override void Save(BinaryWriter writer)
        {
            if (Left == null && Right == null) 
                throw new InvalidOperationException("Unable to save ToolBar slot because it is empty. Empty slot should not be saved.");

            // saving toolbar slot position

            writer.Write(GridPosition);

            // we do not going to write empty subslots 
            // storage layout byte 0 - only left, 1 - only right, 2 - both slots
            byte layout = (Left != null && Right != null) ? (byte)2 : (Left != null ? (byte)0 : (byte)1);

            writer.Write(layout);

            // saving child slots location
            if (Left != null)
            {
                writer.Write(Left.Value);
            }

            if (Right != null)
            {
                writer.Write(Right.Value);
            }
        }

        public override void Load(BinaryReader reader)
        {
            // reading toolbar slot position
            GridPosition = reader.ReadVector2I();

            //read layout byte
            var layout = reader.ReadByte();

            if (layout == 0 || layout == 2)
            {
                // left slot position
                Left = reader.ReadVector2I();
            }

            if (layout == 1 || layout == 2)
            {
                // left slot position
                Right = reader.ReadVector2I(); ;
            }

        }
    }
}
