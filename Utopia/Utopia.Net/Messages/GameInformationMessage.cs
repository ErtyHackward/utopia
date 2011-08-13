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
        /// <summary>
        /// Defines a maximum chunk distance that client can query (in chunks)
        /// </summary>
        private int _maxViewRange;
        /// <summary>
        /// Defines a chunk size used on the server
        /// </summary>
        private Location3<int> _chunkSize;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GameInformation; }
        }

        /// <summary>
        /// Gets or sets a maximum chunk distance that client can query (in chunks)
        /// </summary>
        public int MaxViewRange
        {
            get { return _maxViewRange; }
            set { _maxViewRange = value; }
        }

        /// <summary>
        /// Gets or sets a chunk size used on the server
        /// </summary>
        public Location3<int> ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        public static GameInformationMessage Read(BinaryReader reader)
        {
            GameInformationMessage gi;

            gi._maxViewRange = reader.ReadInt32();

            gi._chunkSize.X = reader.ReadInt32();
            gi._chunkSize.Y = reader.ReadInt32();
            gi._chunkSize.Z = reader.ReadInt32();
            
            return gi;
        }

        public static void Write(BinaryWriter writer, GameInformationMessage info)
        {
            writer.Write(info._maxViewRange);
            writer.Write(info._chunkSize.X);
            writer.Write(info._chunkSize.Y);
            writer.Write(info._chunkSize.Z);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
