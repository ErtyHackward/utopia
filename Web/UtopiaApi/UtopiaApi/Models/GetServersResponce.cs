using ProtoBuf;

namespace UtopiaApi.Models
{
    [ProtoContract]
    public class GetServersResponce
    {
        [ProtoMember(1)]
        public ServerInfo[] Servers { get; set; }
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