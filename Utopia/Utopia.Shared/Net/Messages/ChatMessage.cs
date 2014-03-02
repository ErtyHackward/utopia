using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Chat message is sent to exchange messages between clients, or to inform client by server
    /// </summary>
    [ProtoContract]
    public class ChatMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Chat; }
        }

        /// <summary>
        /// Indicates if message should be displayed in format: * nick message
        /// </summary>
        [ProtoMember(1)]
        public bool Action { get; set; }

        /// <summary>
        /// Gets or sets display name of the sender, can be null if system message
        /// </summary>
        [ProtoMember(3)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets actual message text
        /// </summary>
        [ProtoMember(4)]
        public string Message { get; set; }

        /// <summary>
        /// Indicates if this is the special server message
        /// </summary>
        [ProtoMember(5)]
        public bool IsServerMessage { get; set; }
    }
}
