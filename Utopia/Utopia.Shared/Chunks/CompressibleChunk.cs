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
            
            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress))
            {
                var serializedBytes = Serialize();
                zip.Write(serializedBytes, 0, serializedBytes.Length);
            }

            var bytes = ms.ToArray();

            if (saveResult)
                CompressedBytes = bytes;
            
            return bytes;
        }

        /// <summary>
        /// Performs decompression and deserialization of the chunk using compressed bytes
        /// </summary>
        /// <param name="compressedBytes"></param>
        public void Decompress(byte[] compressedBytes)
        {
            if (compressedBytes == null) throw new ArgumentNullException("compressedBytes");
            CompressedBytes = compressedBytes;
            Decompress();
            CompressedBytes = null;
        }

        /// <summary>
        /// Tries to decompress and deserialize data from CompressedBytes property
        /// </summary>
        public void Decompress()
        {
            if (CompressedBytes == null)
                throw new InvalidOperationException("Set CompressedBytes property before decompression");

            var ms = new MemoryStream(CompressedBytes);
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                var decompressed = new MemoryStream();
                zip.CopyTo(decompressed);
                Deserialize(decompressed);
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
                CompressedDirty = false;
            }
            else
            {
                CompressedDirty = true;
            }
        }

    }
}
