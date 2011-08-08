using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// BlockChange message is sent when some entity or player does modification of the single block 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockChangeMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Global block position
        /// </summary>
        public Location3<int> BlockPosition;
        /// <summary>
        /// Block type
        /// </summary>
        public byte BlockType;
        
        public static BlockChangeMessage Read(BinaryReader reader)
        {
            BlockChangeMessage bcm;

            bcm.MessageId = reader.ReadByte();
            bcm.BlockPosition.X = reader.ReadInt32();
            bcm.BlockPosition.Y = reader.ReadInt32();
            bcm.BlockPosition.Z = reader.ReadInt32();

            bcm.BlockType = reader.ReadByte();


            return bcm;
        }

        public static void Write(BinaryWriter writer, BlockChangeMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.BlockPosition.X);
            writer.Write(msg.BlockPosition.Y);
            writer.Write(msg.BlockPosition.Z);
            writer.Write(msg.BlockType);
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.BlockChange;
            Write(writer, this);
        }
    }
}
