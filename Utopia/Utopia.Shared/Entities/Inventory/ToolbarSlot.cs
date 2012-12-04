using ProtoBuf;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a toolbar slot, that available to contain a link to pair of items into the inventory or equipment
    /// </summary>
    [ProtoContract]
    public class ToolbarSlot : ContainedSlot
    {
        /// <summary>
        /// Left entity
        /// </summary>
        [ProtoMember(1)]
        public uint ItemId { get; set; }
    }
}