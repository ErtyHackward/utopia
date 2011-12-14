using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

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

        #region Events
        
        /// <summary>
        /// Occurs when Login data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginMessage>> MessageLogin;

        public void OnMessageLogin(LoginMessage ea)
        {
            var handler = MessageLogin;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<LoginMessage> { Message = ea });
        }


        /// <summary>
        /// Occurs when chunk data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChunkDataMessage>> MessageChunkData;

        protected void OnMessageChunkData(ChunkDataMessage ea)
        {
            var handler = MessageChunkData;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ChunkDataMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ChatMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChatMessage>> MessageChat;

        protected void OnMessageChat(ChatMessage ea)
        {
            var handler = MessageChat;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ChatMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ErrorMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ErrorMessage>> MessageError;

        protected void OnMessageError(ErrorMessage ea)
        {
            var handler = MessageError;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ErrorMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when BlockChnageMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<BlocksChangedMessage>> MessageBlockChange;

        protected void OnMessageBlockChange(BlocksChangedMessage ea)
        {
            var handler = MessageBlockChange;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<BlocksChangedMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerPositionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityPositionMessage>> MessagePosition;

        protected void OnMessagePosition(EntityPositionMessage ea)
        {
            var handler = MessagePosition;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityPositionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityDirectionMessage>> MessageDirection;

        protected void OnMessageDirection(EntityDirectionMessage ea)
        {
            var handler = MessageDirection;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityDirectionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when GameInformationMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GameInformationMessage>> MessageGameInformation;

        protected void OnMessageGameInformation(GameInformationMessage ea)
        {
            var handler = MessageGameInformation;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<GameInformationMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when LoginResultMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginResultMessage>> MessageLoginResult;

        protected void OnMessageLoginResult(LoginResultMessage ea)
        {
            LoggedOn = ea.Logged;
            var handler = MessageLoginResult;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<LoginResultMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerInMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityInMessage>> MessageEntityIn;

        protected void OnMessageEntityIn(EntityInMessage ea)
        {
            var handler = MessageEntityIn;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityInMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PlayerOutMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityOutMessage>> MessageEntityOut;

        protected void OnMessageEntityOut(EntityOutMessage ea)
        {
            var handler = MessageEntityOut;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityOutMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when DateTimeMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<DateTimeMessage>> MessageDateTime;

        protected void OnMessageDateTime(DateTimeMessage ea)
        {
            var handler = MessageDateTime;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<DateTimeMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageEntityUse;

        protected void OnMessageEntityUse(EntityUseMessage ea)
        {
            var handler = MessageEntityUse;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when PingMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PingMessage>> MessagePing;

        protected void OnMessagePing(PingMessage ea)
        {
            var handler = MessagePing;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<PingMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityVoxelModelMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityVoxelModelMessage>> MessageEntityVoxelModel;

        protected void OnMessageEntityVoxelModel(EntityVoxelModelMessage ea)
        {
            var handler = MessageEntityVoxelModel;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityVoxelModelMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityEquipmentMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityEquipmentMessage>> MessageEntityEquipment;

        protected void OnMessageEntityEquipment(EntityEquipmentMessage ea)
        {
            var handler = MessageEntityEquipment;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityEquipmentMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ItemTransferMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ItemTransferMessage>> MessageItemTransfer;

        protected void OnMessageItemTransfer(ItemTransferMessage ea)
        {
            var handler = MessageItemTransfer;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ItemTransferMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when WeatherMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<WeatherMessage>> MessageWeather;

        protected void OnMessageWeather(WeatherMessage ea)
        {
            var handler = MessageWeather;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<WeatherMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityImpulseMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityImpulseMessage>> MessageEntityImpulse;

        protected void OnMessageEntityImpulse(EntityImpulseMessage ea)
        {
            var handler = MessageEntityImpulse;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityImpulseMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityLockResultMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockResultMessage>> MessageEntityLockResult;

        protected void OnMessageEntityLockResult(EntityLockResultMessage ea)
        {
            var handler = MessageEntityLockResult;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityLockResultMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when UseFeedbackMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<UseFeedbackMessage>> MessageUseFeedback;

        protected void OnMessageUseFeedback(UseFeedbackMessage ea)
        {
            var handler = MessageUseFeedback;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<UseFeedbackMessage> { Message = ea });
        }

        #endregion
        
        /// <summary>
        /// Creates new instance of ServerConnection using address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public ServerConnection(string address, int port) : base(address, port)
        {
            //Set default server connection timeout
            ConnectionTimeOut = 5000; //5 secondes by default

            StartSendThread();
        }

        /// <summary>
        /// Creates new instance of ServerConnection using address string
        /// </summary>
        /// <param name="address"></param>
        public ServerConnection(string address)
        {
            ConnectionTimeOut = 5000; //5 secondes by default

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
                    OnMessageBlockChange((BlocksChangedMessage)msg);
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
                case MessageTypes.Ping:
                    OnMessagePing((PingMessage)msg);
                    break;
                case MessageTypes.EntityVoxelModel:
                    OnMessageEntityVoxelModel((EntityVoxelModelMessage)msg);
                    break;
                case MessageTypes.ItemTransfer:
                    OnMessageItemTransfer((ItemTransferMessage)msg);
                    break;
                case MessageTypes.EntityEquipment:
                    OnMessageEntityEquipment((EntityEquipmentMessage)msg);
                    break;
                case MessageTypes.Weather:
                    OnMessageWeather((WeatherMessage)msg);
                    break;
                case MessageTypes.EntityImpulse:
                    OnMessageEntityImpulse((EntityImpulseMessage)msg);
                    break;
                case MessageTypes.EntityLockResult:
                    OnMessageEntityLockResult((EntityLockResultMessage)msg);
                    break;
                case MessageTypes.UseFeedback:
                    OnMessageUseFeedback((UseFeedbackMessage)msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("msg", "Invalid message received from server");
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

                            if (idByte == MessageTypes.Ping)
                            {
                                InvokeEvent(message);
                            }
                            else _concurrentQueue.Enqueue(message);
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
