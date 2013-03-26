using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class UserAuthenticateResponse
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }
}
