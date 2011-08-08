using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Describes a message used to send game specified information to client
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInformationMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Defines a maximum chunk distance that client can query (in chunks)
        /// </summary>
        public int MaxViewRange;
        /// <summary>
        /// Defines a chunk size used on the server
        /// </summary>
        public Location3<int> ChunkSize;

        public static GameInformationMessage Read(BinaryReader reader)
        {
            GameInformationMessage gi;

            gi.MessageId = reader.ReadByte();
            gi.MaxViewRange = reader.ReadInt32();

            gi.ChunkSize.X = reader.ReadInt32();
            gi.ChunkSize.Y = reader.ReadInt32();
            gi.ChunkSize.Z = reader.ReadInt32();
            
            return gi;
        }

        public static void Write(BinaryWriter writer, GameInformationMessage info)
        {
            writer.Write(info.MessageId);
            writer.Write(info.MaxViewRange);
            writer.Write(info.ChunkSize.X);
            writer.Write(info.ChunkSize.Y);
            writer.Write(info.ChunkSize.Z);
        }
        
        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.GameInformation;
            Write(writer, this);
        }
    }
}
