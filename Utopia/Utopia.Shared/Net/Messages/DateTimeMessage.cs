using System;
using ProtoBuf;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform clients about current time and date
    /// </summary>
    [ProtoContract]
    public class DateTimeMessage : IBinaryMessage
    {
        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.DateTime; }
        }

        /// <summary>
        /// Gets or sets game DateTime
        /// </summary>
        [ProtoMember(1)]
        public UtopiaTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets how many game seconds in one real second
        /// </summary>
        [ProtoMember(2)]
        public double TimeFactor { get; set; }
    }
}
