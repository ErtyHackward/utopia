using System;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds data inside the single big-buffer
    /// </summary>
    public class SingleArrayDataProvider : ChunkDataProvider
    {
        #region Direct circular Array access
        /// <summary>
        /// Static class responsible to manage the Circular array of cubes.
        /// </summary>
        public static SingleArrayChunkCube ChunkCubes { get; set; }
        #endregion

        #region Circular Array access through chunk
        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            byte[] extractedCubes = new byte[AbstractChunk.ChunkBlocksByteLength];

            int index = ChunkCubes.Index(ChunkPosition.X, 0, ChunkPosition.Y);
            //Depending of the layout of the byte !! (Order)
            for (int i = 0; i < AbstractChunk.ChunkBlocksByteLength; i++)
            {
                extractedCubes[index] = ChunkCubes.Cubes[index].Id;
                index++;
            }

            return extractedCubes;
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Location3<int> inChunkPosition)
        {
            return ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + ChunkPosition.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + ChunkPosition.Y)].Id;
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Location3<int> inChunkPosition, byte blockValue)
        {
            ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + ChunkPosition.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + ChunkPosition.Y)].Id = blockValue;
        }

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        public override void SetBlocks(Location3<int>[] positions, byte[] values)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                ChunkCubes.Cubes[ChunkCubes.Index(positions[i].X + ChunkPosition.X,
                                         positions[i].Y,
                                         positions[i].Z + ChunkPosition.Y)].Id = values[i];
            }

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs() { Bytes = values, Count = values.Length, Locations = positions} );
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetBlockBytes(byte[] bytes)
        {
            int index = ChunkCubes.Index(ChunkPosition.X, 0, ChunkPosition.Y);
            //Depending of the layout of the byte !! (Order)
            for (int i = 0; i < bytes.Length; i++)
            {
                ChunkCubes.Cubes[index].Id = bytes[i];
                index++;
            }

            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs() { NewBuffer = bytes });
        }

        #endregion

    }
}