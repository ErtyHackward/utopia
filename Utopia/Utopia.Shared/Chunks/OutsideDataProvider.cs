using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds data inside the single big-buffer
    /// </summary>
    public class OutsideDataProvider : ChunkDataProvider
    {
        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            //todo: implement requesting data from a big buffer
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Location3<int> inChunkPosition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Location3<int> inChunkPosition, byte blockValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        public override void SetBlocks(Location3<int>[] positions, byte[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetBlockBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}