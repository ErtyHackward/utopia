using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Connections
{
    public abstract class TcpConnection : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected TcpClient Client;
        private TcpConnectionStatus _status;
        private readonly Queue<IBinaryMessage> _messages = new Queue<IBinaryMessage>();
        private bool _sendThreadActive;

        private EndPoint _endPoint;

        public Exception LastException { get; set; }

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

        public EndPoint RemoteAddress {
            get { return _endPoint; }
        }

        public event EventHandler<TcpConnectionStatusEventArgs> StatusChanged;

        protected virtual void OnStatusChanged(TcpConnectionStatusEventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, e);
        }

        protected TcpConnection()
        {
            Client = new TcpClient
                {
                    ReceiveTimeout = 0,
                    SendTimeout = 5000,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024
                };
        }
        
        protected TcpConnection(Socket socket)
            : this()
        {
            Client.Client = socket;
            
            Status = socket.Connected ? TcpConnectionStatus.Connected : TcpConnectionStatus.Disconnected;

            if (Status == TcpConnectionStatus.Connected)
            {
                _endPoint = Client.Client.RemoteEndPoint;
            }
        }
        
        public void Connect(string address, int port)
        {
            try
            {
                LastException = null;
                Status = TcpConnectionStatus.Connecting;
                Client.Connect(address, port);
                Status = TcpConnectionStatus.Connected;
                _endPoint = Client.Client.RemoteEndPoint;
                Listen();
            }
            catch (Exception x)
            {
                LastException = x;
                Status = TcpConnectionStatus.Disconnected;
            }
        }

        public void Listen()
        {
            ThreadPool.QueueUserWorkItem(o => ReadThread());
        }

        protected abstract void OnMessage(IBinaryMessage msg);

        protected void ReadThread()
        {
            try
            {
                using (var stream = Client.GetStream())
                {
                    while (true)
                    {
                        var msg = Serializer.DeserializeWithLengthPrefix<IBinaryMessage>(stream, PrefixStyle.Fixed32);
                        if (msg == null)
                        {
                            Status = TcpConnectionStatus.Disconnected;
                            return;
                        }
                        OnMessage(msg);
                    }
                }
            }
            catch (Exception x)
            {
                LastException = x;
                logger.Error("Connection exception: {0}\n{1}", x.Message, x.StackTrace);
                Status = TcpConnectionStatus.Disconnected;
            }
        }

        public void Send(IBinaryMessage msg)
        {
            lock (_messages)
            {
                _messages.Enqueue(msg);
                if (!_sendThreadActive)
                {
                    _sendThreadActive = true;
                    ThreadPool.QueueUserWorkItem(o => SendThread());
                }
            }
        }

        public void Send(params IBinaryMessage[] msg)
        {
            lock (_messages)
            {
                foreach (var binaryMessage in msg)
                {
                    _messages.Enqueue(binaryMessage);    
                }
                
                if (!_sendThreadActive)
                {
                    _sendThreadActive = true;
                    ThreadPool.QueueUserWorkItem(o => SendThread());
                }
            }
        }

        private void SendThread()
        {
            try
            {
                var stream = Client.GetStream();
                while (true)
                {
                    IBinaryMessage msg;
                    lock (_messages)
                        msg = _messages.Dequeue();

                    Serializer.SerializeWithLengthPrefix(stream, msg, PrefixStyle.Fixed32);

                    lock (_messages)
                    {
                        if (_messages.Count == 0)
                        {
                            _sendThreadActive = false;
                            break;
                        }
                    }
                }
                stream.Flush();
            }
            catch (Exception x)
            {
                LastException = x;
                Status = TcpConnectionStatus.Disconnected;
            }
        }

        public void Disconnect()
        {
            while ((_sendThreadActive || _messages.Count > 0) && Status != TcpConnectionStatus.Disconnected)
            {
                Thread.Sleep(10);
            }

            Dispose();
        }

        public void Dispose()
        {
            Client.Close();
            LastException = null;
            Status = TcpConnectionStatus.Disconnected;
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
