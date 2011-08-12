using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that inform about change in view direction of the player
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerDirectionMessage : IBinaryMessage
    {
        /// <summary>
        /// User identification number
        /// </summary>
        private int _userId;
        /// <summary>
        /// Actual direction vector of the player
        /// </summary>
        private Vector3 _direction;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.PlayerDirection; }
        }

        /// <summary>
        /// Gets or sets a user identification number
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// Gets or sets an actual direction vector of the player
        /// </summary>
        public Vector3 Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public static PlayerDirectionMessage Read(BinaryReader reader)
        {
            
            PlayerDirectionMessage msg;

            msg._userId = reader.ReadInt32();
            msg._direction.X = reader.ReadSingle();
            msg._direction.Y = reader.ReadSingle();
            msg._direction.Z = reader.ReadSingle();

            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerDirectionMessage msg)
        {
            writer.Write(msg._userId);
            writer.Write(msg._direction.X);
            writer.Write(msg._direction.Y);
            writer.Write(msg._direction.Z);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
