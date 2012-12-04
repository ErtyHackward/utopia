using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about entity position change event
    /// </summary>
    [ProtoContract]
    public struct EntityPositionMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityPosition; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        /// <summary>
        /// Gets or sets a current position of the player
        /// </summary>
        [ProtoMember(2)]
        public Vector3D Position { get; set; }
    }
}
