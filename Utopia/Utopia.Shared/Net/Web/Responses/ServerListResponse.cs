using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class ServerListResponse
    {
        public List<ServerInfo> Servers { get; set; }
    }

    [JsonObject]
    public struct ServerInfo
    {
        public string ServerName { get; set; }
        
        public string ServerAddress { get; set; }
        
        public uint UsersCount { get; set; }
    }
}
