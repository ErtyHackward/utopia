using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a request for one or more voxel models
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GetVoxelModelsMessage : IBinaryMessage
    {
        private Md5Hash[] _md5Hashes;
        
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetVoxelModels; }
        }

        /// <summary>
        /// Gets or sets a set of md5 hash values of requested models
        /// </summary>
        public Md5Hash[] Md5Hashes
        {
            get { return _md5Hashes; }
            set { _md5Hashes = value; }
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }

        public static void Write(BinaryWriter writer, GetVoxelModelsMessage msg)
        {
            writer.Write(msg._md5Hashes.Length);
            foreach (var md5Hash in msg._md5Hashes)
            {
                writer.Write(md5Hash.Bytes);
            }
        }

        public static GetVoxelModelsMessage Read(BinaryReader reader)
        {
            GetVoxelModelsMessage msg;

            var count = reader.ReadInt32();

            msg._md5Hashes = new Md5Hash[count];

            for (int i = 0; i < count; i++)
            {
                var bytes = reader.ReadBytes(16);
                if (bytes.Length != 16) throw new EndOfStreamException();
                msg._md5Hashes[i] = new Md5Hash(bytes);
            }

            return msg;
        }
    }
}
