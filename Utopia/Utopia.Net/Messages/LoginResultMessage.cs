using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that used to inform client about login operation result
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LoginResultMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Indicates if login procedure succeed
        /// </summary>
        public bool Logged;
        
        public static LoginResultMessage Read(BinaryReader reader)
        {
            LoginResultMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.Logged = reader.ReadBoolean();
            
            return msg;
        }

        public static void Write(BinaryWriter writer, LoginResultMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.Logged);
            
        }

        #region IBinaryWritable Members

        public void Write(System.IO.BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.LoginResult;
            Write(writer, this);
        }

        #endregion
    }
}
