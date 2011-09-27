using System.IO;
using SharpDX;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message to describe physical impulse to some entity. Entity may change its position or respond with opposite
    /// </summary>
    public struct EntityImpulseMessage : IBinaryMessage
    {
        private uint _entityId;
        private Vector3 _vector3;

        /// <summary>
        /// Entity is affected by impulse
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }
        
        /// <summary>
        /// Impulse vector
        /// </summary>
        public Vector3 Vector3
        {
            get { return _vector3; }
            set { _vector3 = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityImpulse; }
        }

        public static EntityImpulseMessage Read(BinaryReader reader)
        {
            EntityImpulseMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._vector3 = reader.ReadVector3();

            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_entityId);
            writer.Write(_vector3);
        }
    }
}
