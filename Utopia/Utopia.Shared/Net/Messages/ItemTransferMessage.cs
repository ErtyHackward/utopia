using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a message to inform about item move from inventory to chest or to other place.
    /// This message handles all items transactions
    /// </summary>
    [ProtoContract]
    public class ItemTransferMessage : IBinaryMessage
    {
        /// <summary>
        /// Source object item taken from, use 0 if item was taken from world space
        /// </summary>
        [ProtoMember(1)]
        public EntityLink SourceContainerEntityLink { get; set; }

        /// <summary>
        /// Source conainer slot position
        /// x == -1 : equipment, y = equipmentSlot
        /// x == -2 : toolbar, toolbarslot
        /// </summary>
        [ProtoMember(2)]
        public Vector2I SourceContainerSlot { get; set; }

        /// <summary>
        /// Destination entity where item must be placed. Use 0 if you need to throw item to world space
        /// </summary>
        [ProtoMember(3)]
        public EntityLink DestinationContainerEntityLink { get; set; }

        /// <summary>
        /// Destination container slot position
        /// x == -1 : equipment, y = equipmentSlot
        /// x == -2 : toolbar, toolbarslot
        /// </summary>
        [ProtoMember(4)]
        public Vector2I DestinationContainerSlot { get; set; }

        /// <summary>
        /// Items count to transfer
        /// </summary>
        [ProtoMember(5)]
        public int ItemsCount { get; set; }

        /// <summary>
        /// EntityId that should be taken from the world or put to. Optional value for container to container operations
        /// </summary>
        [ProtoMember(6)]
        public uint ItemEntityId { get; set; }

        /// <summary>
        /// Indicates if operation is the switch operation
        /// </summary>
        [ProtoMember(7)]
        public bool IsSwitch { get; set; }

        /// <summary>
        /// Entity id that performs the transfer
        /// </summary>
        [ProtoMember(8)]
        public uint SourceEntityId { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.ItemTransfer; }
        }
        
        public override string ToString()
        {
            return string.Format("ItemTransferMessage [Src:{0}, {1}; Dest:{2}, {3}; Cnt:{4} Switch:{5}]", SourceContainerEntityLink, SourceContainerSlot, DestinationContainerEntityLink, DestinationContainerSlot, ItemsCount, IsSwitch ? "Y" : "N");
        }
    }
}
