using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Describes message for connection testing
    /// </summary>
    [ProtoContract]
    public class PingMessage : IBinaryMessage
    {
        /// <summary>
        /// Any long number to verify ping command
        /// </summary>
        [ProtoMember(1)]
        public long Token { get; set; }

        /// <summary>
        /// Indicates whether this command is request or responce
        /// </summary>
        [ProtoMember(2)]
        public bool Request { get; set; }

        public byte MessageId
        {
            get { return (byte)MessageTypes.Ping; }
        }
    }
}
