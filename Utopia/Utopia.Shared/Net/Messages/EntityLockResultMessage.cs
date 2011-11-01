using System.IO;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about lock operation result
    /// </summary>
    public struct EntityLockResultMessage : IBinaryMessage
    {
        private uint _entityId;
        private LockResult _lockResult;
        private bool _isStatic;
        private Vector2I _chunkPosition;

        /// <summary>
        /// Indicates if entity is static
        /// </summary>
        public bool IsStatic
        {
            get { return _isStatic; }
            set { _isStatic = value; }
        }
        
        /// <summary>
        /// Gets or sets static entity chunk position
        /// </summary>
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }

        /// <summary>
        /// Entity that was requested to be locked
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }
        
        /// <summary>
        /// Informs about lock operation result
        /// </summary>
        public LockResult LockResult
        {
            get { return _lockResult; }
            set { _lockResult = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityLockResult; }
        }

        public static EntityLockResultMessage Read(BinaryReader reader)
        {
            EntityLockResultMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._lockResult = (LockResult)reader.ReadByte();
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
            writer.Write((byte)_lockResult);
            writer.Write(_isStatic);
            writer.Write(_chunkPosition);
        }
    }

    /// <summary>
    /// Enumerates all possible entity lock results
    /// </summary>
    public enum LockResult : byte
    {
        SuccessLocked,
        FailAlreadyLocked,
        NoSuchEntity
    }
}
