using ProtoBuf;
using SharpDX;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to describe physical impulse to some entity. Entity may change its position or respond with opposite
    /// </summary>
    [ProtoContract]
    public class EntityImpulseMessage : IBinaryMessage
    {
        /// <summary>
        /// Entity is affected by impulse
        /// </summary>
        [ProtoMember(1)]
        public uint DynamicEntityId { get; set; }

        /// <summary>
        /// Impulse vector
        /// </summary>
        [ProtoMember(2)]
        public Vector3 Vector3 { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityImpulse; }
        }
    }
}
