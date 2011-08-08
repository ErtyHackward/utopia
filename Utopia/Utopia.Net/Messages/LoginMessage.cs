using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message used by client to log in to the server
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LoginMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// User login
        /// </summary>
        public string Login;
        /// <summary>
        /// User password md5 hash
        /// </summary>
        public string Password;
        /// <summary>
        /// True if client ask to register
        /// </summary>
        public bool Register;
        /// <summary>
        /// Client software version
        /// </summary>
        public int Version;

        public static LoginMessage Read(BinaryReader reader)
        {
            LoginMessage msg;
            msg.MessageId = reader.ReadByte();
            msg.Login = reader.ReadString();
            msg.Password = reader.ReadString();
            msg.Register = reader.ReadBoolean();
            msg.Version = reader.ReadInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, LoginMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.Login);
            writer.Write(msg.Password);
            writer.Write(msg.Register);
            writer.Write(msg.Version);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
