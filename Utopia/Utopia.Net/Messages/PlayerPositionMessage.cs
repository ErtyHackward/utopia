﻿using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about player position change event
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerPositionMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification number of the player
        /// </summary>
        private int _userId;
        /// <summary>
        /// Current position of the player
        /// </summary>
        private Vector3 _position;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.PlayerPosition; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// Gets or sets a current position of the player
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public static PlayerPositionMessage Read(BinaryReader reader)
        {
            PlayerPositionMessage msg;

            msg._userId = reader.ReadInt32();
            msg._position.X = reader.ReadSingle();
            msg._position.Y = reader.ReadSingle();
            msg._position.Z = reader.ReadSingle();

            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerPositionMessage msg)
        {
            writer.Write(msg._userId);
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
