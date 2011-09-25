using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Represents a message to inform about item move from inventory to chest or to other place
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ItemTransferMessage : IBinaryMessage
    {
        private uint _sourceEntityId;
        private Vector2I _sourceSlot;
        private uint _destinationEntityId;
        private Vector2I _destinationSlot;
        private int _itemsCount;

        /// <summary>
        /// Source object item taken from, use 0 if item was taken from world space
        /// </summary>
        public uint SourceEntityId
        {
            get { return _sourceEntityId; }
            set { _sourceEntityId = value; }
        }

        /// <summary>
        /// Source conainer slot position
        /// </summary>
        public Vector2I SourceSlot
        {
            get { return _sourceSlot; }
            set { _sourceSlot = value; }
        }
        
        /// <summary>
        /// Destination entity where item must be placed. Use 0 if you need to throw item to world space
        /// </summary>
        public uint DestinationEntityId
        {
            get { return _destinationEntityId; }
            set { _destinationEntityId = value; }
        }
        
        /// <summary>
        /// Destination container slot position
        /// </summary>
        public Vector2I DestinationSlot
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
            writer.Write(_sourceEntityId);
            writer.Write(_sourceSlot);
            writer.Write(_destinationEntityId);
            writer.Write(_destinationSlot);
            writer.Write(_itemsCount);
        }

        public static ItemTransferMessage Read(BinaryReader reader)
        {
            ItemTransferMessage msg;

            msg._sourceEntityId = reader.ReadUInt32();
            msg._sourceSlot = reader.ReadVector2I();
            msg._destinationEntityId = reader.ReadUInt32();
            msg._destinationSlot = reader.ReadVector2I();
            msg._itemsCount = reader.ReadInt32();

            return msg;
        }
    }
}
