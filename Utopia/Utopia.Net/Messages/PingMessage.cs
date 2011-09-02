using System.IO;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Describes message for connection testing
    /// </summary>
    public struct PingMessage : IBinaryMessage
    {
        private int _token;
        private bool _request;

        /// <summary>
        /// Any integer number to verify ping command
        /// </summary>
        public int Token
        {
            get { return _token; }
            set { _token = value; }
        }
        
        /// <summary>
        /// Indicates whether this command is request or responce
        /// </summary>
        public bool Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.Ping; }
        }

        public static PingMessage Read(BinaryReader reader)
        {
            PingMessage msg;

            msg._request = reader.ReadBoolean();
            msg._token = reader.ReadInt32();
            

            return msg;
        }

        public static void Write(BinaryWriter writer, PingMessage msg)
        {
            writer.Write(msg._request);
            writer.Write(msg._token);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
