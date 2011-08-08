using System;
using System.IO;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform clients about current time and date
    /// </summary>
    public struct DateTimeMessage : IBinaryMessage
    {
        public byte MessageId;
        public DateTime DateTime;

        public static DateTimeMessage Read(BinaryReader reader)
        {
            DateTimeMessage msg;
            msg.MessageId = reader.ReadByte();
            msg.DateTime = DateTime.FromBinary(reader.ReadInt64());

            return msg;
        }

        public static void Write(BinaryWriter writer, DateTimeMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.DateTime.ToBinary());
        }

        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.DateTime;
            Write(writer, this);
        }


    }
}
