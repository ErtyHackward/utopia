using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about lock operation result
    /// </summary>
    [ProtoContract]
    public struct EntityLockResultMessage : IBinaryMessage
    {
        /// <summary>
        /// Entity that was requested to be locked
        /// </summary>
        [ProtoMember(1)]
        public EntityLink EntityLink { get; set; }

        /// <summary>
        /// Informs about lock operation result
        /// </summary>
        [ProtoMember(2)]
        public LockResult LockResult { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityLockResult; }
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
