using System;
using System.Collections.Concurrent;
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
        protected NetworkStream Stream;
        protected BinaryWriter Writer;

        // async block
        private readonly AutoResetEvent _needSend = new AutoResetEvent(false);
        private readonly Queue<IBinaryMessage> _messages = new Queue<IBinaryMessage>();
        private Thread _sendThread;
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
        /// Occurs when Login data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginMessage>> MessageLogin;

        protected void OnMessageLogin(LoginMessage ea)
        {
            if (MessageLogin != null)
                MessageLogin(this, new ProtocolMessageEventArgs<LoginMessage> { Message = ea });
        }

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
        public event EventHandler<ProtocolMessageEventArgs<EntityPositionMessage>> MessagePosition;

        protected void OnMessagePosition(EntityPositionMessage ea)
        {
            if (MessagePosition != null)
                MessagePosition(this, new ProtocolMessageEventArgs<EntityPositionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityDirectionMessage>> MessageDirection;

        protected void OnMessageDirection(EntityDirectionMessage ea)
        {
            if (MessageDirection != null)
                MessageDirection(this, new ProtocolMessageEventArgs<EntityDirectionMessage> { Message = ea });
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
        public event EventHandler<ProtocolMessageEventArgs<EntityInMessage>> MessageEntityIn;

        protected void OnMessageEntityIn(EntityInMessage ea)
        {
            if (MessageEntityIn != null)
                MessageEntityIn(this, new ProtocolMessageEventArgs<EntityInMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerOutMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityOutMessage>> MessageEntityOut;

        protected void OnMessageEntityOut(EntityOutMessage ea)
        {
            if (MessageEntityOut != null)
                MessageEntityOut(this, new ProtocolMessageEventArgs<EntityOutMessage> { Message = ea });
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

        /// <summary>
        /// Occurs when EntityUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageEntityUse;

        protected void OnMessageEntityUse(EntityUseMessage ea)
        {
            if (MessageEntityUse != null)
                MessageEntityUse(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
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
                Writer.Write(msg.MessageId);
                msg.Write(Writer);
                Writer.Flush();
            }
        }

        protected override void SentFirstCommands()
        {
            Stream = new NetworkStream(socket);
            Writer = new BinaryWriter(Stream);

            Authenticate();

            base.SentFirstCommands();
        }

        /// <summary>
        /// Sends Login message to the server
        /// </summary>
        public void Authenticate()
        {
            Send(new LoginMessage { Login = Login, Password = Password, Register = Register, Version = ClientVersion });
        }

        /// <summary>
        /// Stops send-thread and releases all resources used
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sendThread != null && _sendThread.IsAlive)
                {
                    _sendThread.Abort();
                }
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
        public void FetchPendingMessages(int messageLimit = 0)
        {
            if(messageLimit == 0) 
                messageLimit = _concurrentQueue.Count;

            for (int i = 0; i < messageLimit; i++)
            {
                IBinaryMessage msg;
                if (_concurrentQueue.TryDequeue(out msg))
                {
                    InvokeEvent(msg);
                }
            }
        }

        /// <summary>
        /// Invokes required event
        /// </summary>
        /// <param name="msg"></param>
        private void InvokeEvent(IBinaryMessage msg)
        {
            if (msg == null) return;

            switch ((MessageTypes)msg.MessageId)
            {
                case MessageTypes.Login:
                    OnMessageLogin((LoginMessage)msg);
                    break;
                case MessageTypes.Chat:
                    OnMessageChat((ChatMessage)msg);
                    break;
                case MessageTypes.Error:
                    OnMessageError((ErrorMessage)msg);
                    break;
                case MessageTypes.DateTime:
                    OnMessageDateTime((DateTimeMessage)msg);
                    break;
                case MessageTypes.GameInformation:
                    OnMessageGameInformation((GameInformationMessage)msg);
                    break;
                case MessageTypes.BlockChange:
                    OnMessageBlockChange((BlockChangeMessage)msg);
                    break;
                case MessageTypes.EntityPosition:
                    OnMessagePosition((EntityPositionMessage)msg);
                    break;
                case MessageTypes.EntityDirection:
                    OnMessageDirection((EntityDirectionMessage)msg);
                    break;
                case MessageTypes.ChunkData:
                    OnMessageChunkData((ChunkDataMessage)msg);
                    break;
                case MessageTypes.EntityIn:
                    OnMessageEntityIn((EntityInMessage)msg);
                    break;
                case MessageTypes.EntityOut:
                    OnMessageEntityOut((EntityOutMessage)msg);
                    break;
                case MessageTypes.LoginResult:
                    OnMessageLoginResult((LoginResultMessage)msg);
                    break;
                case MessageTypes.EntityUse:
                    OnMessageEntityUse((EntityUseMessage)msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("msg","Invalid message received from server");
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
                            var message = NetworkMessageFactory.Instance.ReadMessage(idByte, reader);
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
        public IntVector2 Position { get; set; }
        public byte[] Bytes { get; set; }
    }
}
