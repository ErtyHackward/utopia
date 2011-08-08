using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Utopia.Net.Interfaces;
using Utopia.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Net.Connections
{
    /// <summary>
    /// Represents a tcp connection from client to server. Should be used on client side.
    /// </summary>
    public class ServerConnection : TcpConnection
    {
        private byte[] _tail; // used to store incomplete binary data
        protected NetworkStream stream;
        internal BinaryWriter writer;

        // async block
        private readonly AutoResetEvent _needSend = new AutoResetEvent(false);
        private readonly Queue<IBinaryMessage> _messages = new Queue<IBinaryMessage>();
        private Thread _sendThread;

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
        /// Gets or sets current connection md5 hash
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

        #region Events

        /// <summary>
        /// Occurs when chunk data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChunkDataMessage>> MessageChunkData;

        protected void OnMessageChunkData(ChunkDataMessage ea)
        {
            if (MessageChunkData != null)
                MessageChunkData(this, new ProtocolMessageEventArgs<ChunkDataMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ChatMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChatMessage>> MessageChat;

        protected void OnMessageChat(ChatMessage ea)
        {
            if (MessageChat != null)
                MessageChat(this, new ProtocolMessageEventArgs<ChatMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ErrorMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ErrorMessage>> MessageError;

        protected void OnMessageError(ErrorMessage ea)
        {
            if (MessageError != null)
                MessageError(this, new ProtocolMessageEventArgs<ErrorMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when BlockChnageMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<BlockChangeMessage>> MessageBlockChange;

        protected void OnMessageBlockChange(BlockChangeMessage ea)
        {
            if (MessageBlockChange != null)
                MessageBlockChange(this, new ProtocolMessageEventArgs<BlockChangeMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerPositionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PlayerPositionMessage>> MessagePosition;

        protected void OnMessagePosition(PlayerPositionMessage ea)
        {
            if (MessagePosition != null)
                MessagePosition(this, new ProtocolMessageEventArgs<PlayerPositionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PlayerDirectionMessage>> MessageDirection;

        protected void OnMessageDirection(PlayerDirectionMessage ea)
        {
            if (MessageDirection != null)
                MessageDirection(this, new ProtocolMessageEventArgs<PlayerDirectionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when GameInformationMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GameInformationMessage>> MessageGameInformation;

        protected void OnMessageGameInformation(GameInformationMessage ea)
        {
            if (MessageGameInformation != null)
                MessageGameInformation(this, new ProtocolMessageEventArgs<GameInformationMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when LoginResultMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginResultMessage>> MessageLoginResult;

        protected void OnMessageLoginResult(LoginResultMessage ea)
        {
            LoggedOn = ea.Logged;
            if (MessageLoginResult != null)
                MessageLoginResult(this, new ProtocolMessageEventArgs<LoginResultMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerInMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PlayerInMessage>> MessagePlayerIn;

        protected void OnMessagePlayerIn(PlayerInMessage ea)
        {
            if (MessagePlayerIn != null)
                MessagePlayerIn(this, new ProtocolMessageEventArgs<PlayerInMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerOutMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PlayerOutMessage>> MessagePlayerOut;

        protected void OnMessagePlayerOut(PlayerOutMessage ea)
        {
            if (MessagePlayerOut != null)
                MessagePlayerOut(this, new ProtocolMessageEventArgs<PlayerOutMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when DateTimeMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<DateTimeMessage>> MessageDateTime;

        protected void OnMessageDateTime(DateTimeMessage ea)
        {
            if (MessageDateTime != null)
                MessageDateTime(this, new ProtocolMessageEventArgs<DateTimeMessage> { Message = ea });
        }

        #endregion
        
        /// <summary>
        /// Creates new instance of ServerConnection using address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public ServerConnection(string address, int port) : base(address, port)
        {
            StartSendThread();
        }

        /// <summary>
        /// Creates new instance of ServerConnection using address string
        /// </summary>
        /// <param name="address"></param>
        public ServerConnection(string address)
        {
            StartSendThread();
             remoteAddress = ParseAddress(address);
        }

        private void StartSendThread()
        {
            (_sendThread = new Thread(SendValuesThread) { IsBackground = true, Name = "sender" }).Start();
        }

        private void SendValuesThread()
        {
            while (true)
            {
                _needSend.WaitOne();

                while (_messages.Count > 0)
                {
                    IBinaryMessage msg;
                    lock (_messages)
                    {
                        msg = _messages.Dequeue();
                    }
                    Send(msg);
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
            lock (_synObject)
            {
                msg.Write(writer);
                writer.Flush();
            }
        }

        protected override void SentFirstCommands()
        {
            stream = new NetworkStream(socket);
            writer = new BinaryWriter(stream);

            Authenticate();

            base.SentFirstCommands();
        }

        /// <summary>
        /// Sends Login message to the server
        /// </summary>
        public void Authenticate()
        {
            Send(new LoginMessage { MessageId = (byte)MessageTypes.Login, Login = Login, Password = Password, Register = Register, Version = ClientVersion });
        }

        /// <summary>
        /// Stops send-thread and releases all resources used
        /// </summary>
        public override void Dispose()
        {
            if (_sendThread != null && _sendThread.IsAlive)
            {
                _sendThread.Abort();
            }
            base.Dispose();
        }

        /// <summary>
        /// Process incoming binary data
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
                            var idByte = (MessageTypes)reader.ReadByte();
                            ms.Seek(-1, SeekOrigin.Current);
                            startPosition = ms.Position;
                            switch (idByte)
                            {
                                case MessageTypes.ChunkData:
                                    OnMessageChunkData(ChunkDataMessage.Read(reader));
                                    break;
                                case MessageTypes.Chat:
                                    OnMessageChat(ChatMessage.Read(reader));
                                    break;
                                case MessageTypes.BlockChange:
                                    OnMessageBlockChange(BlockChangeMessage.Read(reader));
                                    break;
                                case MessageTypes.PlayerPosition:
                                    OnMessagePosition(PlayerPositionMessage.Read(reader));
                                    break;
                                case MessageTypes.PlayerDirection:
                                    OnMessageDirection(PlayerDirectionMessage.Read(reader));
                                    break;
                                case MessageTypes.GameInformation:
                                    OnMessageGameInformation(GameInformationMessage.Read(reader));
                                    break;
                                case MessageTypes.LoginResult:
                                    OnMessageLoginResult(LoginResultMessage.Read(reader));
                                    break;
                                case MessageTypes.Error:
                                    OnMessageError(ErrorMessage.Read(reader));
                                    break;
                                case MessageTypes.PlayerIn:
                                    OnMessagePlayerIn(PlayerInMessage.Read(reader));
                                    break;
                                case MessageTypes.PlayerOut:
                                    OnMessagePlayerOut(PlayerOutMessage.Read(reader));
                                    break;
                                case MessageTypes.DateTime:
                                    OnMessageDateTime(DateTimeMessage.Read(reader));
                                    break;
                            }
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        _tail = new byte[ms.Length - startPosition];
                        System.Buffer.BlockCopy(buffer, (int)startPosition, _tail, 0, _tail.Length);
                    }
                }
            }
        }
    }

    public class BlockDataEventArgs : EventArgs
    {
        public IntVector2 Position { get; set; }
        public byte[] Bytes { get; set; }
    }
}
