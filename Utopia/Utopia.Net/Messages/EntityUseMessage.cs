using System.IO;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines message that should be used by client to use an entity
    /// </summary>
    public struct EntityUseMessage : IBinaryMessage
    {
        private uint _entityId;

        /// <summary>
        /// Gets or sets Entity identification number
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityUse; }
        }

        public static EntityUseMessage Read(BinaryReader reader)
        {
            EntityUseMessage msg;
            msg._entityId = reader.ReadUInt32();
            return msg;
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(_entityId);
        }
    }
}
