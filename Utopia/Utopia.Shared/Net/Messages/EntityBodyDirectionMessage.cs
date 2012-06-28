﻿using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that inform about change in view direction of the entity
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityBodyDirectionMessage : IBinaryMessage
    {
        /// <summary>
        /// entity identification number
        /// </summary>
        private uint _entityId;
        /// <summary>
        /// Actual direction quaternion of the entity
        /// </summary>
        private Quaternion _rotation;

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
        public Quaternion Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public static EntityBodyDirectionMessage Read(BinaryReader reader)
        {
            EntityBodyDirectionMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._rotation = reader.ReadQuaternion();

            return msg;
        }

        public static void Write(BinaryWriter writer, EntityBodyDirectionMessage msg)
        {
            writer.Write(msg._entityId);
            writer.Write(msg._rotation);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
