using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Net.Interfaces;
using S33M3Engines.Shared.Math;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about entity position change event
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityPositionMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification number of the entity
        /// </summary>
        private uint _entityId;
        /// <summary>
        /// Current position of the entity
        /// </summary>
        private DVector3 _position;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityPosition; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        /// <summary>
        /// Gets or sets a current position of the player
        /// </summary>
        public DVector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public static EntityPositionMessage Read(BinaryReader reader)
        {
            EntityPositionMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._position.X = reader.ReadSingle();
            msg._position.Y = reader.ReadSingle();
            msg._position.Z = reader.ReadSingle();

            return msg;
        }

        public static void Write(BinaryWriter writer, EntityPositionMessage msg)
        {
            writer.Write(msg._entityId);
            writer.Write(msg._position.X);
            writer.Write(msg._position.Y);
            writer.Write(msg._position.Z);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
