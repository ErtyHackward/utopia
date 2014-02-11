using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Requests any dynamic entity by its id. Used to synchronize data in case of desync
    /// </summary>
    [ProtoContract]
    public class GetEntityMessage : IBinaryMessage
    {
        public byte MessageId { get { return (byte)MessageTypes.GetEntity; } }

        /// <summary>
        /// Id of the entity to request
        /// </summary>
        [ProtoMember(1)]
        public uint DynamicEntityId { get; set; }
    }
}