using System;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base class for a voxel block storage
    /// </summary>
    public abstract class ChunkDataProvider
    {
        /// <summary>
        /// Occurs when block data was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderDataChangedEventArgs> BlockDataChanged;

        public void OnBlockDataChanged(ChunkDataProviderDataChangedEventArgs e)
        {
            var handler = BlockDataChanged;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// Ocurrs when whole buffer was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderBufferChangedEventArgs> BlockBufferChanged;

        public void OnBlockBufferChanged(ChunkDataProviderBufferChangedEventArgs e)
        {
            var handler = BlockBufferChanged;
            if (handler != null) handler(this, e);
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
        public abstract byte GetBlock(Vector3I inChunkPosition);

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public abstract void SetBlock(Vector3I inChunkPosition, byte blockValue);

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        public abstract void SetBlocks(Vector3I[] positions, byte[] values);

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
        public byte this[Vector3I position]
        {
            get { return GetBlock(position); }
            set { SetBlock(position, value); }
        }
    }
}
