using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Utopia.Server.Structs;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Server
{
    /// <summary>
    /// Represents a tcp connection from server to client. Should be used on server side.
    /// </summary>
    public class ClientConnection : TcpConnection
    {
        #region Fields

        private byte[] _tail;
        protected NetworkStream Stream;
        protected BinaryWriter Writer;

        internal readonly ConcurrentQueue<IBinaryMessage> _delayedMessages = new ConcurrentQueue<IBinaryMessage>();

        #endregion

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
        /// Gets or sets player dynamic entity
        /// </summary>
        public ServerDynamicEntity ServerEntity { get; set; }
        

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a LoginMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<LoginMessage>> MessageLogin;

        protected void OnMessageLogin(LoginMessage ea)
        {
            if (MessageLogin != null)
                MessageLogin(this, new ProtocolMessageEventArgs<LoginMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a ChatMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ChatMessage>> MessageChat;

        protected void OnMessageChat(ChatMessage ea)
        {
            if (MessageChat != null)
                MessageChat(this, new ProtocolMessageEventArgs<ChatMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a GetChunksMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<GetChunksMessage>> MessageGetChunks;

        protected void OnMessageGetChunks(GetChunksMessage ea)
        {
            if (MessageGetChunks != null)
                MessageGetChunks(this, new ProtocolMessageEventArgs<GetChunksMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a PlayerPositionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityPositionMessage>> MessagePosition;

        protected void OnMessagePosition(EntityPositionMessage ea)
        {
            if (MessagePosition != null)
                MessagePosition(this, new ProtocolMessageEventArgs<EntityPositionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a PlayerDirectionMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityDirectionMessage>> MessageDirection;

        protected void OnMessageDirection(EntityDirectionMessage ea)
        {
            if (MessageDirection != null)
                MessageDirection(this, new ProtocolMessageEventArgs<EntityDirectionMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a EntityUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageEntityUse;

        protected void OnMessageEntityUse(EntityUseMessage ea)
        {
            if (MessageEntityUse != null)
                MessageEntityUse(this, new ProtocolMessageEventArgs<EntityUseMessage> {Message = ea});
        }
        
        /// <summary>
        /// Occurs when PingMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<PingMessage>> MessagePing;

        protected void OnMessagePing(PingMessage ea)
        {
            if (MessagePing != null)
                MessagePing(this, new ProtocolMessageEventArgs<PingMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityVoxelModelMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityVoxelModelMessage>> MessageEntityVoxelModel;

        protected void OnMessageEntityVoxelModel(EntityVoxelModelMessage ea)
        {
            if (MessageEntityVoxelModel != null)
                MessageEntityVoxelModel(this, new ProtocolMessageEventArgs<EntityVoxelModelMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityEquipmentMessage is received 
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityEquipmentMessage>> MessageEntityEquipment;

        protected void OnMessageEntityEquipment(EntityEquipmentMessage ea)
        {
            if (MessageEntityEquipment != null)
                MessageEntityEquipment(this, new ProtocolMessageEventArgs<EntityEquipmentMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when ItemTransferMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<ItemTransferMessage>> MessageItemTransfer;

        protected void OnMessageItemTransfer(ItemTransferMessage ea)
        {
            if (MessageItemTransfer != null)
                MessageItemTransfer(this, new ProtocolMessageEventArgs<ItemTransferMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityImpulseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityImpulseMessage>> MessageEntityImpulse;

        protected void OnMessageEntityImpulse(EntityImpulseMessage ea)
        {
            if (MessageEntityImpulse != null)
                MessageEntityImpulse(this, new ProtocolMessageEventArgs<EntityImpulseMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when EntityLockMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockMessage>> MessageEntityLock;

        protected void OnMessageEntityLock(EntityLockMessage ea)
        {
            if (MessageEntityLock != null)
                MessageEntityLock(this, new ProtocolMessageEventArgs<EntityLockMessage> { Message = ea });
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
            Stream = new NetworkStream(socket);
            Writer = new BinaryWriter(new BufferedStream(Stream, 1024 * 32));
        }

        public void SendAsync(IBinaryMessage msg)
        {
            _delayedMessages.Enqueue(msg);
            if (!_sendThreadActive)
            {
                _sendThreadActive = true;
                new ThreadStart(SendDelayed).BeginInvoke(null, null);
            }
        }

        public void SendAsync(params IBinaryMessage[] msgs)
        {
            for (int i = 0; i < msgs.Length; i++)
            {
                _delayedMessages.Enqueue(msgs[i]);    
            }
            if (!_sendThreadActive)
            {
                _sendThreadActive = true;
                new ThreadStart(SendDelayed).BeginInvoke(null, null);
            }
        }

        private volatile bool _sendThreadActive;

        private void SendDelayed()
        {
            lock (SendSyncRoot)
            {
                start:

                _sendThreadActive = true;
                IBinaryMessage msg;
                while (_delayedMessages.TryDequeue(out msg))
                {
                    try
                    {
                        Writer.Write(msg.MessageId);
                        msg.Write(Writer);
                        
                    }
                    catch (IOException io)
                    {
                        TraceHelper.Write("Send fail... " + io.Message);
                        return;
                    }
                }
                Writer.Flush();
                // allow next thread to start
                _sendThreadActive = false;

                // we need to try get messages again because we may lose next thread start when setting _sendThreadActive = false
                if (!_delayedMessages.IsEmpty)
                {
                    goto start;
                }
            }
        }


        /// <summary>
        /// Sends a message to client
        /// </summary>
        /// <param name="msg"></param>
        protected bool Send(IBinaryMessage msg)
        {
            lock (SendSyncRoot)
            {
                try
                {
                    Writer.Write(msg.MessageId);
                    msg.Write(Writer);
                    Writer.Flush();
                    return true;
                }
                catch (IOException io)
                {
                    Console.WriteLine("Send fail... " + io.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Sends a group of messages to client
        /// </summary>
        /// <param name="messages"></param>
        protected bool Send(params IBinaryMessage[] messages)
        {
            lock (SendSyncRoot)
            {
                try
                {
                    foreach (var msg in messages)
                    {
                        Writer.Write(msg.MessageId);
                        msg.Write(Writer);
                    }
                    Writer.Flush();
                    return true;
                }
                catch (IOException io)
                {
                    TraceHelper.Write("Send fail... " + io.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Parse a raw byte array
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
                            
                            switch (idByte)
                            {
                                case MessageTypes.Login:
                                    OnMessageLogin(LoginMessage.Read(reader));
                                    break;
                                case MessageTypes.Chat:
                                    OnMessageChat(ChatMessage.Read(reader));
                                    break;
                                case MessageTypes.GetChunks:
                                    OnMessageGetChunks(GetChunksMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityPosition:
                                    OnMessagePosition(EntityPositionMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityDirection:
                                    OnMessageDirection(EntityDirectionMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityUse:
                                    OnMessageEntityUse(EntityUseMessage.Read(reader));
                                    break;
                                case MessageTypes.Ping:
                                    OnMessagePing(PingMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityVoxelModel:
                                    OnMessageEntityVoxelModel(EntityVoxelModelMessage.Read(reader));
                                    break;
                                case MessageTypes.ItemTransfer:
                                    OnMessageItemTransfer(ItemTransferMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityEquipment:
                                    OnMessageEntityEquipment(EntityEquipmentMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityImpulse:
                                    OnMessageEntityImpulse(EntityImpulseMessage.Read(reader));
                                    break;
                                case MessageTypes.EntityLock:
                                    OnMessageEntityLock(EntityLockMessage.Read(reader));
                                    break;
                                default:
                                    throw new ArgumentException("Invalid message id");
                            }
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
}
