using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about tool use result
    /// </summary>
    [ProtoContract]
    public class UseFeedbackMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification token of the use operation
        /// </summary>
        [ProtoMember(1)]
        public int Token { get; set; }

        /// <summary>
        /// Serialized bytes of the EntityImpact
        /// </summary>
        [ProtoMember(2)]
        public byte[] EntityImpactBytes { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.UseFeedback; }
        }
    }
}
