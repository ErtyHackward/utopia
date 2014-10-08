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
    /// Message to synchronise an entity Affliction state
    /// </summary>
    [ProtoContract]
    public class EntityAfflictionStateMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityAfflictionState; }
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
        public DynamicEntityAfflictionState AfflictionState { get; set; }
    }
}
