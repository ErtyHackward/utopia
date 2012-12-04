using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs player that another entity somewhere near, provides an entity object
    /// </summary>
    [ProtoContract]
    public struct EntityInMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityIn; }
        }

        [ProtoMember(1)]
        public IEntity Entity { get; set; }

        /// <summary>
        /// Entity id that throws an item (optional, default 0)
        /// </summary>
        [ProtoMember(2)]
        public uint SourceEntityId { get; set; }

        /// <summary>
        /// A link for an entity
        /// </summary>
        [ProtoMember(3)]
        public EntityLink Link { get; set; }
    }
}
