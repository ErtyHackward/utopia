using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SharpDX;
using Utopia.Net.Interfaces;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Net.Connections
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
        public IDynamicEntity Entity { get; set; }

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
        /// Occurs when a BlockChageMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<BlockChangeMessage>> MessageBlockChange;

        protected void OnMessageBlockChange(BlockChangeMessage ea)
        {
            if (MessageBlockChange != null)
                MessageBlockChange(this, new ProtocolMessageEventArgs<BlockChangeMessage> { Message = ea });
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
                MessageEntityUse(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
        }

        /// <summary>
        /// Occurs when a ToolUseMessage is received
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityUseMessage>> MessageToolUse;

        protected void OnMessageToolUse(EntityUseMessage ea)
        {
            if (MessageToolUse != null)
                MessageToolUse(this, new ProtocolMessageEventArgs<EntityUseMessage> { Message = ea });
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
            Writer = new BinaryWriter(new BufferedStream(Stream, 1024 * 18));
        }
        
        /// <summary>
        /// Sends a message to client
        /// </summary>
        /// <param name="msg"></param>
        public void Send(IBinaryMessage msg)
        {
            try
            {
                Writer.Write(msg.MessageId);
                msg.Write(Writer);
                Writer.Flush();
            }
            catch (IOException)
            {

            }
        }

        /// <summary>
        /// Sends a group of messages to client
        /// </summary>
        /// <param name="messages"></param>
        public void Send(params IBinaryMessage[] messages)
        {
            foreach (var msg in messages)
            {
                Writer.Write(msg.MessageId);
                msg.Write(Writer);
            }
            Writer.Flush();
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
                                case MessageTypes.BlockChange:
                                    OnMessageBlockChange(BlockChangeMessage.Read(reader));
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
