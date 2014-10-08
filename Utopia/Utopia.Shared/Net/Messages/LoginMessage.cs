using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used by client to log in to the server
    /// </summary>
    [ProtoContract]
    public class LoginMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Login; }
        }

        /// <summary>
        /// Gets or sets a user login
        /// </summary>
        [ProtoMember(1)]
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets a user password sha1 hash
        /// </summary>
        [ProtoMember(2)]
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets a client software version
        /// </summary>
        [ProtoMember(4)]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a user display name
        /// </summary>
        [ProtoMember(5)]
        public string DisplayName { get; set; }
    }
}
