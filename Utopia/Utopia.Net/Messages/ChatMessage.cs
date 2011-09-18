﻿using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Chat message is sent to exchange messages between clients, or to inform client by server
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChatMessage : IBinaryMessage
    {
        /// <summary>
        /// Login of the sender, can be null if system message
        /// </summary>
        private string _login;
        /// <summary>
        /// Actual message text
        /// </summary>
        private string _message;
        private bool _action;
        private bool _operator;

        /// <summary>
        /// Gets current message Id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Chat; }
        }

        /// <summary>
        /// Gets or sets login of the sender, can be null if system message
        /// </summary>
        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
        
        /// <summary>
        /// Indicates if message should be displayed in format: * nick message
        /// </summary>
        public bool Action
        {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        /// Indicates if user is operator
        /// </summary>
        public bool Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        /// <summary>
        /// Gets or sets actual message text
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public static ChatMessage Read(BinaryReader reader)
        {
            ChatMessage msg;

            msg._action = reader.ReadBoolean();
            msg._operator = reader.ReadBoolean();
            msg._login = reader.ReadString();
            msg._message = reader.ReadString();

            return msg;
        }

        public static void Write(BinaryWriter writer, ChatMessage msg)
        {
            writer.Write(msg._action);
            writer.Write(msg._operator);
            writer.Write(msg._login);
            writer.Write(msg._message);
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
