using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that can be sent by the server in responce to the GetChunks message. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkDataMessage : IBinaryMessage
    {
        /// <summary>
        /// Chunk position
        /// </summary>
        private Vector2I _position;
        /// <summary>
        /// Result type flag. See the flag members to details
        /// </summary>
        private ChunkDataMessageFlag _flag;
        /// <summary>
        /// Variable amount of bytes, can be the chunk data or md5 hash (depends on flag state)
        /// </summary>
        private byte[] _data;
        private Md5Hash _chunkHash;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.ChunkData; }
        }

        /// <summary>
        /// Value filled in with the datetime at message creation time
        /// </summary>
        public System.DateTime MessageRecTime { get; set; }

        /// <summary>
        /// Gets or sets chunk position
        /// </summary>
        public Vector2I Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// Gets or sets chunk md5 hash
        /// </summary>
        public Md5Hash ChunkHash
        {
            get { return _chunkHash; }
            set { _chunkHash = value; }
        }

        /// <summary>
        /// Gets or sets result type flag. See the flag members to details
        /// </summary>
        public ChunkDataMessageFlag Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        /// <summary>
        /// Gets or sets variable amount of bytes, can be the chunk data or md5 hash (depends on flag state)
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public static void Write(BinaryWriter writer, ChunkDataMessage msg)
        {
            writer.Write(msg._position.X);
            writer.Write(msg._position.Y);
            writer.Write((byte)msg._flag);

            if (msg._chunkHash == null || msg._chunkHash.Bytes == null)
                throw new InvalidDataException("Chunk md5 hash is not valid");

            writer.Write(msg._chunkHash.Bytes);

            if (msg._data != null)
            {
                writer.Write(msg._data.Length);
                if (msg._data.Length > 0)
                {
                    writer.Write(msg._data);
                }
            }
            else writer.Write(0);
        }

        public static ChunkDataMessage Read(BinaryReader reader)
        {
            ChunkDataMessage msg = new ChunkDataMessage();

            msg._position.X = reader.ReadInt32();
            msg._position.Y = reader.ReadInt32();
            msg._flag = (ChunkDataMessageFlag)reader.ReadByte();
            msg.MessageRecTime = System.DateTime.Now;
            
            var hashBytes = reader.ReadBytes(16);

            if(hashBytes.Length != 16)
                throw new EndOfStreamException();

            msg._chunkHash = new Md5Hash(hashBytes);

            var bytesCount = reader.ReadInt32();

            msg._data = bytesCount > 0 ? reader.ReadBytes(bytesCount) : null;

            if (msg._data != null && bytesCount != msg._data.Length)
                throw new EndOfStreamException();

            return msg;
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }

    }

    public enum ChunkDataMessageFlag : byte
    {
        /// <summary>
        /// Client should use chunk data from the server (will be attached to this message)
        /// </summary>
        ChunkWasModified = 0,
        /// <summary>
        /// In this case data will contain md5 hash of resulted chunk, the client shuold generate the chunk
        /// </summary>
        ChunkCanBeGenerated = 1,
        /// <summary>
        /// In this case the message will contain no data, the client should obtain the chunk data locally
        /// </summary>
        ChunkMd5Equal = 2
    }

}
