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
        private DateTime _dateTime;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.DateTime; }
        }

        /// <summary>
        /// Gets or sets game DateTime
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        public static DateTimeMessage Read(BinaryReader reader)
        {
            DateTimeMessage msg;

            msg._dateTime = DateTime.FromBinary(reader.ReadInt64());

            return msg;
        }

        public static void Write(BinaryWriter writer, DateTimeMessage msg)
        {
            writer.Write(msg._dateTime.ToBinary());
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
