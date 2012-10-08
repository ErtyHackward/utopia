using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.PlanGenerator;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.World;

namespace Utopia.Shared.Net.Messages
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
        private Vector3I _chunkSize;

        private WorldParameters _worldParameter;

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
        public Vector3I ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        /// <summary>
        /// Contains plan generation details
        /// </summary>
        public WorldParameters WorldParameter
        {
            get { return _worldParameter; }
            set { _worldParameter = value; }
        }

        public static GameInformationMessage Read(BinaryReader reader)
        {
            GameInformationMessage gi;

            gi._maxViewRange = reader.ReadInt32();

            gi._chunkSize = reader.ReadVector3I();
            WorldParameters _worldParameter = new WorldParameters();
            _worldParameter.Load(reader);
            gi._worldParameter = _worldParameter;

            return gi;
        }

        public static void Write(BinaryWriter writer, GameInformationMessage info)
        {
            writer.Write(info._maxViewRange);
            writer.Write(info._chunkSize);
            info.WorldParameter.Save(writer);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
