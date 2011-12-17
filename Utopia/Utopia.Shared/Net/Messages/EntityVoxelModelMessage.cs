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
        private Md5Hash _hash;

        /// <summary>
        /// Link to the entity
        /// </summary>
        public EntityLink EntityLink
        {
            get { return _entityLink; }
            set { _entityLink = value; }
        }
        
        /// <summary>
        /// New voxel model hash
        /// </summary>
        public Md5Hash Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityVoxelModel; }
        }

        public static EntityVoxelModelMessage Read(BinaryReader reader)
        {
            EntityVoxelModelMessage msg;
            msg._entityLink = reader.ReadEntityLink();
            msg._hash = reader.ReadMd5Hash();
            return msg;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_entityLink);
            writer.Write(_hash);
        }
    }
}
