using System.IO;
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
        public byte MessageId;
        /// <summary>
        /// Identification number of the player
        /// </summary>
        public int UserId;
        /// <summary>
        /// Current position of the player
        /// </summary>
        public Vector3 Position;

        public static PlayerPositionMessage Read(BinaryReader reader)
        {
            PlayerPositionMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.UserId = reader.ReadInt32();
            msg.Position.X = reader.ReadSingle();
            msg.Position.Y = reader.ReadSingle();
            msg.Position.Z = reader.ReadSingle();

            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerPositionMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.UserId);
            writer.Write(msg.Position.X);
            writer.Write(msg.Position.Y);
            writer.Write(msg.Position.Z);
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.PlayerPosition;
            Write(writer, this);
        }
    }
}
