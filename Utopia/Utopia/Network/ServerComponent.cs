using System;
using Ninject;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using S33M3DXEngine.Main;
using Utopia.Shared.Net.Interfaces;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Shared.Structs;
using System.Collections.Generic;

namespace Utopia.Network
{
    /// <summary>
    /// Handles the server connection
    /// </summary>
    public class ServerComponent : GameComponent, IDebugInfo
    {
        private ServerConnection _serverConnection;
        private bool _entered;
        private ErrorMessage _lastError;
        private bool _tryingGlobal;
        private bool _needToTryLocal;
        private BlocksChangedMessageBuffer _blocksChangedMessageBuffer;

        #region Public variables/Properties
        //Initilialization received Data, should be move inside a proper class/struct !
        public IDynamicEntity Player { get; set; }
        public UtopiaTime WorldDateTime { get; set; }
        public double TimeFactor { get; set; }
        public GameInformationMessage GameInformations { get; set; }

        public string Login { get; set; }
        public string DisplayName { get; set; }

        public string Address { get; set; }

        public string LocalAddress { get; set; }
        
        public ServerConnection ServerConnection
        {
            get { return _serverConnection; }
            set {

                if (_serverConnection != value)
                {
                    if (_serverConnection != null)
                    {
                        _serverConnection.StatusChanged -= _serverConnection_StatusChanged;
                    }
                    
                    _serverConnection = value;

                    if (_serverConnection != null)
                    {
                        _serverConnection.StatusChanged += _serverConnection_StatusChanged;
                    }
                }
            }
        }

