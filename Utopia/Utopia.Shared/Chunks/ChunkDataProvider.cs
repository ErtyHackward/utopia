using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base class for chunks block storage
    /// </summary>
    public abstract class ChunkDataProvider
    {
        /// <summary>
        /// Gets or sets current chunk position
        /// </summary>
        public IntVector2 ChunkPosition { get; set; }

        /// <summary>
        /// Occurs when block data was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderDataChangedEventArgs> BlockDataChanged;

        protected void OnBlockDataChanged(ChunkDataProviderDataChangedEventArgs e)
        {
            if (BlockDataChanged != null) 
                BlockDataChanged(this, e);
        }

        /// <summary>
        /// Ocurrs when whole buffer was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderBufferChangedEventArgs> BlockBufferChanged;

        protected void OnBlockBufferChanged(ChunkDataProviderBufferChangedEventArgs e)
        {
            if (BlockBufferChanged != null)
                BlockBufferChanged(this, e);
        }

        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetBlocksBytes();

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract byte GetBlock(Location3<int> inChunkPosition);

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public abstract void SetBlock(Location3<int> inChunkPosition, byte blockValue);

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        public abstract void SetBlocks(Location3<int>[] positions, byte[] values);

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        public abstract void SetBlockBytes(byte[] bytes);

        /// <summary>
        /// Gets or sets a block in the buffer
        /// </summary>
        /// <param name="position">Local position of the block</param>
        /// <returns></returns>
        public byte this[Location3<int> position]
        {
            get { return GetBlock(position); }
            set { SetBlock(position, value); }
        }
    }
}
