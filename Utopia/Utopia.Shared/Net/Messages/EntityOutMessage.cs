using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform that some other entity left view range
    /// </summary>
    [ProtoContract]
    public class EntityOutMessage : IBinaryMessage
    {
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
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        /// <summary>
        /// A link to the entity
        /// </summary>
        [ProtoMember(2)]
        public EntityLink Link { get; set; }

        /// <summary>
        /// Optional id of entity that takes the item
        /// </summary>
        [ProtoMember(3)]
        public uint TakerEntityId { get; set; }
    }
}
