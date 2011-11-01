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
        private uint _entityId;
        private bool _lock;
        private bool _isStatic;
        private Vector2I _chunkPosition;

        /// <summary>
        /// Entity to be locked
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }
        
        /// <summary>
        /// Static entity chunk position
        /// </summary>
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }
        
        /// <summary>
        /// Indicates if entity is static
        /// </summary>
        public bool IsStatic
        {
            get { return _isStatic; }
            set { _isStatic = value; }
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

            msg._entityId = reader.ReadUInt32();
            msg._lock = reader.ReadBoolean();
            msg._isStatic = reader.ReadBoolean();
            msg._chunkPosition = reader.ReadVector2I();

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
            writer.Write(_isStatic);
            writer.Write(_chunkPosition);
        }
    }
}
