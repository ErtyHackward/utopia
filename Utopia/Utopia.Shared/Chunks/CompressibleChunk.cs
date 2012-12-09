using System;
using System.IO;
using System.IO.Compression;
using ProtoBuf;
using ProtoBuf.Meta;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Allows to compress serialized chunk state
    /// </summary>
    public class CompressibleChunk : AbstractChunk
    {
        /// <summary>
        /// Gets or sets compressed chunk state
        /// </summary>
        public byte[] CompressedBytes { get; set; }

        /// <summary>
        /// Gets or sets value that indicates if chunk should compress data for every block modification (this function used in server)
        /// </summary>
        public bool InstantCompress { get; set; }

        /// <summary>
        /// Gets or sets value that indicates if chunk was modified and need to be compressed
        /// </summary>
        public bool CompressedDirty { get; set; }

        public CompressibleChunk(ChunkDataProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// Performs serialization and compression of resulted bytes
        /// </summary>
        /// <param name="saveResult">Whether or not to save resulted bytes into CompressedBytes property</param>
        /// <returns>Compressed bytes array</returns>
        public byte[] Compress(bool saveResult = true)
        {
            if (BlockData == null)
                throw new ArgumentNullException();

            if (!CompressedDirty && CompressedBytes != null)
                return CompressedBytes;
            
            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress))
            {
                var serializedBytes = Serialize();
                zip.Write(serializedBytes, 0, serializedBytes.Length);
            }

            var bytes = ms.ToArray();

            if (saveResult)
            {
                CompressedBytes = bytes;
                CompressedDirty = false;
            }
            ms.Dispose();
            return bytes;
        }

        /// <summary>
        /// Performs serialization, take hash, and compression of resulted bytes
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Compressed bytes array</returns>
        public byte[] CompressAndComputeHash(out Md5Hash hash)
        {
            if (BlockData == null)
                throw new ArgumentNullException();

            if (!CompressedDirty && Md5HashData != null)
            {
                hash = Md5HashData;
                return CompressedBytes;
            }

            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress))
            {
                var serializedBytes = Serialize();
                Md5HashData = hash = Md5Hash.Calculate(serializedBytes);
                zip.Write(serializedBytes, 0, serializedBytes.Length);
            }

            var bytes = ms.ToArray();

            ms.Dispose();
            return bytes;
        }

        /// <summary>
        /// Performs decompression and deserialization of the chunk using compressed bytes
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="compressedBytes"></param>
        /// <param name="getHash">Do we need to take md5hash of the chunk?</param>
        public void Decompress(EntityFactory factory, byte[] compressedBytes, bool getHash = false)
        {
            if (compressedBytes == null) throw new ArgumentNullException("compressedBytes");
            CompressedBytes = compressedBytes;
            Decompress(factory, getHash);
            CompressedBytes = null;
        }

        /// <summary>
        /// Tries to decompress and deserialize data from CompressedBytes property
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="getHash">Do we need to take md5hash of the chunk?</param>
        public void Decompress(EntityFactory factory, bool getHash = false)
        {
            if (CompressedBytes == null)
                throw new InvalidOperationException("Set CompressedBytes property before decompression");

            using (var ms = new MemoryStream(CompressedBytes))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    var decompressed = new MemoryStream();
                    zip.CopyTo(decompressed);
                    decompressed.Position = 0;
                    if (getHash)
                    {
                        Md5HashData = Md5Hash.Calculate(decompressed);
                        decompressed.Position = 0;
                    }

                    BlockData = Serializer.DeserializeWithLengthPrefix<ChunkDataProvider>(decompressed, PrefixStyle.Fixed32);
                    Entities = Serializer.DeserializeWithLengthPrefix<EntityCollection>(decompressed, PrefixStyle.Fixed32);

                    OnDecompressed();
                    
                    decompressed.Dispose();
                    CompressedDirty = false;
                }
            }
        }

        protected virtual void OnDecompressed()
        {

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
            if (InstantCompress)
            {
                CompressedDirty = true;
                Compress();
            }
            else
            {
                CompressedDirty = true;
            }
        }

    }
}
