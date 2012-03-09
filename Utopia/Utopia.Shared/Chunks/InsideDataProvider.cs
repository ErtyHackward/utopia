using System;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk data provider that holds block data inside the chunk. Allows to change chunk size
    /// </summary>
    public class InsideDataProvider : ChunkDataProvider
    {
        private Vector3I _chunkSize;

        private byte[] _blockBytes;

        /// <summary>
        /// Gets or sets the inside buffer
        /// </summary>
        public byte[] BlockBytes
        {
            get { return _blockBytes; }
            set { _blockBytes = value; }
        }

        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBlocksBytes()
        {
            return _blockBytes;
        }
        
        public Vector3I ChunkSize
        {
            get { return _chunkSize; }
        }

        public InsideDataProvider()
        {
            _chunkSize = AbstractChunk.ChunkSize;
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
            
            // copy data
            if (_blockBytes != null && copyData)
            {
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
        public override void SetBlockBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var arrayLength = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;

            if (bytes.Length != arrayLength)
                throw new ArgumentOutOfRangeException(string.Format("Wrong block buffer size. Expected: {0}, Actual: {1}", arrayLength, bytes.Length));

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
            if (_blockBytes == null) return 0;
            return _blockBytes[inChunkPosition.X * _chunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * _chunkSize.Y * _chunkSize.X];
        }

        public byte GetBlock(int x, int y, int z)
        {
            if (_blockBytes == null) return 0;
            return _blockBytes[x * _chunkSize.Y + y + z * _chunkSize.Y * _chunkSize.X];
        }

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        public override void SetBlock(Vector3I inChunkPosition, byte blockValue)
        {
            if (_blockBytes == null)
            {
                _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
            }
            _blockBytes[inChunkPosition.X * _chunkSize.Y + inChunkPosition.Y + inChunkPosition.Z * _chunkSize.Y * _chunkSize.X] = blockValue;

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { inChunkPosition }, Bytes = new[] { blockValue } });
        }

        /// <summary>
        /// Sets a group of blocks
        /// </summary>
        /// <param name="positions">internal chunk positions</param>
        /// <param name="values"></param>
        public override void SetBlocks(Vector3I[] positions, byte[] values)
        {
            if (_blockBytes == null)
            {
                _blockBytes = new byte[_chunkSize.X * _chunkSize.Y * _chunkSize.Z];
            }

            for (var i = 0; i < positions.Length; i++)
            {
                _blockBytes[positions[i].X * _chunkSize.Y + positions[i].Y + positions[i].Z * _chunkSize.Y * _chunkSize.X] = values[i];    
            }

            OnBlockDataChanged(new ChunkDataProviderDataChangedEventArgs { Count = positions.Length, Locations = positions, Bytes = values });
        }
    }
}