using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Connections
{
    /// <summary>
    /// Represents a tcp connection from client to server. Should be used on client side.
    /// </summary>
    public class ServerConnection : TcpConnection
    {
        readonly ConcurrentQueue<IBinaryMessage> _incomingMessages = new ConcurrentQueue<IBinaryMessage>();

        /// <summary>
        /// Modify this constant to actual value
        /// </summary>
        public const int ProtocolVersion = 12;

        /// <summary>
        /// Gets or sets current client version
        /// </summary>
        public int ClientVersion { get; set; }

        /// <summary>
        /// Indicates if login procedure is succeed
        /// </summary>
        public bool LoggedOn { get; set; }

        /// <summary>
        /// Gets or sets current connection user login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets current connection user password SHA1 hash
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets if we need to register
        /// </summary>
        public bool Register { get; set; }

        /// <summary>
        /// Gets or sets current connection user identification number
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets user display message
        /// </summary>
        public string DisplayName { get; set; }

        protected override void OnStatusChanged(TcpConnectionStatusEventArgs e)
        {
            if (e.Status == TcpConnectionStatus.Disconnected || e.Status == TcpConnectionStatus.Disconnecting)
                LoggedOn = false;

            if (e.Status == TcpConnectionStatus.Connected)
                Authenticate();

            base.OnStatusChanged(e);
        }

        /// <summary>
        /// Sends Login message to the server
        /// </summary>
        public void Authenticate()
        {
            Send(new LoginMessage { 
                Login = Login, 
                DisplayName = DisplayName, 
                Password = Password, 
                Version = ClientVersion 
            });
        }

        protected override void OnMessage(IBinaryMessage msg)
        {
            _incomingMessages.Enqueue(msg);
        }

        /// <summary>
        /// Process all buffered messages in current thread
        /// </summary>
        /// <param name="messageLimit">Count of messages to process, set 0 to process all messages</param>
        public IEnumerable<IBinaryMessage> FetchPendingMessages(int messageLimit = 0)
        {
            if(messageLimit == 0)
                messageLimit = _incomingMessages.Count;

            for (int i = 0; i < messageLimit; i++)
            {
                IBinaryMessage msg;
                if (_incomingMessages.TryDequeue(out msg))
                {
                    yield return msg;
                }
            }
        }

    }

    public class BlockDataEventArgs : EventArgs
    {
        public Vector2I Position { get; set; }
        public byte[] Bytes { get; set; }
    }
}
