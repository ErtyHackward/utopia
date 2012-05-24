using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Net.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// BlockChange message is sent when some entity or player does modification of some amount of blocks 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlocksChangedMessage : IBinaryMessage
    {
        private Vector3I[] _blockPositions;
        private byte[] _blockValues;
        private BlockTag[] _tags;

        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.BlockChange; }
        }

        /// <summary>
        /// Gets or sets global blocks position
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

        /// <summary>
        /// According block tags (can be null, otherwise tags array will correspond the order of BlockPosition array, each value of the array still can be null)
        /// </summary>
        public BlockTag[] Tags
        {
            get { return _tags; }
            set { _tags = value; }
        }

        public static BlocksChangedMessage Read(BinaryReader reader)
        {
            BlocksChangedMessage bcm;

            var count = reader.ReadInt32();

            var positions = new Vector3I[count];
            var values = new byte[count];
            var tags = new BlockTag[count];

            for (int i = 0; i < count; i++)
            {
                positions[i] = reader.ReadVector3I();
                values[i] = reader.ReadByte();
                tags[i] = EntityFactory.CreateTagFromBytes(reader);
            }

            bcm._blockPositions = positions;
            bcm._blockValues = values;
            bcm._tags = tags;

            return bcm;
        }

        public static void Write(BinaryWriter writer, BlocksChangedMessage msg)
        {
            writer.Write(msg._blockPositions.Length);

            for (var i = 0; i < msg._blockPositions.Length; i++)
            {
                writer.Write(msg._blockPositions[i]);
                writer.Write(msg._blockValues[i]);
                if (msg._tags == null || msg._tags[i] == null)
                    writer.Write((byte)0);
                else
                    msg._tags[i].Save(writer);
            }
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
