using System.IO;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about lock operation result
    /// </summary>
    public struct EntityLockResultMessage : IBinaryMessage
    {
        private uint _entityId;
        private LockResult _lockResult;

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
        }
    }

    /// <summary>
    /// Enumerates all possible entity lock results
    /// </summary>
    public enum LockResult : byte
    {
        SuccessLocked,
        FailAlreadyLocked
    }
}
