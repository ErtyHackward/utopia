using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;

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
        private EntityType _entityType;
        private EntityLink _link;

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
        /// A link for an entity
        /// </summary>
        public EntityLink Link
        {
            get { return _link; }
            set { _link = value; }
        }
        
        /// <summary>
        /// Optional id of entity that takes the item
        /// </summary>
        public uint TakerEntityId
        {
            get { return _takerEntityId; }
            set { _takerEntityId = value; }
        }

        /// <summary>
        /// The type of the entity that was removed
        /// </summary>
        public EntityType EntityType
        {
            get { return _entityType; }
            set { _entityType = value; }
        }

        public static EntityOutMessage Read(BinaryReader reader)
        {
            EntityOutMessage msg;
            msg._takerEntityId = reader.ReadUInt32();
            msg._entityId = reader.ReadUInt32();
            msg._entityType = (EntityType)reader.ReadByte();
            msg._link = reader.ReadEntityLink();
            return msg;
        }

        public static void Write(BinaryWriter writer, EntityOutMessage msg)
        {
            writer.Write(msg._takerEntityId);
            writer.Write(msg._entityId);
            writer.Write((byte)msg._entityType);
            writer.Write(msg._link);
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
