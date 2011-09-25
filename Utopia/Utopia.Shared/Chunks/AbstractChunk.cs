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
        public readonly static int ChunkBlocksByteLength = 16 * 128 * 16;
        public readonly static Vector3I ChunkSize = new Vector3I(16, 128, 16);

        #endregion
        
        #region Properties
        private ChunkDataProvider _blockDataProvider;

        protected Md5Hash Md5HashData;

        /// <summary>
        /// Gets a chunk blocks data provider
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
            ms.Position = 0;
            var reader = new BinaryReader(ms);
            BlockData.SetBlockBytes(reader.ReadBytes(ChunkBlocksByteLength));
            Entities.Clear();
            Entities.LoadEntities(EntityFactory.Instance, ms, ChunkBlocksByteLength, (int)(ms.Length - ChunkBlocksByteLength));
            ms.Dispose();
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
        public Md5Hash GetMd5Hash()
        {
            return Md5HashData ?? (Md5HashData = CalculateHash(Serialize()));
        }

        protected Md5Hash CalculateHash(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            var provider = new MD5CryptoServiceProvider();
            
            return new Md5Hash(provider.ComputeHash(bytes));
        }

        protected Md5Hash CalculateHash(MemoryStream ms)
        {
            if (ms == null || ms.Length == 0)
            {
                return null;
            }
            var provider = new MD5CryptoServiceProvider();

            return new Md5Hash(provider.ComputeHash(ms));
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
