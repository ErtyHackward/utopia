using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    // todo: the class should be moved to Utopia.Server project

    /// <summary>
    /// Represents a chunk to use in server
    /// </summary>
    public class ServerChunk : CompressibleChunk
    {
        /// <summary>
        /// Gets or sets current chunk position
        /// </summary>
        public IntVector2 Position { get; set; }

        /// <summary>
        /// Indicates that we no need to send bytes to client, it can obtain in using generator
        /// </summary>
        public bool PureGenerated { get; set; }

        /// <summary>
        /// DateTime stamp of the chunk. Determines whether the chunk can be unloaded from memory
        /// </summary>
        public DateTime LastAccess { get; set; }

        public ServerChunk() : base(new InsideDataProvider())
        {
            
        }
    }
}
