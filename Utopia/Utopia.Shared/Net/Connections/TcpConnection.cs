using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Connections
{
    public class TcpConnection
    {
        protected TcpClient Client;
        private TcpConnectionStatus _status;
        private Queue<IBinaryMessage> _messages = new Queue<IBinaryMessage>();

        public TcpConnectionStatus Status
        {
            get { return _status; }
            private set {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged(new TcpConnectionStatusEventArgs { Status = _status });
                }
            }
        }

        public event EventHandler<TcpConnectionStatusEventArgs> StatusChanged;

        protected void OnStatusChanged(TcpConnectionStatusEventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, e);
        }

        public TcpConnection()
        {
            Client = new TcpClient();

            // set defaults
            Client.ReceiveTimeout = 5000;
            Client.SendTimeout = 5000;
            Client.ReceiveBufferSize = 64 * 1024;
            Client.SendBufferSize = 64 * 1024;
        }
        
        public void Connect(string address, int port)
        {
            try
            {
                Status = TcpConnectionStatus.Connecting;
                Client.Connect(address, port);
                Status = TcpConnectionStatus.Connected;
            }
            catch (Exception x)
            {
                Status = TcpConnectionStatus.Disconnected;
            }
        }

        public void Send(IBinaryMessage msg)
        {
            lock (_messages)
            {
                _messages.Enqueue(msg);
            }
        }

        private void SendThread()
        {
            while (true)
            {
                IBinaryMessage msg;
                lock (_messages)
                    msg = _messages.Dequeue();

                Serializer.SerializeWithLengthPrefix(Client.GetStream()

            }
        }

        public TcpConnection(Socket socket)
            : this()
        {
            Client.Client = socket;
            Status = socket.Connected ? TcpConnectionStatus.Connected : TcpConnectionStatus.Disconnected;
        }

    }

    public class TcpConnectionStatusEventArgs : EventArgs
    {
        public TcpConnectionStatus Status { get; set; }
    }

    public enum TcpConnectionStatus
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected
    }
}
