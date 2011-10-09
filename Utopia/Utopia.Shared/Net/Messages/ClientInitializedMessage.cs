using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Message send by client to warn server that it is ready to receive game data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ClientInitializedMessage
    {
        private bool _isInitialized;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.ClientInitialized; }
        }

        /// <summary>
        /// Client initialized ?
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { _isInitialized = value; }
        }

        public static ClientInitializedMessage Read(BinaryReader reader)
        {
            ClientInitializedMessage msg = new ClientInitializedMessage();

            msg.IsInitialized = reader.ReadBoolean();
            return msg;
        }

        public static void Write(BinaryWriter writer, ClientInitializedMessage msg)
        {
            writer.Write(msg.IsInitialized);
        }
    }
}
