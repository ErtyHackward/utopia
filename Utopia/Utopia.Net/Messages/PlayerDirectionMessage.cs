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
        public byte MessageId;
        /// <summary>
        /// User identification number
        /// </summary>
        public int UserId;
        /// <summary>
        /// Actual direction vector of the player
        /// </summary>
        public Vector3 Direction;
        
        public static PlayerDirectionMessage Read(BinaryReader reader)
        {
            
            PlayerDirectionMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.UserId = reader.ReadInt32();
            msg.Direction.X = reader.ReadSingle();
            msg.Direction.Y = reader.ReadSingle();
            msg.Direction.Z = reader.ReadSingle();

            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerDirectionMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.UserId);
            writer.Write(msg.Direction.X);
            writer.Write(msg.Direction.Y);
            writer.Write(msg.Direction.Z);
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.PlayerDirection;
            Write(writer, this);
        }
    }
}
