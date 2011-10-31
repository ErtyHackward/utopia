using System;
using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs player that another entity somewhere near, provides an entity object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityInMessage : IBinaryMessage
    {
        private IEntity _entity;
        private uint _parentEntityId;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityIn; }
        }

        public IEntity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }
        
        /// <summary>
        /// Entity id that throws an item (optional, default 0)
        /// </summary>
        public uint ParentEntityId
        {
            get { return _parentEntityId; }
            set { _parentEntityId = value; }
        }

        public static EntityInMessage Read(BinaryReader reader)
        {
            EntityInMessage msg;

            msg._parentEntityId = reader.ReadUInt32();
            msg._entity = EntityFactory.Instance.CreateFromBytes(reader);

            return msg;
        }

        public static void Write(BinaryWriter writer, EntityInMessage msg)
        {
            if(msg.Entity == null)
                throw new NullReferenceException("Entity object is null");
            writer.Write(msg._parentEntityId);
            msg.Entity.Save(writer);

        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
