using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that can be sent by the server in responce to the GetChunks message. 
    /// </summary>
    [ProtoContract]
    public class ChunkDataMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.ChunkData; }
        }

        /// <summary>
        /// Value filled in with the datetime at message creation time
        /// </summary>
        public System.DateTime MessageRecTime { get; set; }

        /// <summary>
        /// Gets or sets chunk position
        /// </summary>
        [ProtoMember(1)]
        public Vector2I Position { get; set; }

        /// <summary>
        /// Gets or sets result type flag. See the flag members to details
        /// </summary>
        [ProtoMember(2)]
        public ChunkDataMessageFlag Flag { get; set; }

        /// <summary>
        /// Gets or sets chunk md5 hash
        /// </summary>
        [ProtoMember(3)]
        public Md5Hash ChunkHash { get; set; }

        /// <summary>
        /// Gets or sets variable amount of bytes, can be the chunk data or md5 hash (depends on flag state)
        /// </summary>
        [ProtoMember(4)]
        public byte[] Data { get; set; }
    }

    public enum ChunkDataMessageFlag : byte
    {
        /// <summary>
        /// Client should use chunk data from the server (will be attached to this message)
        /// </summary>
        ChunkWasModified = 0,
        /// <summary>
        /// In this case data will contain md5 hash of resulted chunk, the client shuold generate the chunk
        /// </summary>
        ChunkCanBeGenerated = 1,
        /// <summary>
        /// In this case the message will contain no data, the client should obtain the chunk data locally
        /// </summary>
        ChunkMd5Equal = 2
    }

}
