using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Message to synchronise an entity Health change
    /// </summary>
    [ProtoContract]
    public class EntityHealthMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityHealth; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        /// <summary>
        /// New Health of the entity
        /// </summary>
        [ProtoMember(2)]
        public Energy Health { get; set; }

        /// <summary>
        /// Health change
        /// </summary>
        [ProtoMember(3)]
        public float Change { get; set; }
    }
}
