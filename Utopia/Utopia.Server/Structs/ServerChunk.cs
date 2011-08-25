using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Represents a chunk to use in server
    /// </summary>
    public class ServerChunk : CompressibleChunk
    {
        /// <summary>
        /// Indicates that we no need to send bytes to client, it can obtain in using generator
        /// </summary>
        public bool PureGenerated { get; set; }

        /// <summary>
        /// Gets or sets current chunk position
        /// </summary>
        public IntVector2 Position { get; set; }

        /// <summary>
        /// DateTime stamp of the chunk. Determines whether the chunk can be unloaded from memory
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        /// Indicates if the chunk needs to be saved
        /// </summary>
        public bool NeedSave { get; set; }

        public ServerChunk() : base(new InsideDataProvider())
        {
            InstantCompress = true;
        }

        /// <summary>
        /// Creates new instance of the server chunk from generator
        /// </summary>
        /// <param name="chunk"></param>
        public ServerChunk(GeneratedChunk chunk)
            : base(chunk.BlockData)
        {
            PureGenerated = true;
            InstantCompress = true;
        }

        protected override void BlockBufferChanged(object sender, ChunkDataProviderBufferChangedEventArgs e)
        {
            base.BlockBufferChanged(sender, e);
        }

        protected override void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            PureGenerated = false;
            base.BlockDataChanged(sender, e);
        }
    }
}
