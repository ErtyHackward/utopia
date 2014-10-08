using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class ServerListResponse : WebEventArgs
    {
        [JsonProperty("data")]
        public List<ServerInfo> Servers { get; set; }
    }

    // {"id":"83","userId":"10","name":"Test","address":"","count":"0","type":"0","lang":"","date":"2013-01-26 16:00:20","dateUpd":"2013-01-26 16:00:20"}

    [JsonObject]
    public struct ServerInfo
    {
        [JsonProperty("id")]
        public int ServerId { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string ServerName { get; set; }

        [JsonProperty("address")]
        public string ServerAddress { get; set; }

        [JsonProperty("localAddress")]
        public string LocalAddress { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("count")]
        public int UsersCount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
