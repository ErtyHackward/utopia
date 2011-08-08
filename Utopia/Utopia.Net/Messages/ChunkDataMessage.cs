using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that can be sent by the server in responce to the GetChunks message. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkDataMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Chunk position
        /// </summary>
        public IntVector2 Position;
        /// <summary>
        /// Result type flag. See the flag members to details
        /// </summary>
        public ChunkDataMessageFlag Flag;
        /// <summary>
        /// Variable amount of bytes, can be the chunk data or md5 hash (depends on flag state)
        /// </summary>
        public byte[] Data;

        public static void Write(BinaryWriter writer, ChunkDataMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.Position.X);
            writer.Write(msg.Position.Y);
            writer.Write((byte)msg.Flag);
            if (msg.Data != null)
            {
                writer.Write(msg.Data.Length);
                if (msg.Data.Length > 0)
                {
                    writer.Write(msg.Data);
                }
            }
            else writer.Write(0);
        }

        public static ChunkDataMessage Read(BinaryReader reader)
        {
            ChunkDataMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.Position.X = reader.ReadInt32();
            msg.Position.Y = reader.ReadInt32();
            msg.Flag = (ChunkDataMessageFlag)reader.ReadByte();

            var bytesCount = reader.ReadInt32();

            msg.Data = bytesCount > 0 ? reader.ReadBytes(bytesCount) : null;

            return msg;
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.ChunkData;
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
