using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that informs player that another player somewhere near
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerInMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Player identification number
        /// </summary>
        public int UserId;
        /// <summary>
        /// Login of the player
        /// </summary>
        public string Login;

        public static PlayerInMessage Read(BinaryReader reader)
        {
            PlayerInMessage msg;
            msg.MessageId = reader.ReadByte();
            msg.UserId = reader.ReadInt32();
            msg.Login = reader.ReadString();
            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerInMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.UserId);
            writer.Write(msg.Login);
        }
        
        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.PlayerIn;
            Write(writer, this);
        }
    }
}
