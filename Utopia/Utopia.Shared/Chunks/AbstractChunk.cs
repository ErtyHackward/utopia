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
        private static int _chunkBlocksByteLength = 2 * 4 * 2;
        public static int ChunkBlocksByteLength
        {
            get { return _chunkBlocksByteLength; }
        }

        private static Location3<int> _chunkSize = new Location3<int>(2, 4, 2);
        public static Location3<int> ChunkSize
        {
            get { return _chunkSize; }
        }

        #endregion
        
        #region Properties
        private ChunkDataProvider _blockDataProvider;

        protected byte[] Md5HashData;

        /// <summary>
        /// Gets or sets a chunk blocks data provider
        /// </summary>
        public ChunkDataProvider BlockData
        {
            get { return _blockDataProvider; }
        }

        /// <summary>
        /// Provides ability to change the chunk block data provider
        /// </summary>
        /// <param name="newProvider">New provider to replace</param>
        /// <param name="sameData">Indicates if new provider has the same block data as previous</param>
        public virtual void ChangeBlockDataProvider(ChunkDataProvider newProvider, bool sameData)
        {
            if (_blockDataProvider != newProvider)
            {
                if (_blockDataProvider != null)
                {
                    _blockDataProvider.BlockBufferChanged -= BlockBufferChanged;
                    _blockDataProvider.BlockDataChanged -= BlockDataChanged;
                }

                _blockDataProvider = newProvider;

                if (_blockDataProvider != null)
                {
                    _blockDataProvider.BlockBufferChanged += BlockBufferChanged;
                    _blockDataProvider.BlockDataChanged += BlockDataChanged;
                }

                if(!sameData)
                    Md5HashData = null;
            }
        }

        /// <summary>
        /// Gets entity collection of the chunk
        /// </summary>
        public EntityCollection Entities { get; private set; }
        
        #endregion
        
        protected AbstractChunk(ChunkDataProvider blockDataProvider)
        {
            _blockDataProvider = blockDataProvider;
            _blockDataProvider.BlockBufferChanged += BlockBufferChanged;
            _blockDataProvider.BlockDataChanged += BlockDataChanged;

            Entities = new EntityCollection();
        }

        /// <summary>
        /// Loads chunk data from bytes array (blocks and entites)
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            Deserialize(new MemoryStream(bytes));
        }

        /// <summary>
        /// Loads chunk data from memory stream (blocks and entites)
        /// </summary>
        /// <param name="ms"></param>
        public void Deserialize(MemoryStream ms)
        {
            var reader = new BinaryReader(ms);
            BlockData.SetBlockBytes(reader.ReadBytes(ChunkBlocksByteLength));
            Entities.Clear();
            Entities.LoadEntities(EntityFactory.Instance, ms, ChunkBlocksByteLength, (int)(ms.Length - ChunkBlocksByteLength));
        }

        /// <summary>
        /// Saves current chunk data to binary format (blocks and entites)
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(BlockData.GetBlocksBytes());
            Entities.SaveEntities(writer);
        }

        /// <summary>
        /// Saves current chunk data to binary format (blocks and entites)
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

        /// <summary>
        /// Handling of chunk blocks data change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            Md5HashData = null;
        }

        /// <summary>
        /// Handling of whole chunk blocks buffer change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void BlockBufferChanged(object sender, ChunkDataProviderBufferChangedEventArgs e)
        {
            Md5HashData = null;
        }
    }
}
