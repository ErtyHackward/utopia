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
        public byte MessageId;
        public IntVector2 StartPosition;
        public IntVector2 EndPosition;

        /// <summary>
        /// Request mode
        /// </summary>
        public GetChunksMessageFlag Flag;

        /// <summary>
        /// Count of hashes
        /// </summary>
        public int HashesCount;
        /// <summary>
        /// Corresponding positions array of size HashesCount
        /// </summary>
        public IntVector2[] Positions;
        /// <summary>
        /// Corresponding md5 hashes array of size HashesCount, each hash must be 16 bytes length
        /// </summary>
        public byte[][] Md5Hashes;

        public static GetChunksMessage Read(BinaryReader reader)
        {
            GetChunksMessage msg;

            msg.MessageId = reader.ReadByte();
            
            msg.StartPosition.X = reader.ReadInt32();
            msg.StartPosition.Y = reader.ReadInt32();
            
            msg.EndPosition.X = reader.ReadInt32();
            msg.EndPosition.Y = reader.ReadInt32();

            msg.Flag = (GetChunksMessageFlag)reader.ReadByte();

            msg.HashesCount = reader.ReadInt32();

            msg.Positions = new IntVector2[msg.HashesCount];
            msg.Md5Hashes = new byte[msg.HashesCount][];

            for (int i = 0; i < msg.HashesCount; i++)
            {
                msg.Positions[i].X = reader.ReadInt32();
                msg.Positions[i].Y = reader.ReadInt32();
                msg.Md5Hashes[i] = reader.ReadBytes(16);
            }
            
            return msg;
        }

        public static void Write(BinaryWriter writer, GetChunksMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.StartPosition.X);
            writer.Write(msg.StartPosition.Y);
            writer.Write(msg.EndPosition.X);
            writer.Write(msg.EndPosition.Y);
            writer.Write((byte)msg.Flag);

            if (msg.Positions != null)
            {
                writer.Write(msg.Positions.Length);
                for (int i = 0; i < msg.Positions.Length; i++)
                {
                    writer.Write(msg.Positions[i].X);
                    writer.Write(msg.Positions[i].Y);
                    writer.Write(msg.Md5Hashes[i]);
                }
            }
            else writer.Write(0);
        }
        
        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.GetChunks;
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
