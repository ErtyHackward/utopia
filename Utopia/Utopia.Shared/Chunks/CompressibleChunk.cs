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

        /// <summary>
        /// Sets block to chunk internal position
        /// </summary>
        /// <param name="inBlockPosition"></param>
        /// <param name="value"></param>
        public override void SetBlock(Structs.Location3<int> inBlockPosition, byte value)
        {
            base.SetBlock(inBlockPosition, value);

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

        /// <summary>
        /// Performs serialization and compression of resulted bytes
        /// </summary>
        public void Compress()
        {
            if (BlockBytes != null)
            {
                var ms = new MemoryStream();
                using (var zip = new GZipStream(ms, CompressionMode.Compress))
                {
                    var serializedBytes = Serialize();
                    zip.Write(serializedBytes, 0, serializedBytes.Length);
                }
                CompressedBytes = ms.ToArray();
            }
            else CompressedBytes = null;
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
            if (CompressedBytes != null)
            {
                if (BlockBytes == null) BlockBytes = new byte[ChunkBlocksByteLength];
                var ms = new MemoryStream(CompressedBytes);
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    var decompressed = new MemoryStream();
                    zip.CopyTo(decompressed);
                    Deserialize(decompressed);
                }
            }
            else 
            { 
                BlockBytes = null; 
                Entities.Clear(); 
            }
        }

    }
}
