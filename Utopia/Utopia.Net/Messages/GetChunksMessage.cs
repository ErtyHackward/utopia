using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defindes a message used by client to request a range of chunks from the server
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GetChunksMessage : IBinaryMessage
    {
        private IntVector2 _startPosition;
        private IntVector2 _endPosition;

        /// <summary>
        /// Request mode
        /// </summary>
        private GetChunksMessageFlag _flag;

        /// <summary>
        /// Count of hashes
        /// </summary>
        private int _hashesCount;
        /// <summary>
        /// Corresponding positions array of size HashesCount
        /// </summary>
        private IntVector2[] _positions;
        /// <summary>
        /// Corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        private byte[][] _md5Hashes;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetChunks; }
        }

        /// <summary>
        /// Gets or sets region start position
        /// </summary>
        public IntVector2 StartPosition
        {
            get { return _startPosition; }
            set { _startPosition = value; }
        }

        /// <summary>
        /// Gets or sets region end position
        /// </summary>
        public IntVector2 EndPosition
        {
            get { return _endPosition; }
            set { _endPosition = value; }
        }

        /// <summary>
        /// Gets or sets request mode flag
        /// </summary>
        public GetChunksMessageFlag Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        /// <summary>
        /// Gets or sets a count of hashes
        /// </summary>
        public int HashesCount
        {
            get { return _hashesCount; }
            set { _hashesCount = value; }
        }

        /// <summary>
        /// Gets or sets corresponding positions array of size HashesCount
        /// </summary>
        public IntVector2[] Positions
        {
            get { return _positions; }
            set { _positions = value; }
        }

        /// <summary>
        /// Gets or sets corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        public byte[][] Md5Hashes
        {
            get { return _md5Hashes; }
            set { _md5Hashes = value; }
        }

        public static GetChunksMessage Read(BinaryReader reader)
        {
            GetChunksMessage msg;
            
            msg._startPosition.X = reader.ReadInt32();
            msg._startPosition.Y = reader.ReadInt32();
            
            msg._endPosition.X = reader.ReadInt32();
            msg._endPosition.Y = reader.ReadInt32();

            msg._flag = (GetChunksMessageFlag)reader.ReadByte();

            msg._hashesCount = reader.ReadInt32();

            msg._positions = new IntVector2[msg._hashesCount];
            msg._md5Hashes = new byte[msg._hashesCount][];

            for (int i = 0; i < msg._hashesCount; i++)
            {
                msg._positions[i].X = reader.ReadInt32();
                msg._positions[i].Y = reader.ReadInt32();
                msg._md5Hashes[i] = reader.ReadBytes(16);
            }
            
            return msg;
        }

        public static void Write(BinaryWriter writer, GetChunksMessage msg)
        {
            writer.Write(msg._startPosition.X);
            writer.Write(msg._startPosition.Y);
            writer.Write(msg._endPosition.X);
            writer.Write(msg._endPosition.Y);
            writer.Write((byte)msg._flag);

            if (msg._positions != null)
            {
                writer.Write(msg._positions.Length);
                for (int i = 0; i < msg._positions.Length; i++)
                {
                    writer.Write(msg._positions[i].X);
                    writer.Write(msg._positions[i].Y);
                    writer.Write(msg._md5Hashes[i]);
                }
            }
            else writer.Write(0);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }

    public enum GetChunksMessageFlag : byte
    {
        /// <summary>
        /// Normal mode
        /// </summary>
        DontSendChunkDataIfNotModified = 0,
        /// <summary>
        /// Specify this flag if generated chunk is not equal to hash provided by the server
        /// </summary>
        AlwaysSendChunkData = 1
    }
}
