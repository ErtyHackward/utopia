using System;
using System.ComponentModel;

namespace Utopia.Server
{
    /// <summary>
    /// Server specific settings
    /// </summary>
    [Serializable]
    public class ServerSettings
    {
        /// <summary>
        /// Interval between cleanup operation executions. All chunks older than ChunkLiveTimeMinutes parameter will be unloaded
        /// </summary>
        [DefaultValue(60000)]
        public int CleanUpInterval { get; set; }
        /// <summary>
        /// Interval between save operation executions. All modified chunks will be saved into database. 
        /// </summary>
        [DefaultValue(10000)]
        public int SaveInterval { get; set; }
        /// <summary>
        /// Defines maximum allowed chunk view range
        /// </summary>
        [DefaultValue(20)]
        public int MaxViewRangeChunks { get; set; }
        /// <summary>
        /// Defines how long server will store unused chunks
        /// </summary>
        [DefaultValue(60)]
        public int ChunkLiveTimeMinutes { get; set; }
        /// <summary>
        /// Port to listen on
        /// </summary>
        [DefaultValue(4815)]
        public int ServerPort { get; set; }
        /// <summary>
        /// Database file location
        /// </summary>
        public string DatabasePath { get; set; }
    }
}
