using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about entity equipment change
    /// </summary>
    [ProtoContract]
    public class EntityEquipmentMessage : IBinaryMessage
    {
        /// <summary>
        /// New entity object
        /// </summary>
        [ProtoMember(1)]
        public IEntity Entity { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityEquipment; }
        }
    }
}
