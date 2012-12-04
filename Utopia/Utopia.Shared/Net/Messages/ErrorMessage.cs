using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Message used to inform the client about some of urgent event
    /// </summary>
    [ProtoContract]
    public struct ErrorMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Error; }
        }

        /// <summary>
        /// Gets or sets message error code
        /// </summary>
        [ProtoMember(1)]
        public ErrorCodes ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets additinal error data
        /// </summary>
        [ProtoMember(2)]
        public int Data { get; set; }

        /// <summary>
        /// Gets or sets error description
        /// </summary>
        [ProtoMember(3)]
        public string Message { get; set; }
    }

    public enum ErrorCodes : byte
    {
        LoginPasswordIncorrect,
        LoginAlreadyRegistered,
        VersionMissmatch,
        ChunkTooFar,
        AnotherInstanceLogged
    }
}
