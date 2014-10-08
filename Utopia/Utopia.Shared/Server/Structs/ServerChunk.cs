using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Represents a chunk to use in server
    /// </summary>
    public class ServerChunk : CompressibleChunk
    {
        /// <summary>
        /// Indicates that we no need to send bytes to client, it can obtain the chunk locally using its generator
        /// </summary>
        public bool PureGenerated { get; set; }
        
        /// <summary>
        /// DateTime stamp of the chunk. Determines whether the chunk can be unloaded from memory
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        /// Indicates if the chunk needs to be saved
        /// </summary>
        public bool NeedSave { get; set; }

        /// <summary>
        /// Gets or sets last chunk refresh in game time
        /// </summary>
        public UtopiaTime LastSpawningRefresh { get; set; }

        /// <summary>
        /// Occurs when some of containing blocks was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderDataChangedEventArgs> BlocksChanged;

        private void OnBlocksChanged(ChunkDataProviderDataChangedEventArgs e)
        {
            var handler = BlocksChanged;
            if (handler != null) handler(this, e);
        }

        public ServerChunk() : base(new InsideDataProvider())
        {
        }

        /// <summary>
        /// Creates new instance of the server chunk from generator
        /// </summary>
        /// <param name="chunk"></param>
        public ServerChunk(GeneratedChunk chunk)
            : base(chunk.BlockData)
        {
            Entities.Import(chunk.Entities);
            PureGenerated = true;
        }

        /// <summary>
        /// Handling of chunk blocks data change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            PureGenerated = false;

            OnBlocksChanged(e);

            base.BlockDataChanged(sender, e);
        }
    }
}
