using System.IO;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to acquire entity lock for operations like inventory items transfers
    /// </summary>
    public struct EntityLockMessage : IBinaryMessage
    {
        private EntityLink _entityId;
        private bool _lock;

        /// <summary>
        /// Entity to be locked/unlocked
        /// </summary>
        public EntityLink EntityLink
        {
            get { return _entityId; }
            set { _entityId = value; }
        }
        
        /// <summary>
        /// True - request the lock, False - release the lock
        /// </summary>
        public bool Lock
        {
            get { return _lock; }
            set { _lock = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityLock; }
        }

        public static EntityLockMessage Read(BinaryReader reader)
        {
            EntityLockMessage msg;
            msg._entityId = reader.ReadEntityLink();
            msg._lock = reader.ReadBoolean();
            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_entityId);
            writer.Write(_lock);
        }
    }
}
