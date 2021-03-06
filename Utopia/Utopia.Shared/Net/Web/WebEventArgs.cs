using System;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web
{
    public class WebEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        [JsonProperty("error")]
        public int Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorText { get; set; }
    }

    public class WebEventArgs<T> : WebEventArgs
    {
        public T Response { get; set; }
    }
}