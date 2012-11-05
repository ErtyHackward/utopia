using System.IO;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a slot containing an entity(ies) from a blueprint
    /// Should be used to initialize containers
    /// </summary>
    public class BlueprintSlot : ContainedSlot
    {
        public ushort BlueprintId { get; set; }

        // overriding items and do whole writing by ourself
        public override void Save(BinaryWriter writer)
        {
            // saving toolbar slot position
            writer.Write(GridPosition);

            // saving values
            writer.Write(ItemsCount);
            writer.Write(BlueprintId);
        }

        public override void LoadSlot(BinaryReader reader, EntityFactory factory)
        {
            // reading toolbar slot position
            GridPosition = reader.ReadVector2I();

            ItemsCount = reader.ReadInt32();
            BlueprintId = reader.ReadUInt16();
        }
    }
}
