using ProtoBuf;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a slot containing an entity(ies) from a blueprint
    /// Should be used to initialize containers
    /// </summary>
    [ProtoContract]
    public class BlueprintSlot : ContainedSlot
    {
        [ProtoMember(1)]
        public ushort BlueprintId { get; set; }

        public override object Clone()
        {
            var bpSlot = new BlueprintSlot { BlueprintId = BlueprintId, GridPosition = GridPosition, ItemsCount = ItemsCount };

            return bpSlot;
        }
    }
}
