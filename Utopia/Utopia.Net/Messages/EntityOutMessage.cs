using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
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

        public static EntityOutMessage Read(BinaryReader reader)
        {
            EntityOutMessage msg;
            msg._entityId = reader.ReadUInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, EntityOutMessage msg)
        {
            writer.Write(msg._entityId);
        }


        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
