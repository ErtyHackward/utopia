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
        public byte MessageId;
        public ErrorCodes ErrorCode;
        public int Data;
        public string Message;

        public static ErrorMessage Read(BinaryReader reader)
        {
            ErrorMessage msg;

            msg.MessageId = reader.ReadByte();
            msg.ErrorCode = (ErrorCodes)reader.ReadByte();
            msg.Data = reader.ReadInt32();
            msg.Message = reader.ReadString();
            
            return msg;
        }

        public static void Write(BinaryWriter writer, ErrorMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write((byte)msg.ErrorCode);
            writer.Write(msg.Data);
            writer.Write(msg.Message);
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.Error;
            Write(writer, this);
        }
    }

    public enum ErrorCodes : byte
    {
        LoginPasswordIncorrect,
        LoginAlreadyRegistered,
        VersionMissmatch,
        ChunkTooFar
    }
}