        public string LastErrorText {
            get
            {
                string text = null;

                if (_lastError != null)
                {
                    text = "Server error: " + _lastError.Message;
                }

                if (_serverConnection.LastException != null)
                {
                    text += "Exception: " + _serverConnection.LastException.Message;
                }

                return text;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Occurs when Login data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginMessage>> MessageLogin;
        /// <summary>
        /// Occurs when chunk data is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChunkDataMessage>> MessageChunkData;
        /// <summary>
        /// Occurs when ChatMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChatMessage>> MessageChat;
        /// <summary>
        /// Occurs when ErrorMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ErrorMessage>> MessageError;
        /// <summary>
        /// Occurs when BlockChnageMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<BlocksChangedMessage>> MessageBlockChange;
        /// <summary>
        /// Occurs when PlayerPositionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityPositionMessage>> MessagePosition;
        /// <summary>
        /// Occurs when PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityHeadDirectionMessage>> MessageDirection;
        /// <summary>
        /// Occurs when GameInformationMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GameInformationMessage>> MessageGameInformation;
        /// <summary>
        /// Occurs when LoginResultMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginResultMessage>> MessageLoginResult;
        /// <summary>
        /// Occurs when PlayerInMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityInMessage>> MessageEntityIn;
        /// <summary>
        /// Occurs when PlayerOutMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityOutMessage>> MessageEntityOut;
        /// <summary>
        /// Occurs when DateTimeMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<DateTimeMessage>> MessageDateTime;
        /// <summary>
        /// Occurs when EntityUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageEntityUse;
        /// <summary>
        /// Occurs when PingMessage is received (another thread)
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PingMessage>> MessagePing;
        /// <summary>
        /// Occurs when EntityVoxelModelMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityVoxelModelMessage>> MessageEntityVoxelModel;
        /// <summary>
        /// Occurs when EntityEquipmentMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityEquipmentMessage>> MessageEntityEquipment;
        /// <summary>
        /// Occurs when ItemTransferMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ItemTransferMessage>> MessageItemTransfer;
        /// <summary>
        /// Occurs when WeatherMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<WeatherMessage>> MessageWeather;
        /// <summary>
        /// Occurs when EntityImpulseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityImpulseMessage>> MessageEntityImpulse;
        /// <summary>
        /// Occurs when EntityLockMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockMessage>> MessageEntityLock;
        /// <summary>
        /// Occurs when EntityLockResultMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockResultMessage>> MessageEntityLockResult;
        /// <summary>
        /// Occurs when UseFeedbackMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<UseFeedbackMessage>> MessageUseFeedback;
        /// <summary>
        /// Occurs when EntityDataMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityDataMessage>> MessageEntityData;
        /// <summary>
        /// Occurs when the connection status has changed
        /// </summary>
        public event EventHandler<ServerConnectionStatusEventArgs> ConnectionStatusChanged;
        /// <summary>
        /// Occurs when the connection status has changed
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityHealthMessage>> MessageEntityHealth;
        /// <summary>
        /// Occurs when the connection status has changed
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityHealthStateMessage>> MessageEntityHealthState;
        /// <summary>
        /// Occurs when the connection status has changed
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityAfflictionStateMessage>> MessageEntityAfflictionState;


        #endregion

        [Inject]
        public EntityFactory EntityFactory { get; set; }

        public ServerComponent()
        {
            this.MessageGameInformation += ServerComponent_MessageGameInformation;
            _blocksChangedMessageBuffer = new BlocksChangedMessageBuffer();
        }

        public override void BeforeDispose()
        {
            if (ServerConnection != null &&
               ServerConnection.Status != TcpConnectionStatus.Disconnected &&
               ServerConnection.Status != TcpConnectionStatus.Disconnecting)
            {
                ServerConnection.Dispose();
            }

            this.MessageGameInformation -= ServerComponent_MessageGameInformation;

            if (ServerConnection != null) ServerConnection.Dispose();
        }

        #region Public Methods
        public void Disconnect()
        {
            if (ServerConnection != null && ServerConnection.Status == TcpConnectionStatus.Connected) 
                ServerConnection.Dispose();
        }

        public bool BindingServer(string address, string localAddress)
        {
            if (ServerConnection != null && ServerConnection.Status == TcpConnectionStatus.Connected) 
                ServerConnection.Dispose();

            Address = address;
            LocalAddress = localAddress;
            
            if (ServerConnection != null)
                ServerConnection.Dispose();
            ServerConnection = new ServerConnection();
            return true;
        }

        private void ParseAddress(string address, out string addr, out int port)
        {
            port = 4815;
            addr = address;
            if (address.Contains(":"))
            {
                addr = address.Substring(0, address.IndexOf(':'));
                port = int.Parse(address.Substring(address.IndexOf(':') + 1));
            }
        }

        public void ConnectToServer(string userName, string displayName, string passwordHash)
        {
            Login = userName;
            DisplayName = displayName;

            _entered = false;

            if (ServerConnection.LoggedOn)
                ServerConnection.Disconnect();
            
            ServerConnection.Login = userName;
            ServerConnection.DisplayName = displayName;
            ServerConnection.Password = passwordHash;
            ServerConnection.ClientVersion = ServerConnection.ProtocolVersion;
            ServerConnection.Register = false;

            if (ServerConnection.Status != TcpConnectionStatus.Connected)
            {
                string addr;
                int port;

                ParseAddress(Address, out addr, out port);
                _tryingGlobal = true;
                _needToTryLocal = !string.IsNullOrEmpty(LocalAddress);

                ServerConnection.Connect(addr, port);
            }
            else
            {
                ServerConnection.Authenticate();
            }
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (ServerConnection != null)
            {
                foreach (var data in ServerConnection.FetchPendingMessages())
                {
                    InvokeEventForNetworkDataReceived(data);
                }

                //Time based flush of blockChangeBuffer
                OnMessageBlockChange(_blocksChangedMessageBuffer.Flush(elapsedTime, false));
            }
        }

        public void EnterTheWorld()
        {
            if (!_entered)
            {
                _entered = true;
                ServerConnection.Send(new EntityInMessage());
            }
        }
        #endregion

        #region Events Handling
        private void ServerComponent_MessageGameInformation(object sender, ProtocolMessageEventArgs<GameInformationMessage> e)
        {
            GameInformations = e.Message;
        }
        #endregion

        #region Events Raising
        public void OnMessageLogin(LoginMessage ea)
        {
            if (MessageLogin != null) MessageLogin(this, new ProtocolMessageEventArgs<LoginMessage> { Message = ea });
        }

        protected void OnMessageChunkData(ChunkDataMessage ea)
        {
            if (MessageChunkData != null) MessageChunkData(this, new ProtocolMessageEventArgs<ChunkDataMessage> { Message = ea });
        }

        protected void OnMessageChat(ChatMessage ea)
        {
            if (MessageChat != null) MessageChat(this, new ProtocolMessageEventArgs<ChatMessage> { Message = ea });
        }

        protected void OnMessageError(ErrorMessage ea)
        {
            _lastError = ea;
            if (MessageError != null) MessageError(this, new ProtocolMessageEventArgs<ErrorMessage> { Message = ea });
        }

        protected void OnMessageBlockChange(BlocksChangedMessage ea)
        {
            if (MessageBlockChange != null && ea != null) MessageBlockChange(this, new ProtocolMessageEventArgs<BlocksChangedMessage> { Message = ea });
        }

        protected void OnMessagePosition(EntityPositionMessage ea)
        {
            if (MessagePosition != null) MessagePosition(this, new ProtocolMessageEventArgs<EntityPositionMessage> { Message = ea });
        }

        protected void OnMessageDirection(EntityHeadDirectionMessage ea)
        {
            if (MessageDirection != null) MessageDirection(this, new ProtocolMessageEventArgs<EntityHeadDirectionMessage> { Message = ea });
        }

        protected void OnMessageGameInformation(GameInformationMessage ea)
        {
            if (MessageGameInformation != null) MessageGameInformation(this, new ProtocolMessageEventArgs<GameInformationMessage> { Message = ea });
        }

        protected void OnMessageLoginResult(LoginResultMessage ea)
        {
            ServerConnection.LoggedOn = ea.Logged;
            if (MessageLoginResult != null) MessageLoginResult(this, new ProtocolMessageEventArgs<LoginResultMessage> { Message = ea });
        }

        protected void OnMessageEntityIn(EntityInMessage ea)
        {
            if (MessageEntityIn != null) MessageEntityIn(this, new ProtocolMessageEventArgs<EntityInMessage> { Message = ea });
        }

        protected void OnMessageEntityOut(EntityOutMessage ea)
        {
            if (MessageEntityOut != null) MessageEntityOut(this, new ProtocolMessageEventArgs<EntityOutMessage> { Message = ea });
        }

        protected void OnMessageDateTime(DateTimeMessage ea)
        {
            if (MessageDateTime != null) MessageDateTime(this, new ProtocolMessageEventArgs<DateTimeMessage> { Message = ea });
        }

        protected void OnMessageEntityUse(EntityUseMessage ea)
        {
            if (MessageEntityUse != null) MessageEntityUse(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
        }

        protected void OnMessagePing(PingMessage ea)
        {
            if (MessagePing != null) MessagePing(this, new ProtocolMessageEventArgs<PingMessage> { Message = ea });
        }

        protected void OnMessageEntityVoxelModel(EntityVoxelModelMessage ea)
        {
            if (MessageEntityVoxelModel != null) MessageEntityVoxelModel(this, new ProtocolMessageEventArgs<EntityVoxelModelMessage> { Message = ea });
        }

        protected void OnMessageEntityEquipment(EntityEquipmentMessage ea)
        {
            if (MessageEntityEquipment != null) MessageEntityEquipment(this, new ProtocolMessageEventArgs<EntityEquipmentMessage> { Message = ea });
        }

        protected void OnMessageItemTransfer(ItemTransferMessage ea)
        {
            if (MessageItemTransfer != null) MessageItemTransfer(this, new ProtocolMessageEventArgs<ItemTransferMessage> { Message = ea });
        }

        protected void OnMessageWeather(WeatherMessage ea)
        {
            if (MessageWeather != null) MessageWeather(this, new ProtocolMessageEventArgs<WeatherMessage> { Message = ea });
        }

        protected void OnMessageEntityImpulse(EntityImpulseMessage ea)
        {
            if (MessageEntityImpulse != null) MessageEntityImpulse(this, new ProtocolMessageEventArgs<EntityImpulseMessage> { Message = ea });
        }

        protected void OnMessageEntityLockResult(EntityLockMessage ea)
        {
            if (MessageEntityLock != null) MessageEntityLock(this, new ProtocolMessageEventArgs<EntityLockMessage> { Message = ea });
        }

        protected void OnMessageEntityLockResult(EntityLockResultMessage ea)
        {
            if (MessageEntityLockResult != null) MessageEntityLockResult(this, new ProtocolMessageEventArgs<EntityLockResultMessage> { Message = ea });
        }

        protected void OnMessageUseFeedback(UseFeedbackMessage ea)
        {
            if (MessageUseFeedback != null) MessageUseFeedback(this, new ProtocolMessageEventArgs<UseFeedbackMessage> { Message = ea });
        }

        protected virtual void OnMessageEntityData(EntityDataMessage ea)
        {
            if (MessageEntityData != null) MessageEntityData(this, new ProtocolMessageEventArgs<EntityDataMessage> { Message = ea });
        }

        protected virtual void OnConnectionStatusChanged(ServerConnectionStatusEventArgs e)
        {
            if (ConnectionStatusChanged != null) ConnectionStatusChanged(this, e);
        }

        protected void OnMessageEntityHealth(EntityHealthMessage ea)
        {
            if (MessageEntityHealth != null) MessageEntityHealth(this, new ProtocolMessageEventArgs<EntityHealthMessage>() { Message = ea });
        }

        protected void OnMessageEntityHealthState(EntityHealthStateMessage ea)
        {
            if (MessageEntityHealthState != null) MessageEntityHealthState(this, new ProtocolMessageEventArgs<EntityHealthStateMessage>() { Message = ea });
        }

        protected void OnMessageEntityAfflictionState(EntityAfflictionStateMessage ea)
        {
            if (MessageEntityAfflictionState != null) MessageEntityAfflictionState(this, new ProtocolMessageEventArgs<EntityAfflictionStateMessage>() { Message = ea });
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Invokes required event from Received network Messages
        /// </summary>
        /// <param name="msg"></param>
        private void InvokeEventForNetworkDataReceived(IBinaryMessage msg)
        {
            EntityFactory.ProcessMessage(msg);

            if (msg is ITimeStampedMsg)
            {
                ((ITimeStampedMsg)msg).MessageRecTime = DateTime.Now;
            }

            if ((MessageTypes)msg.MessageId != MessageTypes.BlockChange)
            {
                OnMessageBlockChange(_blocksChangedMessageBuffer.Flush(0, true));
            }
            
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
                    _blocksChangedMessageBuffer.Add((BlocksChangedMessage)msg);
                    break;
                case MessageTypes.EntityPosition:
                    OnMessagePosition((EntityPositionMessage)msg);
                    break;
                case MessageTypes.EntityDirection:
                    OnMessageDirection((EntityHeadDirectionMessage)msg);
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
                case MessageTypes.EntityLock:
                    OnMessageEntityLockResult((EntityLockMessage)msg);
                    break;
                case MessageTypes.EntityLockResult:
                    OnMessageEntityLockResult((EntityLockResultMessage)msg);
                    break;
                case MessageTypes.UseFeedback:
                    OnMessageUseFeedback((UseFeedbackMessage)msg);
                    break;
                case MessageTypes.EntityData:
                    OnMessageEntityData((EntityDataMessage)msg);
                    break;
                case MessageTypes.EntityHealth:
                    OnMessageEntityHealth((EntityHealthMessage)msg);
                    break;
                case MessageTypes.EntityHealthState:
                    OnMessageEntityHealthState((EntityHealthStateMessage)msg);
                    break;
                case MessageTypes.EntityAfflictionState:
                    OnMessageEntityAfflictionState((EntityAfflictionStateMessage)msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("msg", "Invalid message received from server");
            }
        }

        void _serverConnection_StatusChanged(object sender, TcpConnectionStatusEventArgs e)
        {
            bool final = true;
            if (e.Status == TcpConnectionStatus.Disconnected)
            {
                foreach (var message in ServerConnection.FetchPendingMessages())
                {
                    var error = message as ErrorMessage;
                    if (error != null)
                        _lastError = error;
                }

                if (_tryingGlobal && _needToTryLocal)
                {
                    // doing second attempt
                    string addr;
                    int port;

                    ParseAddress(LocalAddress, out addr, out port);
                    _tryingGlobal = false;
                    _needToTryLocal = false;
                    final = false;

                    ServerConnection.Connect(addr, port);
                }
            }

            if (e.Status == TcpConnectionStatus.Connected)
            {
                _needToTryLocal = false;
            }

            var ea = new ServerConnectionStatusEventArgs 
            { 
                Status = e.Status,
                Final = final
            };

            OnConnectionStatusChanged(ea);
        }
        #endregion

        #region IDebugInfo Implementation
        public bool ShowDebugInfo { get; set; }
        
        public string GetDebugInfo()
        {
            if (ShowDebugInfo)
            {
                return string.Format("Received: NotImpl Receive speed: NotImpl");
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion


    }


    public class ServerConnectionStatusEventArgs : TcpConnectionStatusEventArgs
    {
        public bool Final { get; set; }
    }
}
