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
        /// <summary>
        /// Global block position
        /// </summary>
        private Location3<int> _blockPosition;

        /// <summary>
        /// Block type
        /// </summary>
        private byte _blockType;

        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.BlockChange; }
        }

        /// <summary>
        /// Gets or sets global block position
        /// </summary>
        public Location3<int> BlockPosition
        {
            get { return _blockPosition; }
            set { _blockPosition = value; }
        }

        /// <summary>
        /// Gets or sets block type
        /// </summary>
        public byte BlockType
        {
            get { return _blockType; }
            set { _blockType = value; }
        }

        public static BlockChangeMessage Read(BinaryReader reader)
        {
            BlockChangeMessage bcm;

            bcm._blockPosition.X = reader.ReadInt32();
            bcm._blockPosition.Y = reader.ReadInt32();
            bcm._blockPosition.Z = reader.ReadInt32();

            bcm._blockType = reader.ReadByte();


            return bcm;
        }

        public static void Write(BinaryWriter writer, BlockChangeMessage msg)
        {
            writer.Write(msg._blockPosition.X);
            writer.Write(msg._blockPosition.Y);
            writer.Write(msg._blockPosition.Z);
            writer.Write(msg._blockType);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
