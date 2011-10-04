using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that inform about change in view direction of the entity
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityDirectionMessage : IBinaryMessage
    {
        /// <summary>
        /// entity identification number
        /// </summary>
        private uint _entityId;
        /// <summary>
        /// Actual direction quaternion of the entity
        /// </summary>
        private Quaternion _direction;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityDirection; }
        }

        /// <summary>
        /// Gets or sets an entity identification number
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        /// <summary>
        /// Gets or sets an actual direction quaternion of the entity
        /// </summary>
        public Quaternion Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public static EntityDirectionMessage Read(BinaryReader reader)
        {
            
            EntityDirectionMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._direction = reader.ReadQuaternion();

            return msg;
        }

        public static void Write(BinaryWriter writer, EntityDirectionMessage msg)
        {
            writer.Write(msg._entityId);
            writer.Write(msg._direction);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
