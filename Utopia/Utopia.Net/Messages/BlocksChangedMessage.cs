using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// BlockChange message is sent when some entity or player does modification of some amount of blocks 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlocksChangedMessage : IBinaryMessage
    {
        private Vector3I[] _blockPositions;
        private Vector2I _chunkPosition;
        private byte[] _blockValues;

        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.BlockChange; }
        }

        /// <summary>
        /// Gets or sets local blocks position
        /// </summary>
        public Vector3I[] BlockPositions
        {
            get { return _blockPositions; }
            set { _blockPositions = value; }
        }

        /// <summary>
        /// Gets or sets blocks id values
        /// </summary>
        public byte[] BlockValues
        {
            get { return _blockValues; }
            set { _blockValues = value; }
        }

        
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }

        public static BlocksChangedMessage Read(BinaryReader reader)
        {
            BlocksChangedMessage bcm;

            bcm._chunkPosition = reader.ReadIntVector2();
            var count = reader.ReadInt32();

            var positions = new Vector3I[count];
            var values = new byte[count];

            for (int i = 0; i < count; i++)
            {
                positions[i] = reader.ReadIntLocation3();
                values[i] = reader.ReadByte();
            }

            bcm._blockPositions = positions;
            bcm._blockValues = values;

            return bcm;
        }

        public static void Write(BinaryWriter writer, BlocksChangedMessage msg)
        {
            writer.Write(msg._chunkPosition);
            writer.Write(msg._blockPositions.Length);

            for (int i = 0; i < msg._blockPositions.Length; i++)
            {
                writer.Write(msg._blockPositions[i]);
                writer.Write(msg._blockValues[i]);
            }
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
