using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform that some other entity left view range
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityOutMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification number of the entity
        /// </summary>
        private uint _entityId;
        private uint _takerEntityId;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityOut; }
        }

        /// <summary>
        /// Gets or sets an identification number of the entity
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }
        
        /// <summary>
        /// Optional id of entity that takes the item
        /// </summary>
        public uint TakerEntityId
        {
            get { return _takerEntityId; }
            set { _takerEntityId = value; }
        }

        public static EntityOutMessage Read(BinaryReader reader)
        {
            EntityOutMessage msg;
            msg._takerEntityId = reader.ReadUInt32();
            msg._entityId = reader.ReadUInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, EntityOutMessage msg)
        {
            writer.Write(msg._takerEntityId);
            writer.Write(msg._entityId);
        }
        
        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
