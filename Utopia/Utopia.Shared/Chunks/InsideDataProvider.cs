using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Utopia.Shared.Entities;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds block data inside the chunk. Allows to change chunk size
    /// </summary>
    [ProtoContract]
    public class InsideDataProvider : ChunkDataProvider
    {
        private readonly object _writeSyncRoot = new object();
        private Vector3I _chunkSize;
        private byte[] _blockBytes;
        private ChunkColumnInfo[] _chunkColumns;
        private ChunkMetaData _chunkMetaData;

        // transaction allows to delay events by accumulating the changes
        private bool _transaction;
        private readonly List<Vector3I> _transactionPositions = new List<Vector3I>();
        private readonly List<byte> _transactionValues = new List<byte>();
        private readonly List<BlockTag> _transactionTags = new List<BlockTag>();

        private Dictionary<Vector3I, BlockTag> _tags = new Dictionary<Vector3I, BlockTag>();

        public Vector3I ChunkSize
        {
            get { return _chunkSize; }
        }

        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(1)]
        public Vector3I SerializeChunkSize
        {
            get { return _chunkSize; }
            set
            {
                _chunkSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the inside buffer
        /// </summary>
        [ProtoMember(2, OverwriteList = true)]
        public byte[] BlockBytes
        {
            get { return _blockBytes; }
            set { 
                _blockBytes = value;
            }
        }

        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<Vector3I, BlockTag> SerializeTags
        {
            get { return _tags; }
            set { _tags = value; }
        }

        /// <summary>
        /// Gets or sets the chunk MetaData
        /// </summary>
        [ProtoMember(4)]
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

        [ProtoMember(5, OverwriteList = true)]
        public override ChunkColumnInfo[] ColumnsInfo
        {
            get
            {
                return _chunkColumns;
            }
            set
            {
                _chunkColumns = value;
            }
        }

        public override object WriteSyncRoot
        {
            get { return _writeSyncRoot; }
        }

        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            return _blockBytes;
        }

        public InsideDataProvider()
        {
            _chunkSize = AbstractChunk.ChunkSize;
            _chunkColumns = new ChunkColumnInfo[_chunkSize.X * _chunkSize.Z];
            _chunkMetaData = new ChunkMetaData();
        }

        /// <summary>
        /// Creates a copy of the data
        /// </summary>
        /// <param name="insideDataProvider"></param>
        public InsideDataProvider(InsideDataProvider insideDataProvider)
        {
            _chunkSize = insideDataProvider._chunkSize;
            _chunkColumns = (ChunkColumnInfo[])insideDataProvider._chunkColumns.Clone();
            _chunkMetaData = new ChunkMetaData(insideDataProvider.ChunkMetaData);
            _tags = new Dictionary<Vector3I, BlockTag>(insideDataProvider._tags);
            if (insideDataProvider._blockBytes != null)
                _blockBytes = (byte[])insideDataProvider._blockBytes.Clone();
        }

        /// <summary>
        /// Starts accumulating changes
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction)
                throw new InvalidOperationException("Transaction is already started");
            _transaction = true;
        }

        /// <summary>
        /// Fire all accumulated changes in one event
        /// </summary>
        public void CommitTransaction()
        {
            if (!_transaction)
                throw new InvalidOperationException("Transaction was not started");
            _transaction = false;

            if (_transactionPositions.Count == 0) 
                return;

            lock (_transactionPositions)
            {

                OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs
                                       {
                                           Count = _transactionPositions.Count,
                                           Locations = _transactionPositions.ToArray(),
                                           Bytes = _transactionValues.ToArray(),
                                           Tags = _transactionTags.ToArray()
                                       });
            }

            _transactionPositions.Clear();
            _transactionValues.Clear();
            _transactionTags.Clear();

        }

        /// <summary>
        /// Changes current chunk size. Can recreate internal array and copy all previous data to the new array
        /// </summary>
        /// <param name="newSize"></param>
        /// <param name="copyData"></param>
        public void UpdateChunkSize(Vector3I newSize, bool copyData = false)
        {
            // no need to do anything?
            if (_chunkSize == newSize) return;

            _chunkColumns = new ChunkColumnInfo[newSize.X * newSize.Z];

            // copy data
            if (_blockBytes != null && copyData)
            {
                lock (_writeSyncRoot) { }
                
                var newArray = new byte[newSize.X * newSize.Y * newSize.Z];

                Vector3I copySize;

                copySize.X = newSize.X > _chunkSize.X ? _chunkSize.X : newSize.X;
                copySize.Y = newSize.Y > _chunkSize.Y ? _chunkSize.Y : newSize.Y;
                copySize.Z = newSize.Z > _chunkSize.Z ? _chunkSize.Z : newSize.Z;

                for (int x = 0; x < copySize.X; x++)
                {
                    for (int y = 0; y < copySize.Y; y++)
                    {
                        for (int z = 0; z < copySize.Z; z++)
                        {
                            newArray[x * newSize.Y + y + z * newSize.Y * newSize.X] = _blockBytes[x * _chunkSize.Y + y + z * _chunkSize.Y * _chunkSize.X];
                        }
                    }
                }

                _blockBytes = newArray;
            }

            _chunkSize = newSize;
        }

        /// <summary>
        /// Sets a full block buffer for a chunk
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tags"> </param>
        public override void SetBlockBytes(byte[] bytes, IEnumerable<KeyValuePair<Vector3I,BlockTag>> tags = null)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var arrayLength = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;

            if (bytes.Length != arrayLength)
                throw new ArgumentOutOfRangeException(string.Format("Wrong block buffer size. Expected: {0}, Actual: {1}", arrayLength, bytes.Length));

            lock (_writeSyncRoot) { }

            BlockBytes = bytes;

            _tags.Clear();

            if (tags != null)
            {
                foreach (var keyValuePair in tags)
                {
                    _tags.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            OnBlockBufferChanged(new ChunkDataProviderBufferChangedEventArgs { NewBuffer = bytes });
        }

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override byte GetBlock(Vector3I inChunkPosition)
        {
            if (_blockBytes == null) return 0;
            //return _blockBytes[inChunkPosition.X * _chunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * _chunkSize.Y * _chunkSize.X];
            return _blockBytes[((inChunkPosition.Z * _chunkSize.X) + inChunkPosition.X) * _chunkSize.Y + inChunkPosition.Y];
        }

        public byte GetBlock(int x, int y, int z)
        {
            if (_blockBytes == null) return 0;
            //return _blockBytes[x * _chunkSize.Y + y + z * _chunkSize.Y * _chunkSize.X];
            return _blockBytes[((z * _chunkSize.X) + x) * _chunkSize.Y + y];
        }

        /// <summary>
        /// Gets an optional block tag
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override BlockTag GetTag(Vector3I inChunkPosition)
        {
            BlockTag result;
            _tags.TryGetValue(inChunkPosition, out result);
            return result;
        }

        private void SetTag(BlockTag tag, Vector3I inChunkPosition)
        {
            if (tag != null)
                _tags[inChunkPosition] = tag;
            else
                _tags.Remove(inChunkPosition);
        }

        public override IEnumerable<KeyValuePair<Vector3I, BlockTag>> GetTags()
        {
            return _tags;
        }

        public IEnumerable<KeyValuePair<Vector3I, byte>> AllBlocks(bool includeEmpty = false)
        {
            for (int x = 0; x < _chunkSize.X; x++)
            {
                for (int y = 0; y < _chunkSize.Y; y++)
                {
                    for (int z = 0; z < _chunkSize.Z; z++)
                    {
                        Vector3I vec;
                        vec.X = x;
                        vec.Y = y;
                        vec.Z = z;

                        var value = GetBlock(vec);

                        if (value == 0 && !includeEmpty)
                            continue;
                        
                        yield return new KeyValuePair<Vector3I, byte>(vec, value);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        /// <param name="tag"></param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null)
        {
            lock (_writeSyncRoot) { }

            if (_blockBytes == null)
            {
                _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
            }
            _blockBytes[inChunkPosition.X * _chunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * _chunkSize.Y * _chunkSize.X] = blockValue;
            RefreshMetaData(ref inChunkPosition);
            SetTag(tag, inChunkPosition);

            if (_transaction)
            {
                lock (_transactionPositions)
                {
                    _transactionPositions.Add(inChunkPosition);
                    _transactionValues.Add(blockValue);
                    _transactionTags.Add(tag);
                }
            }
            else
            {
                // notify everyone about block change
                OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs
                                       {
                                           Count = 1,
                                           Locations = new[] {inChunkPosition},
                                           Bytes = new[] {blockValue},
                                           Tags = tag != null ? new[] {tag} : null
                                       });
            }
        }

        /// <summary>
        /// Sets a group of blocks
        /// </summary>
        /// <param name="positions">internal chunk positions</param>
        /// <param name="values"></param>
        /// <param name="tags"> </param>
        public override void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null)
        {
            lock (_writeSyncRoot) { }

            if (_blockBytes == null)
            {
                _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
            }

            for (var i = 0; i < positions.Length; i++)
            {
                _blockBytes[positions[i].X * _chunkSize.Y + positions[i].Y + positions[i].Z * _chunkSize.Y * _chunkSize.X] = values[i];
                RefreshMetaData(ref positions[i]);
            }

            if (tags != null)
            {
                for (var i = 0; i < positions.Length; i++)
                {
                    SetTag(tags[i], positions[i]);
                }
            }
            
            if (_transaction)
            {
                lock (_transactionPositions)
                {
                    _transactionPositions.AddRange(positions);
                    _transactionValues.AddRange(values);
                    if (tags == null)
                        tags = new BlockTag[positions.Length];
                    _transactionTags.AddRange(tags);
                }
            }
            else
            {
                // notify everyone about block change
                OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs
                                       {
                                           Count = positions.Length,
                                           Locations = positions,
                                           Bytes = values,
                                           Tags = tags
                                       });
            }
        }

        #region Chunk Column Information Manager

        public override ChunkColumnInfo GetColumnInfo(Vector3I inChunkPosition)
        {
            return _chunkColumns[inChunkPosition.Z * _chunkSize.X + inChunkPosition.X];
        }

        public override ChunkColumnInfo GetColumnInfo(Vector2I inChunkPosition)
        {
            return _chunkColumns[inChunkPosition.Y * _chunkSize.X + inChunkPosition.X];
        }

        public override ChunkColumnInfo GetColumnInfo(byte inChunkPositionX, byte inChunkPositionZ)
        {
            return _chunkColumns[inChunkPositionZ * _chunkSize.X + inChunkPositionX];
        }

        public override ChunkColumnInfo GetColumnInfo(int inChunkPositionX, int inChunkPositionZ)
        {
            return _chunkColumns[inChunkPositionZ * _chunkSize.X + inChunkPositionX];
        }

        private void RefreshMetaData(ref Vector3I inChunkPosition)
        {
            //Must look from World Top to bottom to recompute the new High Block !
            int yPosi = _chunkSize.Y - 1;
            while (GetBlock(inChunkPosition.X, yPosi, inChunkPosition.Z) == WorldConfiguration.CubeId.Air && yPosi > 0)
            {
                yPosi--;
            }

            //Compute 2D index of ColumnInfo and update ColumnInfo
            int index2D = inChunkPosition.X * _chunkSize.Z + inChunkPosition.Z;
            ColumnsInfo[index2D].MaxHeight = (byte)yPosi;
            ChunkMetaData.setChunkMaxHeightBuilt(ColumnsInfo);
        }
        #endregion

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            //Load the Chunk Block informations ==================
            _chunkSize = reader.ReadVector3I(); 
            var bytesCount = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;
            _blockBytes = reader.ReadBytes(bytesCount);

            if (_blockBytes.Length != bytesCount)
                throw new EndOfStreamException();

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
                _chunkColumns[i] = new ChunkColumnInfo();
                _chunkColumns[i].Load(reader);
            }

            //Load The chunk metaData
            ChunkMetaData.Load(reader);
        }
    }
}