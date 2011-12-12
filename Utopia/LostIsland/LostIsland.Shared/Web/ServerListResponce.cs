using System.Collections.Generic;
using ProtoBuf;

namespace UtopiaApi.Models
{
    [ProtoContract]
    public class ServerListResponce
    {
        [ProtoMember(1)]
        public List<ServerInfo> Servers { get; set; }
    }

    [ProtoContract]
    public struct ServerInfo
    {
        [ProtoMember(1)]
        public string ServerName { get; set; }
        [ProtoMember(2)]
        public string ServerAddress { get; set; }
    }
}