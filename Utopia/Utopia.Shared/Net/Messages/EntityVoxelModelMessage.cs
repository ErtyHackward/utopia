using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs about voxel model change
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityVoxelModelMessage: IBinaryMessage
    {
        private EntityLink _entityLink;
        private byte[] _bytes;

        /// <summary>
        /// Link to the entity
        /// </summary>
        public EntityLink EntityLink
        {
            get { return _entityLink; }
            set { _entityLink = value; }
        }
        
        /// <summary>
        /// Serialized visual model 
        /// </summary>
        public byte[] Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityVoxelModel; }
        }

        public static EntityVoxelModelMessage Read(BinaryReader reader)
        {
            EntityVoxelModelMessage msg;

            msg._entityLink = reader.ReadEntityLink();
            var bytesCount = reader.ReadInt32();
            msg._bytes = reader.ReadBytes(bytesCount);

            if (msg._bytes.Length != bytesCount)
            {
                throw new EndOfStreamException();
            }

            return msg;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_entityLink);
            writer.Write(_bytes.Length);
            writer.Write(_bytes);
        }
    }
}
