using System;
using System.Collections.Generic;
using System.Threading;
using ProtoBuf;
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
        private readonly object _syncRoot = new object();
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
        
        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(1)]
        public override Vector3I ChunkSize
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
            get {
                lock (_syncRoot)
                {
                    return new Dictionary<Vector3I, BlockTag>(_tags);
                }
            }
            set {
                lock (_syncRoot)
                {
                    _tags = value;     
                }
            }
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
            get { return _syncRoot; }
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
            // if there are currently other transaction running, wait unit it will finish
            while (true)
            {
                lock (_syncRoot)
                {
                    if (!_transaction)
                    {
                        _transaction = true;
                        break;
                    }
                }

                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Fire all accumulated changes in one event
        /// </summary>
        public void CommitTransaction()
        {
            if (!_transaction)
                throw new InvalidOperationException("Transaction was not started");
            lock (_syncRoot)
            {
                _transaction = false;    
            }
            
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

            lock (_syncRoot)
            {
                _chunkColumns = new ChunkColumnInfo[newSize.X * newSize.Z];

                // copy data
                if (_blockBytes != null && copyData)
                {
                    var newArray = new byte[newSize.X * newSize.Y * newSize.Z];

                    Vector3I copySize;

                    copySize.x = newSize.X > _chunkSize.X ? _chunkSize.X : newSize.X;
                    copySize.y = newSize.Y > _chunkSize.Y ? _chunkSize.Y : newSize.Y;
                    copySize.z = newSize.Z > _chunkSize.Z ? _chunkSize.Z : newSize.Z;

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

            lock (_syncRoot)
            {
                BlockBytes = bytes;

                _tags.Clear();

                if (tags != null)
                {
                    foreach (var keyValuePair in tags)
                    {
                        _tags.Add(keyValuePair.Key, keyValuePair.Value);
                    }
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
            if (_blockBytes == null) 
                return 0;
            lock (_syncRoot)
                return _blockBytes[((inChunkPosition.Z * _chunkSize.X) + inChunkPosition.X) * _chunkSize.Y + inChunkPosition.Y];
        }

        public byte GetBlock(int x, int y, int z)
        {
            if (_blockBytes == null) 
                return 0;
            lock (_syncRoot)
                return _blockBytes[((z * _chunkSize.X) + x) * _chunkSize.Y + y];
        }

        /// <summary>
        /// Gets an optional block tag
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public override BlockTag GetTag(Vector3I inChunkPosition)
        {
            lock (_syncRoot)
            {
                BlockTag result;
                _tags.TryGetValue(inChunkPosition, out result);
                return result == null ? null : (BlockTag)result.Clone();
            }
        }

        private void SetTag(BlockTag tag, Vector3I inChunkPosition)
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
                foreach (var blockTag in _tags)
                {
                    yield return blockTag;
                }
            }
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
                        vec.x = x;
                        vec.y = y;
                        vec.z = z;

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
        /// <param name="sourceDynamicId">Id of the entity that is responsible for the change</param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null, uint sourceDynamicId = 0)
        {
            lock (_syncRoot)
            {
                if (_blockBytes == null)
                {
                    _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
                }

                _blockBytes[inChunkPosition.X * _chunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * _chunkSize.Y * _chunkSize.X] = blockValue;
                RefreshMetaData(ref inChunkPosition, blockValue);
                SetTag(tag, inChunkPosition);
            }

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
                                           Tags = tag != null ? new[] {tag} : null,
                                           SourceDynamicId = sourceDynamicId
                                       });
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
            lock (_syncRoot)
            {
                if (_blockBytes == null)
                {
                    _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
                }

                for (var i = 0; i < positions.Length; i++)
                {
                    _blockBytes[positions[i].X * _chunkSize.Y + positions[i].Y + positions[i].Z * _chunkSize.Y * _chunkSize.X] = values[i];
                    RefreshMetaData(ref positions[i], values[i]);
                }

                if (tags != null)
                {
                    for (var i = 0; i < positions.Length; i++)
                    {
                        SetTag(tags[i], positions[i]);
                    }
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
                                           Tags = tags,
                                           SourceDynamicId = sourceDynamicId
                                       });
            }
        }

        #region Chunk Column Information Manager

        public override ChunkColumnInfo GetColumnInfo(int inChunkPositionX, int inChunkPositionZ)
        {
            return _chunkColumns[inChunkPositionZ * _chunkSize.X + inChunkPositionX];
        }

        private void RefreshMetaData(ref Vector3I inChunkPosition, byte newBlockValue)
        {
            int indexColumn = inChunkPosition.X * _chunkSize.Z + inChunkPosition.Z;
            if (newBlockValue != WorldConfiguration.CubeId.Air)
            {
                //Change being made above surface !
                if (ColumnsInfo[indexColumn].MaxHeight < inChunkPosition.Y)
                {
                    ColumnsInfo[indexColumn].MaxHeight = (byte)inChunkPosition.Y;
                    ChunkMetaData.setChunkMaxHeightBuilt((byte)inChunkPosition.Y);
                    if (ColumnsInfo[indexColumn].IsWild)
                    {
                        ColumnsInfo[indexColumn].IsWild = false;
                        ChunkMetaData.setChunkWildStatus(ColumnsInfo);
                    }
                }
            }
            else
            {
                //Change being made at the surface (Block removed)
                if (ColumnsInfo[indexColumn].MaxHeight <= inChunkPosition.Y)
                {
                    int yPosi = inChunkPosition.Y - 1;
                    while (yPosi > 0 && GetBlock(inChunkPosition.X, yPosi, inChunkPosition.Z) == WorldConfiguration.CubeId.Air)
                    {
                        yPosi--;
                    }
                    ChunkMetaData.setChunkMaxHeightBuilt((byte)yPosi);
                    if (ColumnsInfo[indexColumn].IsWild)
                    {
                        ColumnsInfo[indexColumn].IsWild = false;
                        ChunkMetaData.setChunkWildStatus(ColumnsInfo);
                    }
                }
            }
        }
        #endregion

        
    }
}