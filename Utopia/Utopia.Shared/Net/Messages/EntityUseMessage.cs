using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs server about client tool using
    /// </summary>
    [ProtoContract]
    public struct EntityUseMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification number of entity that performs use operation (player or NPC)
        /// </summary>
        [ProtoMember(1)]
        public uint DynamicEntityId { get; set; }

        [ProtoMember(2)]
        public Vector3I PickedBlockPosition { get; set; }

        [ProtoMember(3)]
        public Vector3I NewBlockPosition { get; set; }

        /// <summary>
        /// Gets or sets Tool Entity Id that performs action
        /// </summary>
        [ProtoMember(4)]
        public uint ToolId { get; set; }

        /// <summary>
        /// Picked entity position (optional)
        /// </summary>
        [ProtoMember(5)]
        public Vector3D PickedEntityPosition { get; set; }

        /// <summary>
        /// Picked entity link (optional)
        /// </summary>
        [ProtoMember(6)]
        public EntityLink PickedEntityLink { get; set; }

        [ProtoMember(7)]
        public bool IsBlockPicked { get; set; }

        [ProtoMember(8)]
        public bool IsEntityPicked { get; set; }

        [ProtoMember(9)]
        public Vector3 PickedBlockFaceOffset { get; set; }

        /// <summary>
        /// Identification token of the use operation
        /// </summary>
        [ProtoMember(10)]
        public int Token { get; set; }

        /// <summary>
        /// Gets message id (cast to MessageTypes enumeration)
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityUse; }
        }
    }
}
