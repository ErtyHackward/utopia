using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Net.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.World;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Describes a message used to send game specified information to client
    /// </summary>
    [ProtoContract]
    public class GameInformationMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GameInformation; }
        }

        /// <summary>
        /// Gets or sets a maximum chunk distance that client can query (in chunks)
        /// </summary>
        [ProtoMember(1)]
        public int MaxViewRange { get; set; }

        /// <summary>
        /// Gets or sets a chunk size used on the server
        /// </summary>
        [ProtoMember(2)]
        public Vector3I ChunkSize { get; set; }

        /// <summary>
        /// Contains plan generation details
        /// </summary>
        [ProtoMember(3)]
        public WorldParameters WorldParameter { get; set; }

        /// <summary>
        /// Contains gameplay global stuff like list of factions
        /// </summary>
        [ProtoMember(4)]
        public GlobalState GlobalState { get; set; }

        /// <summary>
        /// Holds size of the server area
        /// </summary>
        [ProtoMember(5)]
        public Vector2I AreaSize { get; set; }
    }
}
