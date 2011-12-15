using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message used by client to log in to the server
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LoginMessage : IBinaryMessage
    {
        /// <summary>
        /// User login
        /// </summary>
        private string _login;

        /// <summary>
        /// User password md5 hash
        /// </summary>
        private string _password;

        /// <summary>
        /// User display name
        /// </summary>
        private string _displayName;

        /// <summary>
        /// True if client ask to register
        /// </summary>
        private bool _register;

        /// <summary>
        /// Client software version
        /// </summary>
        private int _version;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Login; }
        }

        /// <summary>
        /// Gets or sets a user login
        /// </summary>
        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }

        /// <summary>
        /// Gets or sets a user password md5 hash
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }


        /// <summary>
        /// Gets or sets value indicating if client is asking to register
        /// </summary>
        public bool Register
        {
            get { return _register; }
            set { _register = value; }
        }

        /// <summary>
        /// Gets or sets a client software version
        /// </summary>
        public int Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Gets or sets a user display name
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public static LoginMessage Read(BinaryReader reader)
        {
            LoginMessage msg;
            msg._login = reader.ReadString();
            msg._password = reader.ReadString();
            msg._displayName = reader.ReadString();
            msg._register = reader.ReadBoolean();
            msg._version = reader.ReadInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, LoginMessage msg)
        {
            writer.Write(msg._login);
            writer.Write(msg._password);
            writer.Write(msg._displayName);
            writer.Write(msg._register);
            writer.Write(msg._version);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
