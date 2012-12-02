using System.IO;
using System.Runtime.InteropServices;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that inform about change in view direction of the entity
    /// </summary>
    [ProtoContract]
    public struct EntityBodyDirectionMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityDirection; }
        }

        /// <summary>
        /// Gets or sets an entity identification number
        /// </summary>
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        /// <summary>
        /// Gets or sets an actual direction quaternion of the entity
        /// </summary>
        [ProtoMember(2)]
        public Quaternion Rotation { get; set; }
    }
}
