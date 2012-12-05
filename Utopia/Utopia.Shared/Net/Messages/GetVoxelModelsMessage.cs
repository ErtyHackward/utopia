using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a request for one or more voxel models
    /// </summary>
    [ProtoContract]
    public class GetVoxelModelsMessage : IBinaryMessage
    {
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetVoxelModels; }
        }

        /// <summary>
        /// Gets or sets a set of md5 hash values of requested models
        /// </summary>
        [ProtoMember(1)]
        public string[] Names { get; set; }
    }
}
