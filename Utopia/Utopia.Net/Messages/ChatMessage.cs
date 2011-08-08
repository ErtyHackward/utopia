using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Chat message is sent to exchange messages between clients, or to inform client by server
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChatMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Login of the sender, can be null if system message
        /// </summary>
        public string Login;
        /// <summary>
        /// Actual message text
        /// </summary>
        public string Message;

        public static ChatMessage Read(BinaryReader reader)
        {
            ChatMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.Login = reader.ReadString();
            msg.Message = reader.ReadString();

            return msg;
        }

        public static void Write(BinaryWriter writer, ChatMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.Login);
            writer.Write(msg.Message);
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.Chat;
            Write(writer, this);
        }
    }
}
