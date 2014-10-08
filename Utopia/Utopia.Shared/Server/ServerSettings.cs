using System;
using System.Xml.Serialization;
using S33M3CoreComponents.Config;

namespace Utopia.Shared.Server
{
    /// <summary>
    /// Server specific settings
    /// </summary>
    [XmlRoot("ServerSettings")]
    [Serializable]
    public class ServerSettings : IConfigClass
    {
        /// <summary>
        /// Interval between cleanup operation executions (milliseconds). All chunks older than ChunkLiveTimeMinutes parameter will be unloaded
        /// </summary>
        //[DefaultValue(60000)]
        public int CleanUpInterval { get; set; }

        /// <summary>
        /// Interval between save operation executions (milliseconds). All modified chunks will be saved into database. 
        /// </summary>
        //[DefaultValue(30000)]
        public int SaveInterval { get; set; }

        /// <summary>
        /// Defines maximum allowed chunk view range
        /// </summary>
        //[DefaultValue(20)]
        public int MaxViewRangeChunks { get; set; }

        /// <summary>
        /// Defines how long server will store unused chunks (minutes)
        /// </summary>
        //[DefaultValue(60)]
        public int ChunkLiveTimeMinutes { get; set; }

        /// <summary>
        /// Get maximum amount of chunks that can be stored in memory
        /// </summary>
        public int ChunksCountLimit { get; set; }

        /// <summary>
        /// Port to listen on
        /// </summary>
        //[DefaultValue(4815)]
        public int ServerPort { get; set; }

        /// <summary>
        /// Database file location
        /// </summary>
        public string DatabasePath { get; set; }

        /// <summary>
        /// Server global name
        /// </summary>
        //[DefaultValue("unnamed server")]
        public string ServerName { get; set; }

        public string Seed { get; set; }

        public string MessageOfTheDay { get; set; }

        public string ServerDescription { get; set; }

        public ServerSettings()
        {
            CleanUpInterval = 60000;
            SaveInterval = 30000;
            MaxViewRangeChunks = 20;
            ChunkLiveTimeMinutes = 60;
            ChunksCountLimit = 32768; // 16*16*128*32768 = 1Gb of memory 
            ServerPort = 4815;
            ServerName = "unnamed server";
        }

        public void Initialize()
        {
        }
    }
}
