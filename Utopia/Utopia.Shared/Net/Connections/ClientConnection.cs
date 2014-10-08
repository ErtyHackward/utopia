using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Connections
{
    /// <summary>
    /// Represents a tcp connection from server to client. Should be used on server side.
    /// </summary>
    public class ClientConnection : TcpConnection
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Properties

        /// <summary>
        /// Gets identification string of connection
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets a list of nearby clients  
        /// </summary>
        public List<ClientConnection> VisibleGroup { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client done the authorization procedure
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// Gets or sets associated user identification number
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets associated user login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets current player display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets player dynamic entity
        /// </summary>
        public ServerDynamicEntity ServerEntity { get; set; }

        public UserRole UserRole { get; set; }

        public static EntityFactory EntityFactory { get; set; }
        
        public CharacterEntity SelectedNpc { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a LoginMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginMessage>> MessageLogin;

        public void OnMessageLogin(LoginMessage ea)
        {
            var handler = MessageLogin;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<LoginMessage> { Message = ea });
        }
        
        /// <summary>
        /// Occurs when a ChatMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChatMessage>> MessageChat;

        protected void OnMessageChat(ChatMessage ea)
        {
            var handler = MessageChat;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ChatMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a GetChunksMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GetChunksMessage>> MessageGetChunks;

        protected void OnMessageGetChunks(GetChunksMessage ea)
        {
            var handler = MessageGetChunks;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<GetChunksMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a PlayerPositionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityPositionMessage>> MessagePosition;

        protected void OnMessagePosition(EntityPositionMessage ea)
        {
            var handler = MessagePosition;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityPositionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityHeadDirectionMessage>> MessageDirection;

        protected void OnMessageDirection(EntityHeadDirectionMessage ea)
        {
            var handler = MessageDirection;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityHeadDirectionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a EntityUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageEntityUse;

        protected void OnMessageEntityUse(EntityUseMessage ea)
        {
            var handler = MessageEntityUse;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
        }
        
        /// <summary>
        /// Occurs when PingMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PingMessage>> MessagePing;

        protected void OnMessagePing(PingMessage ea)
        {
            var handler = MessagePing;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<PingMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityVoxelModelMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityVoxelModelMessage>> MessageEntityVoxelModel;

        protected void OnMessageEntityVoxelModel(EntityVoxelModelMessage ea)
        {
            var handler = MessageEntityVoxelModel;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityVoxelModelMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityEquipmentMessage is received 
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityEquipmentMessage>> MessageEntityEquipment;

        protected void OnMessageEntityEquipment(EntityEquipmentMessage ea)
        {
            var handler = MessageEntityEquipment;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityEquipmentMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ItemTransferMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ItemTransferMessage>> MessageItemTransfer;

        protected void OnMessageItemTransfer(ItemTransferMessage ea)
        {
            var handler = MessageItemTransfer;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<ItemTransferMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityImpulseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityImpulseMessage>> MessageEntityImpulse;

        protected void OnMessageEntityImpulse(EntityImpulseMessage ea)
        {
            var handler = MessageEntityImpulse;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityImpulseMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityLockMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockMessage>> MessageEntityLock;

        protected void OnMessageEntityLock(EntityLockMessage ea)
        {
            var handler = MessageEntityLock;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityLockMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when RequestDateTimeSyncMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<RequestDateTimeSyncMessage>> MessageRequestDateTimeSync;

        protected void OnMessageRequestDateTimeSync(RequestDateTimeSyncMessage ea)
        {
            var handler = MessageRequestDateTimeSync;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<RequestDateTimeSyncMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityInMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityInMessage>> MessageEntityIn;

        protected void OnMessageEntityIn(EntityInMessage ea)
        {
            var handler = MessageEntityIn;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityInMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when GetEntityMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GetEntityMessage>> MessageGetEntity;

        protected void OnMessageGetEntity(GetEntityMessage ea)
        {
            var handler = MessageGetEntity;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<GetEntityMessage> { Message = ea });
        }

        public event EventHandler<ProtocolMessageEventArgs<EntityHealthMessage>> MessageEntityHealth;

        protected virtual void OnMessageEntityHealth(EntityHealthMessage e)
        {
            var handler = MessageEntityHealth;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityHealthMessage> { Message = e});
        }

        public event EventHandler<ProtocolMessageEventArgs<EntityHealthStateMessage>> MessageEntityHealthState;

        protected virtual void OnMessageEntityHealthState(EntityHealthStateMessage e)
        {
            var handler = MessageEntityHealthState;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityHealthStateMessage> { Message = e });
        }

        public event EventHandler<ProtocolMessageEventArgs<EntityAfflictionStateMessage>> MessageEntityAfflictionState;

        protected virtual void OnMessageEntityAfflictionState(EntityAfflictionStateMessage e)
        {
            var handler = MessageEntityAfflictionState;
            if (handler != null) handler(this, new ProtocolMessageEventArgs<EntityAfflictionStateMessage> { Message = e });
        }

        #endregion
        
        /// <summary>
        /// Creates new instance of ClientConnection over socket specified
        /// </summary>
        /// <param name="socket"></param>
        public ClientConnection(Socket socket) : base(socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            var endPoint = (IPEndPoint)socket.RemoteEndPoint;

            Id = string.Format("{0}:{1}", endPoint.Address, endPoint.Port);
            VisibleGroup = new List<ClientConnection>();
        }

        public void SendChat(string message)
        {
            Send(new ChatMessage
            {
                IsServerMessage = true,
                DisplayName = "server",
                Message = message
            });
        }

        protected override void OnMessage(IBinaryMessage message)
        {
            switch ((MessageTypes)message.MessageId)
            {
                case MessageTypes.Login:
                    OnMessageLogin((LoginMessage)message);
                    break;
                case MessageTypes.Chat:
                    OnMessageChat((ChatMessage)message);
                    break;
                case MessageTypes.GetChunks:
                    OnMessageGetChunks((GetChunksMessage)message);
                    break;
                case MessageTypes.EntityPosition:
                    OnMessagePosition((EntityPositionMessage)message);
                    break;
                case MessageTypes.EntityDirection:
                    OnMessageDirection((EntityHeadDirectionMessage)message);
                    break;
                case MessageTypes.EntityUse:
                    OnMessageEntityUse((EntityUseMessage)message);
                    break;
                case MessageTypes.Ping:
                    OnMessagePing((PingMessage)message);
                    break;
                case MessageTypes.EntityVoxelModel:
                    OnMessageEntityVoxelModel((EntityVoxelModelMessage)message);
                    break;
                case MessageTypes.ItemTransfer:
                    OnMessageItemTransfer((ItemTransferMessage)message);
                    break;
                case MessageTypes.EntityEquipment:
                    OnMessageEntityEquipment((EntityEquipmentMessage)message);
                    break;
                case MessageTypes.EntityImpulse:
                    OnMessageEntityImpulse((EntityImpulseMessage)message);
                    break;
                case MessageTypes.EntityLock:
                    OnMessageEntityLock((EntityLockMessage)message);
                    break;
                case MessageTypes.RequestDateTimeSync:
                    OnMessageRequestDateTimeSync((RequestDateTimeSyncMessage)message);
                    break;
                case MessageTypes.EntityIn:
                    OnMessageEntityIn((EntityInMessage)message);
                    break;
                case MessageTypes.GetEntity:
                    OnMessageGetEntity((GetEntityMessage)message);
                    break;
                case MessageTypes.EntityHealth:
                    OnMessageEntityHealth((EntityHealthMessage)message);
                    break;
                case MessageTypes.EntityHealthState:
                    OnMessageEntityHealthState((EntityHealthStateMessage)message);
                    break;
                case MessageTypes.EntityAfflictionState:
                    OnMessageEntityAfflictionState((EntityAfflictionStateMessage)message);
                    break;
                default:
                    throw new ArgumentException("Invalid message id");
            }
        }
    }
}
