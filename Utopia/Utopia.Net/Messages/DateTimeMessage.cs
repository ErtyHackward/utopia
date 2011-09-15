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
        private double _timeFactor;

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

        /// <summary>
        /// Gets or sets how many game seconds in one real second
        /// </summary>
        public double TimeFactor
        {
            get { return _timeFactor; }
            set { _timeFactor = value; }
        }

        public static DateTimeMessage Read(BinaryReader reader)
        {
            DateTimeMessage msg;

            msg._dateTime = DateTime.FromBinary(reader.ReadInt64());
            msg._timeFactor = reader.ReadDouble();
            return msg;
        }

        public static void Write(BinaryWriter writer, DateTimeMessage msg)
        {
            writer.Write(msg._dateTime.ToBinary());
            writer.Write(msg._timeFactor);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
