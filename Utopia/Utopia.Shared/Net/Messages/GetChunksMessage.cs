using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defindes a message used by a client to request a range of chunks from a server
    /// </summary>
    [ProtoContract]
    public class GetChunksMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetChunks; }
        }

        /// <summary>
        /// Gets or sets chunks range
        /// </summary>
        [ProtoMember(1)]
        public Range2I Range { get; set; }

        /// <summary>
        /// Gets or sets request mode flag
        /// </summary>
        [ProtoMember(2)]
        public GetChunksMessageFlag Flag { get; set; }

        /// <summary>
        /// Gets or sets a count of hashes
        /// </summary>
        [ProtoMember(3)]
        public int HashesCount { get; set; }

        /// <summary>
        /// Gets or sets corresponding positions array of size HashesCount
        /// </summary>
        [ProtoMember(4)]
        public Vector2I[] Positions { get; set; }

        /// <summary>
        /// Gets or sets corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        [ProtoMember(5)]
        public Md5Hash[] Md5Hashes { get; set; }
    }

    public enum GetChunksMessageFlag : byte
    {
        /// <summary>
        /// Normal mode
        /// </summary>
        DontSendChunkDataIfNotModified = 0,
        /// <summary>
        /// Specify this flag if generated chunk is not equal to hash provided by the server
        /// </summary>
        AlwaysSendChunkData = 1
    }
}
