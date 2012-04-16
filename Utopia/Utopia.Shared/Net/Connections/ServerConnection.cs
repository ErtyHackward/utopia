using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Net.Connections
{
    /// <summary>
    /// Represents a tcp connection from client to server. Should be used on client side.
    /// </summary>
    public class ServerConnection : TcpConnection
    {
        private byte[] _tail; // used to store incomplete binary data
        protected NetworkStream Stream;
        protected BinaryWriter Writer;

        // async block
        private bool _isrunning = true;
        private readonly NetworkMessageFactory _networkMessageFactory;
        private readonly AutoResetEvent _needSend = new AutoResetEvent(false);
        private readonly Queue<IBinaryMessage> _messages = new Queue<IBinaryMessage>();
// ReSharper disable NotAccessedField.Local
        private Thread _sendThread;
// ReSharper restore NotAccessedField.Local
        private readonly ConcurrentQueue<IBinaryMessage> _concurrentQueue = new ConcurrentQueue<IBinaryMessage>();

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
               
        /// <summary>
        /// Creates new instance of ServerConnection using address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public ServerConnection(string address, int port, NetworkMessageFactory networkMessageFactory) : base(address, port)
        {
            //Set default server connection timeout
            ConnectionTimeOut = 5000; //5 secondes by default
            _networkMessageFactory = networkMessageFactory;

            StartSendThread();
        }

        /// <summary>
        /// Creates new instance of ServerConnection using address string
        /// </summary>
        /// <param name="address"></param>
        public ServerConnection(string address, NetworkMessageFactory networkMessageFactory)
        {
            ConnectionTimeOut = 5000; //5 secondes by default
            _networkMessageFactory = networkMessageFactory;

            StartSendThread();
            remoteAddress = ParseAddress(address);

            ConnectionStatusChanged += ServerConnectionConnectionStatusChanged;
        }

        void ServerConnectionConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatus.Disconnected || e.Status == ConnectionStatus.Disconnecting)
            {
                LoggedOn = false;
            }

        }

        private void StartSendThread()
        {
            (_sendThread = new Thread(SendValuesThread) { IsBackground = true, Name = "sender" }).Start();
        }

        private void SendValuesThread()
        {
            while (_isrunning)
            {
                _needSend.WaitOne();
                try
                {
                    lock (_sendSynObject)
                    {
                        while (_messages.Count > 0)
                        {
                            IBinaryMessage msg;
                            lock (_messages)
                            {
                                msg = _messages.Dequeue();
                            }

                            Writer.Write(msg.MessageId);
                            msg.Write(Writer);
                        }

                        Writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = ex });
                }
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns

        /// <summary>
        /// Enqueues message to be sent in other thread. Lagless way to send something
        /// </summary>
        /// <param name="msg"></param>
        public void SendAsync(IBinaryMessage msg)
        {
            lock (_messages)
                _messages.Enqueue(msg);
            _needSend.Set();
        }

        /// <summary>
        /// Sends some message to server
        /// </summary>
        /// <param name="msg"></param>
        public void Send(IBinaryMessage msg)
        {
            lock (_sendSynObject)
            {
                try
                {
                    if (socket.Connected)
                    {
                        Writer.Write(msg.MessageId);
                        msg.Write(Writer);
                        Writer.Flush();
                    }
                    else
                    {
                        SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Reason =  DisconnectReason.Unknown });
                    }
                }
                catch (Exception ex)
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = ex });
                }
            }
        }
        
        protected override void SentFirstCommands()
        {
            Stream = new NetworkStream(socket);
            Writer = new BinaryWriter(new BufferedStream(Stream, 32 * 1024));

            Authenticate();

            base.SentFirstCommands();
        }

        /// <summary>
        /// Sends Login message to the server
        /// </summary>
        public void Authenticate()
        {
            Send(new LoginMessage { Login = Login, DisplayName = DisplayName, Password = Password, Register = Register, Version = ClientVersion });
        }

        /// <summary>
        /// Stops send-thread and releases all resources used
        /// </summary>
        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                _isrunning = false;
                _needSend.Dispose();
                if(Writer != null)
                    Writer.Dispose();
                if(Stream != null)
                    Stream.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Process all buffered messages in current thread
        /// </summary>
        /// <param name="messageLimit">Count of messages to process, set 0 to process all messages</param>
        public IEnumerable<IBinaryMessage> FetchPendingMessages(int messageLimit = 0)
        {
            if(messageLimit == 0) 
                messageLimit = _concurrentQueue.Count;

            for (int i = 0; i < messageLimit; i++)
            {
                IBinaryMessage msg;
                if (_concurrentQueue.TryDequeue(out msg))
                {
                    yield return msg;
                }
            }
        }

        /// <summary>
        /// Process incoming binary data and puts it into the internal buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        public override void ParseRaw(byte[] buffer, int length)
        {
            if (_tail != null)
            {
                var nBuffer = new byte[_tail.Length + length];
                System.Buffer.BlockCopy(_tail, 0, nBuffer, 0, _tail.Length);
                System.Buffer.BlockCopy(buffer, 0, nBuffer, _tail.Length, length);

                buffer = nBuffer;
                length = nBuffer.Length;
                _tail = null;
            }

            using (var ms = new MemoryStream(buffer, 0, length))
            {
                using (var reader = new BinaryReader(ms))
                {
                    long startPosition = 0;
                    try
                    {
                        while (ms.Position != ms.Length)
                        {
                            startPosition = ms.Position;
                            var idByte = (MessageTypes)reader.ReadByte();

                            // if we need some message not to go into the buffer then it should be done here using InvokeEvent()
                            
                            // using Factory here makes additional box\unbox operation to pass strucutre by interface
                            // need to profile in real conditions
                            var message = _networkMessageFactory.ReadMessage(idByte, reader);

                            _concurrentQueue.Enqueue(message);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        // we need to save tail data to use it with next tcp pocket
                        _tail = new byte[ms.Length - startPosition];
                        System.Buffer.BlockCopy(buffer, (int)startPosition, _tail, 0, _tail.Length);
                    }
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
