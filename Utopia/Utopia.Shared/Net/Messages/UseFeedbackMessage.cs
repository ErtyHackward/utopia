using System.IO;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform about tool use result
    /// </summary>
    public struct UseFeedbackMessage : IBinaryMessage
    {
        private int _token;
        private byte[] _entityImpactBytes;

        /// <summary>
        /// Identification token of the use operation
        /// </summary>
        public int Token
        {
            get { return _token; }
            set { _token = value; }
        }
        
        /// <summary>
        /// Serialized bytes of the EntityImpact
        /// </summary>
        public byte[] EntityImpactBytes
        {
            get { return _entityImpactBytes; }
            set { _entityImpactBytes = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.UseFeedback; }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_token);
            writer.Write(_entityImpactBytes.Length);
            writer.Write(_entityImpactBytes);
        }

        public static UseFeedbackMessage Read(BinaryReader reader)
        {
            UseFeedbackMessage msg;

            msg._token = reader.ReadInt32();

            var bytesLength = reader.ReadInt32();

            msg._entityImpactBytes = reader.ReadBytes(bytesLength);

            if (msg._entityImpactBytes.Length != bytesLength)
                throw new EndOfStreamException();
            
            return msg;
        }
    }
}
