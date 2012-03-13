using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a message to inform about item move from inventory to chest or to other place.
    /// This message handles all items transactions
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ItemTransferMessage : IBinaryMessage
    {
        private EntityLink _sourceEntityId;
        private Vector2I _sourceSlot;
        private EntityLink _destinationEntityId;
        private Vector2I _destinationSlot;
        private int _itemsCount;
        private uint _itemEntityId;
        private bool _isSwitch;

        /// <summary>
        /// Source object item taken from, use 0 if item was taken from world space
        /// </summary>
        public EntityLink SourceContainerEntityLink
        {
            get { return _sourceEntityId; }
            set { _sourceEntityId = value; }
        }

        /// <summary>
        /// Source conainer slot position
        /// x == -1 : equipment, y = equipmentSlot
        /// x == -2 : toolbar, toolbarslot
        /// </summary>
        public Vector2I SourceContainerSlot
        {
            get { return _sourceSlot; }
            set { _sourceSlot = value; }
        }
        
        /// <summary>
        /// Destination entity where item must be placed. Use 0 if you need to throw item to world space
        /// </summary>
        public EntityLink DestinationContainerEntityLink
        {
            get { return _destinationEntityId; }
            set { _destinationEntityId = value; }
        }
        
        /// <summary>
        /// Destination container slot position
        /// x == -1 : equipment, y = equipmentSlot
        /// x == -2 : toolbar, toolbarslot
        /// </summary>
        public Vector2I DestinationContainerSlot
        {
            get { return _destinationSlot; }
            set { _destinationSlot = value; }
        }
        
        /// <summary>
        /// Items count to transfer
        /// </summary>
        public int ItemsCount
        {
            get { return _itemsCount; }
            set { _itemsCount = value; }
        }

        /// <summary>
        /// EntityId that should be taken from the world or put to. Optional value for container to container operations
        /// </summary>
        public uint ItemEntityId
        {
            get { return _itemEntityId; }
            set { _itemEntityId = value; }
        }

        /// <summary>
        /// Indicates if operation is the switch operation
        /// </summary>
        public bool IsSwitch
        {
            get { return _isSwitch; }
            set { _isSwitch = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.ItemTransfer; }
        }
        
        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            _sourceEntityId.Save(writer);
            writer.Write(_sourceSlot);
            _destinationEntityId.Save(writer);
            writer.Write(_destinationSlot);
            writer.Write(_itemEntityId);
            writer.Write(_itemsCount);
            writer.Write(_isSwitch);
        }

        public static ItemTransferMessage Read(BinaryReader reader)
        {
            ItemTransferMessage msg;

            msg._sourceEntityId = new EntityLink(reader);
            msg._sourceSlot = reader.ReadVector2I();
            msg._destinationEntityId = new EntityLink(reader);
            msg._destinationSlot = reader.ReadVector2I();
            msg._itemEntityId = reader.ReadUInt32();
            msg._itemsCount = reader.ReadInt32();
            msg._isSwitch = reader.ReadBoolean();

            return msg;
        }
    }
}
