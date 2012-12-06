using ProtoBuf;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a message containing a voxel model
    /// </summary>
    [ProtoContract]
    public class VoxelModelDataMessage : IBinaryMessage
    {

        public byte MessageId
        {
            get { return (byte)MessageTypes.VoxelModelData; }
        }

        [ProtoMember(1)]
        public VoxelModel VoxelModel { get; set; }
    }
}
