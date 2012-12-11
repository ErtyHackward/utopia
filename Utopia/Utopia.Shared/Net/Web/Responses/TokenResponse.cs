using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        public string DisplayName { get; set; }
    }
}
