using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs about voxel model change
    /// </summary>
    [ProtoContract]
    public class EntityVoxelModelMessage : IBinaryMessage
    {
        /// <summary>
        /// Link to the entity
        /// </summary>
        [ProtoMember(1)]
        public EntityLink EntityLink { get; set; }

        /// <summary>
        /// New character class name
        /// </summary>
        [ProtoMember(2)]
        public string ClassName { get; set; }

        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityVoxelModel; }
        }
    }
}
