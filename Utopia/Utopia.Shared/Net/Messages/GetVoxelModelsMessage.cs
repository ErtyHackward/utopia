using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Represents a request for one or more voxel models
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GetVoxelModelsMessage : IBinaryMessage
    {
        private string[] _names;
        
        public byte MessageId
        {
            get { return (byte)MessageTypes.GetVoxelModels; }
        }

        /// <summary>
        /// Gets or sets a set of md5 hash values of requested models
        /// </summary>
        public string[] Names
        {
            get { return _names; }
            set { _names = value; }
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }

        public static void Write(BinaryWriter writer, GetVoxelModelsMessage msg)
        {
            writer.Write(msg._names.Length);
            foreach (var name in msg._names)
            {
                writer.Write(name);
            }
        }

        public static GetVoxelModelsMessage Read(BinaryReader reader)
        {
            GetVoxelModelsMessage msg;

            var count = reader.ReadInt32();

            msg._names = new string[count];

            for (int i = 0; i < count; i++)
            {
                msg._names[i] = reader.ReadString();
            }

            return msg;
        }
    }
}
