using System;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds data inside the single big-buffer
    /// </summary>
    public class SingleArrayDataProvider : ChunkDataProvider
    {
        //A reference to the class using this DataProvider.
        public ISingleArrayDataProviderUser DataProviderUser {get; set;}
        public SingleArrayChunkContainer ChunkCubes { get; set; }

        public SingleArrayDataProvider(SingleArrayChunkContainer singleArrayContainer)
        {
            ChunkCubes = singleArrayContainer;
        }

        #region Circular Array access through chunk
        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            //byte[] extractedCubes = new byte[AbstractChunk.ChunkBlocksByteLength];

            //int index = ChunkCubes.Index(DataProviderUser.ChunkPosition.X, 0, DataProviderUser.ChunkPosition.Y);
            ////Depending of the layout of the byte !! (Order)
            //for (int i = 0; i < AbstractChunk.ChunkBlocksByteLength; i++)
            //{
            //    extractedCubes[index] = ChunkCubes.Cubes[index].Id;
            //    index++;
            //}

            //return extractedCubes;

            byte[] extractedCubes = new byte[AbstractChunk.ChunkBlocksByteLength];
            int index = ChunkCubes.Index(DataProviderUser.ChunkPosition.X, 0, DataProviderUser.ChunkPosition.Y);
            Array.Copy(ChunkCubes.Cubes, index, extractedCubes, 0, AbstractChunk.ChunkBlocksByteLength);
            return extractedCubes;
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Location3<int> inChunkPosition)
        {
            return ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + DataProviderUser.ChunkPosition.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + DataProviderUser.ChunkPosition.Y)];
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Location3<int> inChunkPosition, byte blockValue)
        {
            ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + DataProviderUser.ChunkPosition.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + DataProviderUser.ChunkPosition.Y)] = blockValue;

            //Init ChunkCubes.CubesMetaData[] ???
            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { inChunkPosition }, Bytes = new[] { blockValue } });
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
                ChunkCubes.Cubes[ChunkCubes.Index(positions[i].X + DataProviderUser.ChunkPosition.X,
                                         positions[i].Y,
                                         positions[i].Z + DataProviderUser.ChunkPosition.Y)] = values[i];
            }

            //Init ChunkCubes.CubesMetaData[] ???
            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs() { Bytes = values, Count = values.Length, Locations = positions} );
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetBlockBytes(byte[] bytes)
        {
            ////Depending of the layout of the byte !! (Order)
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    ChunkCubes.Cubes[index].Id = bytes[i];
            //    index++;
            //}

            int index = ChunkCubes.Index(DataProviderUser.ChunkPosition.X, 0, DataProviderUser.ChunkPosition.Y);
            Array.Copy(bytes, 0, ChunkCubes.Cubes, index, AbstractChunk.ChunkBlocksByteLength);

            //Init ChunkCubes.CubesMetaData[] ???

            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs() { NewBuffer = bytes });
        }

        #endregion

    }
}