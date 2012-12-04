using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that used to inform client about login operation result
    /// </summary>
    [ProtoContract]
    public struct LoginResultMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.LoginResult; }
        }

        /// <summary>
        /// Gets or sets value indicating if logon procedure was completed successfully
        /// </summary>
        [ProtoMember(1)]
        public bool Logged { get; set; }
    }
}
