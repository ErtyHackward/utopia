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
        private EntityLink _sourceEntityLink;
        private Vector2I _sourceSlot;
        private EntityLink _destinationEntityLink;
        private Vector2I _destinationSlot;
        private int _itemsCount;
        private uint _itemEntityId;
        private bool _isSwitch;

        /// <summary>
        /// Source object item taken from, use 0 if item was taken from world space
        /// </summary>
        public EntityLink SourceContainerEntityLink
        {
            get { return _sourceEntityLink; }
            set { _sourceEntityLink = value; }
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
            get { return _destinationEntityLink; }
            set { _destinationEntityLink = value; }
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
            _sourceEntityLink.Save(writer);
            writer.Write(_sourceSlot);
            _destinationEntityLink.Save(writer);
            writer.Write(_destinationSlot);
            writer.Write(_itemEntityId);
            writer.Write(_itemsCount);
            writer.Write(_isSwitch);
        }

        public static ItemTransferMessage Read(BinaryReader reader)
        {
            ItemTransferMessage msg;

            msg._sourceEntityLink = new EntityLink(reader);
            msg._sourceSlot = reader.ReadVector2I();
            msg._destinationEntityLink = new EntityLink(reader);
            msg._destinationSlot = reader.ReadVector2I();
            msg._itemEntityId = reader.ReadUInt32();
            msg._itemsCount = reader.ReadInt32();
            msg._isSwitch = reader.ReadBoolean();

            return msg;
        }

        public override string ToString()
        {
            return string.Format("ItemTransferMessage [Src:{0}, {1}; Dest:{2}, {3}; Cnt:{4} Switch:{5}]", _sourceEntityLink, _sourceSlot, _destinationEntityLink, _destinationSlot, _itemsCount, _isSwitch ? "Y" : "N");
        }
    }
}
