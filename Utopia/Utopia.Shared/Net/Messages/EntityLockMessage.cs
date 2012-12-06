using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to acquire entity lock for operations like inventory items transfers
    /// </summary>
    [ProtoContract]
    public class EntityLockMessage : IBinaryMessage
    {
        /// <summary>
        /// Entity to be locked/unlocked
        /// </summary>
        [ProtoMember(1)]
        public EntityLink EntityLink { get; set; }

        /// <summary>
        /// True - request the lock, False - release the lock
        /// </summary>
        [ProtoMember(2)]
        public bool Lock { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityLock; }
        }
    }
}
