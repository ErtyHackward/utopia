using System;
using System.IO;
using System.IO.Compression;

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

            return bytes;
        }

        /// <summary>
        /// Performs decompression and deserialization of the chunk using compressed bytes
        /// </summary>
        /// <param name="compressedBytes"></param>
        /// <param name="getHash">Do we need to take md5hash of the chunk?</param>
        public void Decompress(byte[] compressedBytes, bool getHash = false)
        {
            if (compressedBytes == null) throw new ArgumentNullException("compressedBytes");
            CompressedBytes = compressedBytes;
            Decompress(getHash);
            CompressedBytes = null;
        }

        /// <summary>
        /// Tries to decompress and deserialize data from CompressedBytes property
        /// </summary>
        /// <param name="getHash">Do we need to take md5hash of the chunk?</param>
        public void Decompress(bool getHash = false)
        {
            if (CompressedBytes == null)
                throw new InvalidOperationException("Set CompressedBytes property before decompression");

            var ms = new MemoryStream(CompressedBytes);
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                var decompressed = new MemoryStream();
                zip.CopyTo(decompressed);
                if (getHash)
                {
                    Md5HashData = CalculateHash(decompressed);
                    decompressed.Position = 0;
                }
                Deserialize(decompressed);
                CompressedDirty = false;
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

        public override void ChangeBlockDataProvider(ChunkDataProvider newProvider, bool sameData)
        {
            base.ChangeBlockDataProvider(newProvider, sameData);

            if (!sameData)
            {
                OnBlockDataChanged();
            }
        }

        private void OnBlockDataChanged()
        {
            if (InstantCompress)
            {
                Compress();
            }
            else
            {
                CompressedDirty = true;
            }
        }

    }
}
