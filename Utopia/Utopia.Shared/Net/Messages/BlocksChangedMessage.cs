using ProtoBuf;
using Utopia.Shared.Chunks;
using Utopia.Shared.Net.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// BlockChange message is sent when some entity or player does modification of some amount of blocks 
    /// </summary>
    [ProtoContract]
    public class BlocksChangedMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.BlockChange; }
        }

        /// <summary>
        /// Gets or sets global blocks position
        /// </summary>
        [ProtoMember(1)]
        public Vector3I[] BlockPositions { get; set; }

        /// <summary>
        /// Gets or sets blocks id values
        /// </summary>
        [ProtoMember(2)]
        public byte[] BlockValues { get; set; }

        /// <summary>
        /// According block tags (can be null, otherwise tags array will correspond the order of BlockPosition array, each value of the array still can be null)
        /// </summary>
        [ProtoMember(3)]
        public BlockTag[] Tags { get; set; }
    }
}
