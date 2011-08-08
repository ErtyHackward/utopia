using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Defines a class to represent Chunk on the server
    /// </summary>
    public class Chunk
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
            set { 
                _chunkSize = value;
                ChunkByteLength = _chunkSize.X * _chunkSize.Y * _chunkSize.Z;
            }
        }

        private byte[] _data;
        private byte[] _dataCompressed;
        private byte[] _md5DataHash;

        /// <summary>
        /// Gets or sets actual chunk data (automatically compresses it)
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
            set { 
                _data = value;
                Compress();
                CalculateHash();
            }
        }
        
        /// <summary>
        /// Gets or sets compressed Data array using GZip (automatically decompresses it)
        /// </summary>
        public byte[] DataCompressed
        {
            get { return _dataCompressed; }
            set { 
                _dataCompressed = value;
                Decompress();
                CalculateHash();
            }
        }

        /// <summary>
        /// Indicates if data can be obtained only by generator (thus we no need to send bytes)
        /// </summary>
        public bool PureGenerated { get; set; }
        
        /// <summary>
        /// Gets the MD5 hash of the chunk Data
        /// </summary>
        public byte[] Md5DataHash
        {
            get { return _md5DataHash; }
        }

        /// <summary>
        /// Chunk position
        /// </summary>
        public IntVector2 Position { get; set; }

        /// <summary>
        /// DateTime stamp of the chunk
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        /// Indicates if this chunk needs to be saved
        /// </summary>
        public bool NeedSave { get; set; }

        private void Compress()
        {
            if (_data != null)
            {
                var ms = new MemoryStream();
                using (var zip = new GZipStream(ms, CompressionMode.Compress))
                {
                    zip.Write(_data, 0, _data.Length);
                }
                _dataCompressed = ms.ToArray();
            }
            else _dataCompressed = null;
        }

        private void Decompress()
        {
            if (_dataCompressed != null)
            {
                if (_data == null) _data = new byte[ChunkByteLength];
                var ms = new MemoryStream(_dataCompressed);
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(_data, 0, ChunkByteLength);
                }
            }
            else { _data = null; }
        }
        
        public byte GetBlock(Location3<int> inBlockPosition)
        {
            return _data[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z];
        }

        public void SetBlock(Location3<int> inBlockPosition, byte value)
        {
            if (_data == null)
            {
                _data = new byte[ChunkByteLength];
            }
            _data[inBlockPosition.X * _chunkSize.X * _chunkSize.Y + inBlockPosition.Y * _chunkSize.Y + inBlockPosition.Z] = value;
            
            Compress();
            CalculateHash();
            PureGenerated = false;
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

        private void CalculateHash()
        {
            if (_data == null)
            {
                _md5DataHash = null;
                return;
            }
            var provider = new MD5CryptoServiceProvider();
            _md5DataHash = provider.ComputeHash(_data);
        }

    }
}
