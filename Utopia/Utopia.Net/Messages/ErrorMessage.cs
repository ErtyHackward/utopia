using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Message used to inform the client about some of urgent event
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorMessage : IBinaryMessage
    {
        private ErrorCodes _errorCode;
        private int _data;
        private string _message;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Error; }
        }

        /// <summary>
        /// Gets or sets message error code
        /// </summary>
        public ErrorCodes ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        /// <summary>
        /// Gets or sets additinal error data
        /// </summary>
        public int Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Gets or sets error description
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public static ErrorMessage Read(BinaryReader reader)
        {
            ErrorMessage msg;

            msg._errorCode = (ErrorCodes)reader.ReadByte();
            msg._data = reader.ReadInt32();
            msg._message = reader.ReadString();
            
            return msg;
        }

        public static void Write(BinaryWriter writer, ErrorMessage msg)
        {
            writer.Write((byte)msg.ErrorCode);
            writer.Write(msg.Data);
            writer.Write(msg.Message);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }

    public enum ErrorCodes : byte
    {
        LoginPasswordIncorrect,
        LoginAlreadyRegistered,
        VersionMissmatch,
        ChunkTooFar,
        AnotherInstanceLogged
    }
}
