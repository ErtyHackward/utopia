using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class TokenResponse : WebEventArgs
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        public string DisplayName { get; set; }
    }
}
