using System.IO;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a toolbar slot, that available to contain a pair of items
    /// </summary>
    public class ToolbarSlot : ContainedSlot
    {
        public Slot Left { get; set; }
        public Slot Right { get; set; }

        // overriding items and do whole writing by ourself

        public override void Save(BinaryWriter writer)
        {
            writer.Write(GridPosition.X);
            writer.Write(GridPosition.Z);

            if(Left != null)
                Left.Save(writer);
            else 
                WriteEmpty(writer);

            if(Right != null)
                Right.Save(writer);
            else
                WriteEmpty(writer);
            
        }

        public override void Load(BinaryReader reader)
        {
            Location2<byte> location;
            location.X = reader.ReadByte();
            location.Z = reader.ReadByte();
            GridPosition = location;

            Left.Load(reader);
            if (Left.IsEmpty)
                Left = null;

            Right.Load(reader);
            if (Right.IsEmpty)
                Right = null;
        }
    }
}
