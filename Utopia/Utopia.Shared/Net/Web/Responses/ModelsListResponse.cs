using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web.Responses
{
    [JsonObject]
    public class ModelsListResponse : WebEventArgs
    {
        [JsonProperty("models")]
        public List<ModelInfo> Models { get; set; }
    }

     /*{
        "id": "143",
        "name": "Wallplank4",
        "description": null,
        "sizeX": "0",
        "sizeY": "0",
        "sizeZ": "0",
        "userId": "3582",
        "createDate": "2014-03-25 11:20:07",
        "updateDate": "2014-03-25 11:36:50",
        "views": "0",
        "downloads": "0",
        "fileName": "/public/uploads/models/fileName/143/Wallplank4.uvm",
        "fileSize": "1034",
        "screen": "/public/uploads/models/screen/143/Wallplank4.png",
        "forumPath": "viewtopic.php?f=11&amp;t=262"
    }*/

    [JsonObject]
    public struct ModelInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("updateDate")]
        public DateTime Updated { get; set; }
    }
}