using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Response to GetEntityMessage. Contains requested entity object
    /// </summary>
    [ProtoContract]
    public class EntityDataMessage : IBinaryMessage
    {
        public byte MessageId { get { return (byte)MessageTypes.EntityData; } }

        /// <summary>
        /// Complete entity object
        /// </summary>
        [ProtoMember(1)]
        public IEntity Entity { get; set; }

        /// <summary>
        /// Id of the entity (could be used if no entity was found)
        /// </summary>
        [ProtoMember(2)]
        public uint DynamicId { get; set; }
    }
}