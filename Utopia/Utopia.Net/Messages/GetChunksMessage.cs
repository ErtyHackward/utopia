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
        private Range2 _range;
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
        private Vector2I[] _positions;
        /// <summary>
        /// Corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        private Md5Hash[] _md5Hashes;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetChunks; }
        }

        /// <summary>
        /// Gets or sets chunks range
        /// </summary>
        public Range2 Range
        {
            get { return _range; }
            set { _range = value; }
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
        public Vector2I[] Positions
        {
            get { return _positions; }
            set { _positions = value; }
        }

        /// <summary>
        /// Gets or sets corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        public Md5Hash[] Md5Hashes
        {
            get { return _md5Hashes; }
            set { _md5Hashes = value; }
        }

        public static GetChunksMessage Read(BinaryReader reader)
        {
            GetChunksMessage msg;
            
            msg._range = reader.ReadRange2();
            
            msg._flag = (GetChunksMessageFlag)reader.ReadByte();

            msg._hashesCount = reader.ReadInt32();

            if (msg._hashesCount > 0)
            {
                msg._positions = new Vector2I[msg._hashesCount];
                msg._md5Hashes = new Md5Hash[msg._hashesCount];
                
                for (int i = 0; i < msg._hashesCount; i++)
                {
                    msg._positions[i] = reader.ReadVector2I();
                    var bytes = reader.ReadBytes(16);
                    if (bytes.Length != 16) 
                        throw new EndOfStreamException();
                    msg._md5Hashes[i] = new Md5Hash(bytes);
                }
            }
            else
            {
                msg._positions = null;
                msg._md5Hashes = null;
            }

            return msg;
        }

        public static void Write(BinaryWriter writer, GetChunksMessage msg)
        {
            writer.Write(msg._range);
            writer.Write((byte)msg._flag);

            if (msg._positions != null)
            {
                writer.Write(msg._positions.Length);
                for (int i = 0; i < msg._positions.Length; i++)
                {
                    writer.Write(msg._positions[i]);
                    writer.Write(msg._md5Hashes[i].Bytes);
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
