using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Messages
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
        private Vector3D _position;

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
        public Vector3D Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public static EntityPositionMessage Read(BinaryReader reader)
        {
            EntityPositionMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._position = reader.ReadVector3D();
            
            return msg;
        }

        public static void Write(BinaryWriter writer, EntityPositionMessage msg)
        {
            writer.Write(msg._entityId);
            writer.Write(msg._position);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
