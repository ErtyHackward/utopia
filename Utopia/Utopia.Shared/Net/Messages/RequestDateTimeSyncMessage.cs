using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    [ProtoContract]
    public class RequestDateTimeSyncMessage : IBinaryMessage
    {
        public byte MessageId
        {
            get { return (byte)MessageTypes.RequestDateTimeSync; }
        }
    }
}
