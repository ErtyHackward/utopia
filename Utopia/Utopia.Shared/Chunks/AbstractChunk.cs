using System;
using System.IO;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base chunk class
    /// </summary>
    [ProtoContract]
    public abstract class AbstractChunk : IAbstractChunk
    {
        #region Static
        /// <summary>
        /// Gets byte amount needed to store chunk block data
        /// </summary>
        public static int ChunkBlocksByteLength = 16 * 128 * 16;
        public static Vector3I ChunkSize = new Vector3I(16, 128, 16);

        public static void SetChunkHeight(int height)
        {
            ChunkBlocksByteLength = 16 * 16 * height;
            ChunkSize = new Vector3I(16, height, 16);
        }

        #endregion
        
        #region Properties
        private ChunkDataProvider _blockDataProvider;

        protected Md5Hash Md5HashData;
        private EntityCollection _entities;

        /// <summary>
        /// Gets or sets current chunk position - In Chunk coordinate
        /// </summary>
        [ProtoMember(1)]
        public Vector3I Position { get; set; }

        /// <summary>
        /// Gets a chunk blocks data provider
        /// </summary>
        [ProtoMember(2)]
        public ChunkDataProvider BlockData
        {
            get { return _blockDataProvider; }
            set { 

                if (_blockDataProvider != null)
                {
                    _blockDataProvider.BlockBufferChanged -= BlockBufferChanged;
                    _blockDataProvider.BlockDataChanged -= BlockDataChanged;
                }

                _blockDataProvider = value;

                if (_blockDataProvider != null)
                {
                    _blockDataProvider.BlockBufferChanged += BlockBufferChanged;
                    _blockDataProvider.BlockDataChanged += BlockDataChanged;
                }
            }
        }

        /// <summary>
        /// Gets entity collection of the chunk
        /// </summary>
        [ProtoMember(3)]
        public EntityCollection Entities
        {
            get { return _entities; }
            set {
                if (_entities == value)
                    return;

                if (_entities != null)
                {
                    _entities.CollectionDirty -= EntitiesCollectionDirty;
                    _entities.Chunk = null;
                }

                _entities = value;

                if (_entities != null)
                {
                    _entities.Chunk = this;
                    _entities.CollectionDirty += EntitiesCollectionDirty;
                }
            }
        }

        /// <summary>
        /// Return the chunk position in Block unit
        /// </summary>
        public Vector3D BlockPosition
        {
            get
            {
                return new Vector3D(Position.X * ChunkSize.X, Position.Y * ChunkSize.Y, Position.Z * ChunkSize.Z);
            }
        }
        
        #endregion
        
        protected AbstractChunk(ChunkDataProvider blockDataProvider)
        {
            BlockData = blockDataProvider;
            Entities = new EntityCollection(this);
        }

        /// <summary>
        /// Gets md5 hash of the chunk data (blocks and entities)
        /// </summary>
        /// <returns></returns>
        public Md5Hash GetMd5Hash()
        {
            return Md5HashData ?? (Md5HashData = Md5Hash.Calculate(GetBytesForHash()));
        }

        private byte[] GetBytesForHash()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(BlockData.GetBlocksBytes());
                    Serializer.Serialize(ms, BlockData.GetTags());
                    Serializer.Serialize(ms, BlockData.ChunkMetaData);
                    Serializer.Serialize(ms, BlockData.ColumnsInfo);
                    Serializer.Serialize(ms, Entities);
                    return ms.ToArray();
                }
            }
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, BlockData, PrefixStyle.Fixed32);
                Serializer.SerializeWithLengthPrefix(ms, Entities, PrefixStyle.Fixed32);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Consumes all data from the chunk provided, om nom nom nom
        /// Don't use the consumed chunk anymore
        /// </summary>
        /// <param name="chunk"></param>
        public virtual void Consume(AbstractChunk chunk)
        {
            BlockData.Consume(chunk.BlockData);
            Entities.Import(chunk.Entities);
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

        /// <summary>
        /// Handling of entities collection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void EntitiesCollectionDirty(object sender, EventArgs e)
        {
            Md5HashData = null;
        }
    }
}
