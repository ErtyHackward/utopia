using System;
using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that informs player that another entity somewhere near, provides an entity object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityInMessage : IBinaryMessage
    {
        private IDynamicEntity _entity;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityIn; }
        }

        public IDynamicEntity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public static EntityInMessage Read(BinaryReader reader)
        {
            EntityInMessage msg;
            
            var entity = EntityFactory.Instance.CreateFromBytes(reader);
            msg._entity = (IDynamicEntity)entity;

            return msg;
        }

        public static void Write(BinaryWriter writer, EntityInMessage msg)
        {
            if(msg.Entity == null)
                throw new NullReferenceException("Entity object is null");

            msg.Entity.Save(writer);

        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
