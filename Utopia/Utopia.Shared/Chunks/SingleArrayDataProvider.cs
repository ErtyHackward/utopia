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
            int byteArrayIndex = 0;
            int baseCubeIndex = ChunkCubes.Index(DataProviderUser.ChunkPositionBlockUnit.X, 0, DataProviderUser.ChunkPositionBlockUnit.Y);
            int CubeIndexX = baseCubeIndex;
            int CubeIndexZ = baseCubeIndex;
            int cubeIndex = baseCubeIndex;

            byte[] extractedCubes = new byte[AbstractChunk.ChunkBlocksByteLength];

            for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
            {
                if (Z != 0) { CubeIndexZ += ChunkCubes.MoveZ; CubeIndexX = CubeIndexZ; cubeIndex = CubeIndexZ; }

                for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
                {
                    if (X != 0) { CubeIndexX += ChunkCubes.MoveX; cubeIndex = CubeIndexX; }

                    for (int Y = 0; Y < AbstractChunk.ChunkSize.Y; Y++)
                    {
                        if (Y != 0) { cubeIndex += ChunkCubes.MoveY; }

                        extractedCubes[byteArrayIndex] = ChunkCubes.Cubes[cubeIndex].Id;
                        byteArrayIndex++;
                    }
                }
            }

            return extractedCubes;
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Vector3I inChunkPosition)
        {
            return ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + DataProviderUser.ChunkPositionBlockUnit.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + DataProviderUser.ChunkPositionBlockUnit.Y)].Id;
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue)
        {
            ChunkCubes.Cubes[ChunkCubes.Index(inChunkPosition.X + DataProviderUser.ChunkPositionBlockUnit.X,
                                                     inChunkPosition.Y,
                                                     inChunkPosition.Z + DataProviderUser.ChunkPositionBlockUnit.Y)] = new TerraCube(blockValue);

            //Init ChunkCubes.CubesMetaData[] ???
            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { inChunkPosition }, Bytes = new[] { blockValue } });
        }

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        public override void SetBlocks(Vector3I[] positions, byte[] values)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                ChunkCubes.Cubes[ChunkCubes.Index(positions[i].X + DataProviderUser.ChunkPositionBlockUnit.X,
                                         positions[i].Y,
                                         positions[i].Z + DataProviderUser.ChunkPositionBlockUnit.Y)] = new TerraCube(values[i]);
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
            int byteArrayIndex = 0;
            int baseCubeIndex = ChunkCubes.Index(DataProviderUser.ChunkPositionBlockUnit.X, 0, DataProviderUser.ChunkPositionBlockUnit.Y);
            int CubeIndexX = baseCubeIndex;
            int CubeIndexZ = baseCubeIndex;
            int cubeIndex = baseCubeIndex;
            for (int Z = 0; Z < AbstractChunk.ChunkSize.Z;Z++)
            {
                if (Z != 0) { CubeIndexZ += ChunkCubes.MoveZ; CubeIndexX = CubeIndexZ; cubeIndex = CubeIndexZ; }

                for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
                {
                    if (X != 0) { CubeIndexX += ChunkCubes.MoveX; cubeIndex = CubeIndexX; }

                    for (int Y = 0; Y < AbstractChunk.ChunkSize.Y; Y++)
                    {
                        if (Y != 0) { cubeIndex += ChunkCubes.MoveY; }

                        ChunkCubes.Cubes[cubeIndex] = new TerraCube(bytes[byteArrayIndex]);
                        byteArrayIndex++;
                    }
                }
            }


            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs() { NewBuffer = bytes });
        }

        #endregion

    }
}