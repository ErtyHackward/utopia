using System;
using ProtoBuf;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using System.Collections.Generic;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds data inside the single big-buffer
    /// </summary>
    [ProtoContract]
    public class SingleArrayDataProvider : ChunkDataProvider
    {
        private readonly object _syncRoot = new object();
        private Vector3I _chunkSize;
        private readonly Dictionary<Vector3I, BlockTag> _tags = new Dictionary<Vector3I, BlockTag>();

        //A reference to the class using this DataProvider.
        public ISingleArrayDataProviderUser DataProviderUser { get; set; }
        public SingleArrayChunkContainer ChunkCubes { get; set; }
        public ChunkColumnInfo[] ChunkColumns;
        private ChunkMetaData _chunkMetaData;

        #region Serialization

        [ProtoMember(1)]
        public override Vector3I ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        [ProtoMember(2, OverwriteList = true)]
        public byte[] SerializeBytes {
            get { return GetBlocksBytes(); }
            set { SetBlockBytes(value); }
        }

        [ProtoMember(3)]
        public Dictionary<Vector3I, BlockTag> SerializeTags
        {
            get { return _tags; }
        }

        [ProtoMember(4)]
        public ChunkMetaData SerializeChunkMetaData
        {
            get { return _chunkMetaData; }
            set { _chunkMetaData = value; }
        }

        [ProtoMember(5, OverwriteList = true)]
        public ChunkColumnInfo[] SerializeChunkColums
        {
            get { return ChunkColumns; }
            set { ChunkColumns = value; }
        }

        #endregion

        public SingleArrayDataProvider()
        {
            _chunkSize = AbstractChunk.ChunkSize;
            ChunkColumns = new ChunkColumnInfo[_chunkSize.X * _chunkSize.Z];
            _chunkMetaData = new ChunkMetaData();
        }

        public SingleArrayDataProvider(SingleArrayChunkContainer singleArrayContainer) : this()
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

            for (int Z = 0; Z < _chunkSize.Z; Z++)
            {
                if (Z != 0) { CubeIndexZ += ChunkCubes.MoveZ; CubeIndexX = CubeIndexZ; cubeIndex = CubeIndexZ; }

                for (int X = 0; X < _chunkSize.X; X++)
                {
                    if (X != 0) { CubeIndexX += ChunkCubes.MoveX; cubeIndex = CubeIndexX; }

                    for (int Y = 0; Y < _chunkSize.Y; Y++)
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
        /// <param name="tag"></param>
        /// <param name="sourceDynamicId">Id of the entity that is responsible for the change</param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null, uint sourceDynamicId = 0)
        {
            int index = ChunkCubes.Index(inChunkPosition.X + DataProviderUser.ChunkPositionBlockUnit.X,
                                         inChunkPosition.Y,
                                         inChunkPosition.Z + DataProviderUser.ChunkPositionBlockUnit.Y);

            ChunkCubes.Cubes[index] = new TerraCube(blockValue);

            SetTag(tag, inChunkPosition);

            RefreshMetaData(BlockHelper.ConvertToGlobal(new Vector3I(DataProviderUser.ChunkPositionBlockUnit.X, 0, DataProviderUser.ChunkPositionBlockUnit.Y), inChunkPosition), blockValue);

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs
                                   {
                                       Locations = new[] { inChunkPosition },
                                       Bytes = new[] { blockValue },
                                       Tags = tag != null ? new[] { tag } : null,
                                       SourceDynamicId = sourceDynamicId
                                   });
        }

        /// <summary>
        /// Operation is not supported
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="blockValue"></param>
        public void SetBlockWithoutEvents(Vector3I worldPosition, byte blockValue)
        {
            int index = ChunkCubes.Index(worldPosition.X, worldPosition.Y, worldPosition.Z);
            ChunkCubes.Cubes[index] = new TerraCube(blockValue);
            RefreshMetaData(worldPosition, blockValue);
        }

        private void RefreshMetaData(Vector3I worldPosition, byte newBlockValue)
        {
            //From World Coordinate to Chunk Coordinate
            int arrayX = MathHelper.Mod(worldPosition.X, AbstractChunk.ChunkSize.X);
            int arrayZ = MathHelper.Mod(worldPosition.Z, AbstractChunk.ChunkSize.Z);
            //Compute 2D index of ColumnInfo and update ColumnInfo
            int index2D = arrayX * AbstractChunk.ChunkSize.Z + arrayZ;

            if (newBlockValue != WorldConfiguration.CubeId.Air)
            {
                //Change being made above surface !
                if (ColumnsInfo[index2D].MaxHeight < worldPosition.Y)
                {
                    ColumnsInfo[index2D].MaxHeight = (byte)worldPosition.Y;
                    ChunkMetaData.setChunkMaxHeightBuilt((byte)worldPosition.Y);
                    if (ColumnsInfo[index2D].IsWild)
                    {
                        ColumnsInfo[index2D].IsWild = false;
                        ChunkMetaData.setChunkWildStatus(ColumnsInfo);
                    }
                }
            }
            else
            {
                //Change being made at the surface (Block removed)
                if (ColumnsInfo[index2D].MaxHeight <= worldPosition.Y)
                {
                    int yPosi = worldPosition.Y - 1;
                    int index = ChunkCubes.Index(worldPosition.X, yPosi, worldPosition.Z);
                    while (ChunkCubes.Cubes[index].Id == WorldConfiguration.CubeId.Air && yPosi > 0)
                    {
                        index = ChunkCubes.FastIndex(index, yPosi, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1, false);
                        yPosi--;
                    }
                    ChunkMetaData.setChunkMaxHeightBuilt((byte)yPosi);
                    if (ColumnsInfo[index2D].IsWild)
                    {
                        ColumnsInfo[index2D].IsWild = false;
                        ChunkMetaData.setChunkWildStatus(ColumnsInfo);
                    }
                }
            }
        }
        
        /// <summary>
        /// Sets a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        /// <param name="tags"> </param>
        /// <param name="sourceDynamicId">Id of the entity that is responsible for the change</param>
        public override void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null, uint sourceDynamicId = 0)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                SetBlock(positions[i], values[i], tags == null ? null : tags[i], sourceDynamicId);
            }
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tags"> </param>
        public override void SetBlockBytes(byte[] bytes, IEnumerable<KeyValuePair<Vector3I, BlockTag>> tags = null)
        {
            lock (_syncRoot)
            {
                int byteArrayIndex = 0;
                int baseCubeIndex = ChunkCubes.Index(DataProviderUser.ChunkPositionBlockUnit.X, 0,
                                                     DataProviderUser.ChunkPositionBlockUnit.Y);
                int CubeIndexX = baseCubeIndex;
                int CubeIndexZ = baseCubeIndex;
                int cubeIndex = baseCubeIndex;
                for (int Z = 0; Z < _chunkSize.Z; Z++)
                {
                    if (Z != 0)
                    {
                        CubeIndexZ += ChunkCubes.MoveZ;
                        CubeIndexX = CubeIndexZ;
                        cubeIndex = CubeIndexZ;
                    }

                    for (int X = 0; X < _chunkSize.X; X++)
                    {
                        if (X != 0)
                        {
                            CubeIndexX += ChunkCubes.MoveX;
                            cubeIndex = CubeIndexX;
                        }

                        for (int Y = 0; Y < _chunkSize.Y; Y++)
                        {
                            if (Y != 0)
                            {
                                cubeIndex += ChunkCubes.MoveY;
                            }

                            ChunkCubes.Cubes[cubeIndex] = new TerraCube(bytes[byteArrayIndex]);
                            byteArrayIndex++;
                        }
                    }
                }

                _tags.Clear();

                if (tags != null)
                {
                    foreach (var pair in tags)
                    {
                        _tags.Add(pair.Key, pair.Value);
                    }
                }
            }
            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs { NewBuffer = bytes });
        }

        #endregion

        #region Chunk Column Information Manager
        public override ChunkColumnInfo[] ColumnsInfo
        {
            get
            {
                return ChunkColumns;
            }
            set
            {
                ChunkColumns = value;
            }
        }

        public override ChunkColumnInfo GetColumnInfo(int inChunkPositionX, int inChunkPositionZ)
        {
            return ChunkColumns[inChunkPositionX * _chunkSize.Z + inChunkPositionZ];
        }
        #endregion

        public override ChunkMetaData ChunkMetaData
        {
            get
            {
                return _chunkMetaData;
            }
            set
            {
                _chunkMetaData = value;
            }
        }

        public override BlockTag GetTag(Vector3I inChunkPosition)
        {
            BlockTag result;
            _tags.TryGetValue(inChunkPosition, out result);
            return result == null ? null : (BlockTag)result.Clone();
        }

        public void SetTag(BlockTag tag, Vector3I inChunkPosition)
        {
            lock (_syncRoot)
            {
                if (tag != null)
                    _tags[inChunkPosition] = (BlockTag)tag.Clone();
                else
                    _tags.Remove(inChunkPosition);
            }
        }

        public override IEnumerable<KeyValuePair<Vector3I, BlockTag>> GetTags()
        {
            lock (_syncRoot)
            {
                foreach (var pair in _tags)
                {
                    yield return pair;    
                }
            }
        }

        public override object WriteSyncRoot
        {
            get { return _syncRoot; }
        }
    }
}