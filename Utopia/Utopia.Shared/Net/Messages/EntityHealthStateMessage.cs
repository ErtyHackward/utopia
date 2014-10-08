using ProtoBuf;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Message to synchronise an entity Health State change
    /// </summary>
    [ProtoContract]
    public class EntityHealthStateMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityHealthState; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        /// <summary>
        /// New Health of the entity
        /// </summary>
        [ProtoMember(2)]
        public DynamicEntityHealthState HealthState { get; set; }
    }
}
