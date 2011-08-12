namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a world generator chunk 
    /// </summary>
    public class GeneratedChunk
    {
        /// <summary>
        /// Gets byte amount needed to store chunk block data
        /// </summary>
        public static int ChunkByteLength { get; private set; }

        private static Location3<int> _chunkSize;
        /// <summary>
        /// Gets or sets chunk size
        /// </summary>
        public static Location3<int> ChunkSize
        {
            get { return _chunkSize; }
            set
            {
                _chunkSize = value;
                ChunkByteLength = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;
            }
        }

        public byte[] BlockData { get; set; }

        public byte GetBlock(Location3<int> inBlockPosition)
        {
            return BlockData[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z];
        }

        public void SetBlock(Location3<int> inBlockPosition, byte value)
        {
            if (BlockData == null)
            {
                BlockData = new byte[ChunkByteLength];
            }
            BlockData[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z] = value;
        }
    }
}
