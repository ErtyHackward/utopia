using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds block data inside the chunk
    /// </summary>
    public class InsideDataProvider : ChunkDataProvider
    {
        /// <summary>
        /// Gets or sets the inside buffer
        /// </summary>
        public byte[] BlockBytes { get; set; }

        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            return BlockBytes;
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetBlockBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (bytes.Length != AbstractChunk.ChunkBlocksByteLength)
                throw new ArgumentOutOfRangeException(string.Format("Wrong block buffer size. Expected: {0}, Actual: {1}", AbstractChunk.ChunkBlocksByteLength, bytes.Length));

            BlockBytes = bytes;
            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs { NewBuffer = bytes });
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Vector3I inChunkPosition)
        {
            return BlockBytes[inChunkPosition.X * AbstractChunk.ChunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * AbstractChunk.ChunkSize.Y * AbstractChunk.ChunkSize.X];
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue)
        {
            if (BlockBytes == null)
            {
                BlockBytes = new byte[AbstractChunk.ChunkBlocksByteLength];
            }
            BlockBytes[inChunkPosition.X * AbstractChunk.ChunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * AbstractChunk.ChunkSize.Y * AbstractChunk.ChunkSize.X] = blockValue;

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { inChunkPosition }, Bytes = new[] { blockValue } });
        }

        /// <summary>
        /// Sets a group of blocks
        /// </summary>
        /// <param name="positions">internal chunk positions</param>
        /// <param name="values"></param>
        public override void SetBlocks(Vector3I[] positions, byte[] values)
        {
            if (BlockBytes == null)
            {
                BlockBytes = new byte[AbstractChunk.ChunkBlocksByteLength];
            }

            for (var i = 0; i < positions.Length; i++)
            {
                BlockBytes[positions[i].X * AbstractChunk.ChunkSize.Y + positions[i].Y + positions[i].Z * AbstractChunk.ChunkSize.Y * AbstractChunk.ChunkSize.X] = values[i];    
            }

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = positions.Length, Locations = positions, Bytes = values });
        }
    }
}