using System.IO;
using System.Security.Cryptography;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base chunk class
    /// </summary>
    public abstract class AbstractChunk
    {
        #region Static
        /// <summary>
        /// Gets byte amount needed to store chunk block data
        /// </summary>
        public static int ChunkBlocksByteLength { get; private set; }

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
                ChunkBlocksByteLength = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;
            }
        }
        #endregion
        
        #region Properties
        /// <summary>
        /// Contains only block information as a byte array. This array must be with length of ChunkBlocksByteLength. To change it change ChunkSize static property.
        /// </summary>
        public byte[] BlockBytes { get; set; }

        /// <summary>
        /// Gets entity collection of the chunk
        /// </summary>
        public EntityCollection Entities { get; private set; }


        protected byte[] Md5HashData;

        #endregion

        protected AbstractChunk()
        {
            Entities = new EntityCollection();
        }

        /// <summary>
        /// Gets block from specified internal position
        /// </summary>
        /// <param name="inBlockPosition"></param>
        /// <returns></returns>
        public virtual byte GetBlock(Location3<int> inBlockPosition)
        {
            return BlockBytes[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z];
        }

        /// <summary>
        /// Sets block to chunk internal position
        /// </summary>
        /// <param name="inBlockPosition"></param>
        /// <param name="value"></param>
        public virtual void SetBlock(Location3<int> inBlockPosition, byte value)
        {
            if (BlockBytes == null)
            {
                BlockBytes = new byte[ChunkBlocksByteLength];
            }
            BlockBytes[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z] = value;
            Md5HashData = null;
        }

        /// <summary>
        /// Gets or sets a block in the chunk
        /// </summary>
        /// <param name="position">Local position of the block</param>
        /// <returns></returns>
        public byte this[Location3<int> position]
        {
            get { return GetBlock(position); }
            set { SetBlock(position, value); }
        }

        /// <summary>
        /// Loads chunk data from bytes array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            Deserialize(new MemoryStream(bytes));
        }

        /// <summary>
        /// Loads chunk data from memory stream
        /// </summary>
        /// <param name="ms"></param>
        public void Deserialize(MemoryStream ms)
        {
            var reader = new BinaryReader(ms);
            BlockBytes = reader.ReadBytes(ChunkBlocksByteLength);
            Entities.Clear();
            Entities.LoadEntities(EntityFactory.Instance, ms, ChunkBlocksByteLength, (int)(ms.Length - ChunkBlocksByteLength));
        }

        /// <summary>
        /// Saves current chunk data to binary format
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(BlockBytes);
            Entities.SaveEntities(writer);
        }

        /// <summary>
        /// Saves current chunk data to binary format
        /// </summary>
        public byte[] Serialize()
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            Serialize(writer);
            return ms.ToArray();
        }

        /// <summary>
        /// Gets md5 hash of the chunk data (blocks and entities)
        /// </summary>
        /// <returns></returns>
        public byte[] GetMd5Hash()
        {
            return Md5HashData ?? (Md5HashData = CalculateHash(Serialize()));
        }
        
        protected byte[] CalculateHash(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            var provider = new MD5CryptoServiceProvider();
            
            return provider.ComputeHash(bytes);
        }
    }
}
