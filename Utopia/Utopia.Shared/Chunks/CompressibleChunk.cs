using System;
using System.IO;
using System.IO.Compression;
using ProtoBuf;
using ProtoBuf.Meta;
using Utopia.Shared.Entities;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Allows to compress serialized chunk state
    /// The main reason of the class is to provide cache of compressed chunk and not re-compress it on each send
    /// </summary>
    public class CompressibleChunk : AbstractChunk
    {
        /// <summary>
        /// Gets or sets compressed chunk state
        /// </summary>
        public byte[] CompressedBytes { get; set; }

        /// <summary>
        /// Gets or sets value that indicates if chunk was modified and need to be compressed
        /// </summary>
        public bool CompressedDirty
        {
            get { return CompressedBytes == null; }
            set
            {
                if (value == CompressedDirty)
                    return;

                if (value)
                {
                    CompressedBytes = null;
                }
                else
                {
                    Compress();
                }
            }
        }

        public CompressibleChunk(ChunkDataProvider provider) 
            : base(provider)
        {

        }

        /// <summary>
        /// Performs serialization and compression of resulted bytes
        /// </summary>
        /// <param name="cacheResult">Whether or not to save resulted bytes into CompressedBytes property of the class</param>
        /// <returns>Compressed bytes array</returns>
        public byte[] Compress(bool cacheResult = true)
        {
            if (BlockData == null)
                throw new ArgumentNullException();

            if (!CompressedDirty)
                return CompressedBytes;
            
            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress))
            {
                var serializedBytes = Serialize();
                zip.Write(serializedBytes, 0, serializedBytes.Length);
            }

            var bytes = ms.ToArray();

            if (cacheResult)
            {
                CompressedBytes = bytes;
            }
            ms.Dispose();
            return bytes;
        }
        
        /// <summary>
        /// Tries to decompress and deserialize data from CompressedBytes property
        /// </summary>
        public void Decompress(byte[] compressedBytes)
        {
            if (compressedBytes == null) 
                throw new ArgumentNullException("compressedBytes");

            CompressedBytes = compressedBytes;

            using (var ms = new MemoryStream(CompressedBytes))
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            using (var decompressed = new MemoryStream())
            {
                zip.CopyTo(decompressed);
                decompressed.Position = 0;
                BlockData = (ChunkDataProvider)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(decompressed, BlockData, typeof(ChunkDataProvider), PrefixStyle.Fixed32, 0);
                if (Entities.Count > 0) Entities.Clear();
                Entities = (EntityCollection)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(decompressed, Entities, typeof(EntityCollection), PrefixStyle.Fixed32, 0);
            }
        }

        protected override void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            OnBlockDataChanged();

            base.BlockDataChanged(sender, e);
        }

        protected override void BlockBufferChanged(object sender, ChunkDataProviderBufferChangedEventArgs e)
        {
            OnBlockDataChanged();

            base.BlockBufferChanged(sender, e);
        }

        protected override void EntitiesCollectionDirty(object sender, EventArgs e)
        {
            base.EntitiesCollectionDirty(sender, e);

            OnBlockDataChanged();
        }

        private void OnBlockDataChanged()
        {
            CompressedBytes = null;
        }
    }
}
