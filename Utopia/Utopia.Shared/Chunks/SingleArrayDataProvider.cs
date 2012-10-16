using System;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using System.IO;
using Utopia.Shared.Entities;
using System.Collections.Generic;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds data inside the single big-buffer
    /// </summary>
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

        public SingleArrayDataProvider(SingleArrayChunkContainer singleArrayContainer)
        {
            ChunkCubes = singleArrayContainer;
            _chunkSize = AbstractChunk.ChunkSize;
            ChunkColumns = new ChunkColumnInfo[_chunkSize.X * _chunkSize.Z];
            _chunkMetaData = new ChunkMetaData();
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
        /// Operation is not supported
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        /// <param name="tag"></param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Operation is not supported
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        /// <param name="tags"></param>
        /// <exception cref="NotSupportedException"></exception>
        public override void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null)
        {
            throw new NotSupportedException();
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

                            //By default if air block set sunlight to maximum
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

        public override ChunkColumnInfo GetColumnInfo(Vector3I inChunkPosition)
        {
            return ChunkColumns[inChunkPosition.X * _chunkSize.Z + inChunkPosition.Z];
        }

        public override ChunkColumnInfo GetColumnInfo(Vector2I inChunkPosition)
        {
            return ChunkColumns[inChunkPosition.X * _chunkSize.Z + inChunkPosition.Y];
        }

        public override ChunkColumnInfo GetColumnInfo(byte inChunkPositionX, byte inChunkPositionZ)
        {
            return ChunkColumns[inChunkPositionX * _chunkSize.Z + inChunkPositionZ];
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
            return result;
        }

        public void SetTag(BlockTag tag, Vector3I inChunkPosition)
        {
            lock (_syncRoot)
            {
                if (tag != null)
                    _tags[inChunkPosition] = tag;
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

        // used to store chunk in the local storage
        public override void Save(BinaryWriter writer)
        {
            lock (_syncRoot)
            {
                //Save the Chunk Block informations ==================
                writer.Write(_chunkSize);
                writer.Write(GetBlocksBytes());

                //Save the Block tags metaData informations ==========
                writer.Write(_tags.Count);
                foreach (var pair in _tags)
                {
                    writer.Write(pair.Key); //Block Tag Position
                    writer.Write(pair.Value.Id); //Block Tag Cube ID
                    pair.Value.Save(writer); //Block tag object binary form
                }

                //Save the Chunk Column informations =================
                writer.Write(ChunkColumns.Length); //Save the qt of chunkColumn
                for (var i = 0; i < ChunkColumns.Length; i++)
                {
                    ChunkColumns[i].Save(writer); //Save the chunkColumn object data
                }

                //Save The chunk metaData
                ChunkMetaData.Save(writer);
            }
        }

        public override void Load(BinaryReader reader)
        {
            lock (_syncRoot)
            {
                //Load the Chunk Block informations ==================
                _chunkSize = reader.ReadVector3I();
                var bytesCount = _chunkSize.X*_chunkSize.Y*_chunkSize.Z;
                SetBlockBytes(reader.ReadBytes(bytesCount));

                //Load the Block tags metaData informations ==========
                var tagsCount = reader.ReadInt32();
                _tags.Clear();
                for (var i = 0; i < tagsCount; i++)
                {
                    var position = reader.ReadVector3I();
                    var tag = EntityFactory.CreateTagFromBytes(reader);
                    _tags.Add(position, tag);
                }

                //Load the Chunk Column informations =================
                var columnsInfoCount = reader.ReadInt32();
                for (var i = 0; i < columnsInfoCount; i++)
                {
                    ChunkColumns[i] = new ChunkColumnInfo();
                    ChunkColumns[i].Load(reader);
                }

                //Load The chunk metaData
                ChunkMetaData.Load(reader);
            }
        }

        public override object WriteSyncRoot
        {
            get { return _syncRoot; }
        }
    }
}