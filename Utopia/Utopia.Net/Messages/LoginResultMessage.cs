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
        /// <summary>
        /// Indicates if login procedure succeed
        /// </summary>
        private bool _logged;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.LoginResult; }
        }

        /// <summary>
        /// Gets or sets value indicating if logon procedure was completed successfully
        /// </summary>
        public bool Logged
        {
            get { return _logged; }
            set { _logged = value; }
        }

        public static LoginResultMessage Read(BinaryReader reader)
        {
            LoginResultMessage msg;

            msg._logged = reader.ReadBoolean();
            
            return msg;
        }

        public static void Write(BinaryWriter writer, LoginResultMessage msg)
        {
            writer.Write(msg._logged);
        }

        #region IBinaryWritable Members

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }

        #endregion
    }
}
