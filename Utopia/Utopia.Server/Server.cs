﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SharpDX;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Server.Managers;
using Utopia.Server.Structs;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Config;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

namespace Utopia.Server
{
    /// <summary>
    /// Main Utopia server class
    /// </summary>
    public class Server : IDisposable
    {
        /// <summary>
        /// Modify this constant to actual value
        /// </summary>
        public const int ServerProtocolVersion = 1;
        
        #region fields

        private readonly List<ServerChunk> _saveList = new List<ServerChunk>();
        // ReSharper disable NotAccessedField.Local
        private Timer _cleanUpTimer;
        private Timer _saveTimer;
        // ReSharper restore NotAccessedField.Local        
        #endregion

        /// <summary>
        /// Gets main server memory chunk storage.
        /// </summary>
        public Dictionary<IntVector2, ServerChunk> Chunks { get; private set; }

        /// <summary>
        /// Gets connection listener. Allows to accept client connections
        /// </summary>
        public TcpConnectionListener Listener { get; private set; }

        /// <summary>
        /// Gets server settings manager
        /// </summary>
        public XmlSettingsManager<ServerSettings> SettingsManager { get; private set; }

        /// <summary>
        /// Gets server connection manager
        /// </summary>
        public ConnectionManager ConnectionManager { get; private set; }
        
        /// <summary>
        /// Gets main chunks storage
        /// </summary>
        public IChunksStorage ChunksStorage { get; private set; }

        /// <summary>
        /// Gets main users storage
        /// </summary>
        public IUsersStorage UsersStorage { get; private set; }

        /// <summary>
        /// Gets servers world generator
        /// </summary>
        public WorldGenerator WorldGenerator { get; private set; }

        /// <summary>
        /// Gets main entity storage
        /// </summary>
        public IEntityStorage EntityStorage { get; private set; }

        /// <summary>
        /// Gets entity manager
        /// </summary>
        public EntityManager EntityManager { get; private set; }

        /// <summary>
        /// Create new instance of the Server class
        /// </summary>
        public Server(
            XmlSettingsManager<ServerSettings> settingsManager, 
            WorldGenerator worldGenerator, 
            IUsersStorage usersStorage, 
            IChunksStorage chunksStorage, 
            IEntityStorage entityStorage
            )
        {
            // dependency injection
            SettingsManager = settingsManager;
            ChunksStorage = chunksStorage;
            UsersStorage = usersStorage;
            WorldGenerator = worldGenerator;
            EntityStorage = entityStorage;

            // memory storage for chunks
            Chunks = new Dictionary<IntVector2, ServerChunk>();

            EntityManager = new EntityManager();
            
            EntityFactory.Instance.SetLastId(EntityStorage.GetMaximumId());

            // connections
            Listener = new TcpConnectionListener(SettingsManager.Settings.ServerPort);
            Listener.IncomingConnection += ListenerIncomingConnection;
            
            ConnectionManager = new ConnectionManager();
            ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;
            
            // async server events (saving modified chunks, unloading unused chunks)
            _cleanUpTimer = new Timer(CleanUp, null, SettingsManager.Settings.CleanUpInterval, SettingsManager.Settings.CleanUpInterval);
            _saveTimer = new Timer(SaveChunks, null, SettingsManager.Settings.SaveInterval, SettingsManager.Settings.SaveInterval);
            
        }
        
        // this functions executes in other thread
        private void SaveChunks(object obj)
        {
            if (_saveList.Count == 0)
                return;

            lock (_saveList)
            {
                var positions = new IntVector2[_saveList.Count];
                var datas = new List<byte[]>(_saveList.Count);

                for (int i = 0; i < _saveList.Count; i++)
                {
                    _saveList[i].NeedSave = false;
                    positions[i] = _saveList[i].Position;
                    datas.Add(_saveList[i].CompressedBytes);
                }

                ChunksStorage.SaveChunksData(positions, datas.ToArray());
                _saveList.Clear();
            }
        }

        // this functions executes in other thread
        private void CleanUp(object obj)
        {
            var chunksToRemove = new List<ServerChunk>(); 

            lock (Chunks)
            {
                    
                chunksToRemove.AddRange(Chunks.Values.Where(chunk => chunk.LastAccess < DateTime.Now.AddMinutes(-SettingsManager.Settings.ChunkLiveTimeMinutes)));

                foreach (var chunk in chunksToRemove)
                {
                    Chunks.Remove(chunk.Position);
                }
            }
        }

        void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageLogin -= ConnectionMessageLogin;
            e.Connection.MessageGetChunks -= ConnectionMessageGetChunks;
            e.Connection.MessagePosition -= ConnectionMessagePosition;
            e.Connection.MessageDirection -= ConnectionMessageDirection;
            e.Connection.MessageChat -= ConnectionMessageChat;
            Console.WriteLine("{0} disconnected", e.Connection.RemoteAddress);
            
            if (e.Connection.Authorized)
            {
                // saving the entity
                EntityStorage.SaveEntity(e.Connection.Entity);

                // tell everybody that this player is gone
                EntityManager.RemoveEntity(e.Connection.Entity);
            }

        }

        void ConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.Login == connection.Login)
                ConnectionManager.Broadcast(e.Message);
        }

        void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageLogin += ConnectionMessageLogin;
            e.Connection.MessageGetChunks += ConnectionMessageGetChunks;
            e.Connection.MessagePosition += ConnectionMessagePosition;
            e.Connection.MessageDirection += ConnectionMessageDirection;
            e.Connection.MessageChat += ConnectionMessageChat;
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.Entity.EntityId)
            {
                connection.Entity.Rotation = e.Message.Direction;
            }
        }

        void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.Entity.EntityId)
            {
                connection.Entity.Position = e.Message.Position;
            }
        }

        /// <summary>
        /// Gets chunk. First it tries to get cached in memory value, then it checks the database, and then it generates the chunk
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public ServerChunk GetChunk(IntVector2 position)
        {
            ServerChunk chunk = null;
            // search chunk in memory or load it
            if (Chunks.ContainsKey(position))
            {
                chunk = Chunks[position];
                chunk.LastAccess = DateTime.Now;
            }
            else
            {
                lock (Chunks)
                {
                    if (!Chunks.ContainsKey(position))
                    {
                        var data = ChunksStorage.LoadChunkData(position);

                        if (data == null)
                        {
                            var generatedChunk = WorldGenerator.GetChunk(position);
                            
                            if (generatedChunk != null)
                            {
                                chunk = new ServerChunk(generatedChunk) { Position = position, LastAccess = DateTime.Now };
                            }
                        }
                        else
                        {
                            chunk = new ServerChunk { CompressedBytes = data };
                            chunk.Decompress();
                        }

                        Chunks.Add(position, chunk);
                    }
                    else chunk = Chunks[position];
                }
            }
            return chunk;
        }

        void ConnectionMessageGetChunks(object sender, ProtocolMessageEventArgs<GetChunksMessage> e)
        {
            var connection = sender as ClientConnection;

            try
            {
                var range = new Range2 { Min = e.Message.StartPosition, Max = e.Message.EndPosition };

                // list to get indicies
                var positionsList = e.Message.Positions == null ? null : new List<IntVector2>(e.Message.Positions);

                range.Foreach( pos => {

                    var chunk = GetChunk(pos);
                    
                    if (e.Message.Flag == GetChunksMessageFlag.AlwaysSendChunkData)
                    {
                        goto sendAllData;
                    }
                    
                    // do we have hashes from client?
                    if (e.Message.HashesCount > 0)
                    {
                        int hashIndex = positionsList.IndexOf(pos);

                        if (hashIndex != -1) 
                        {
                            if (e.Message.Md5Hashes[hashIndex] == chunk.GetMd5Hash())
                            {
                                connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkMd5Equal });
                                return;
                            }
                        }
                    }
                    
                    if (chunk.PureGenerated)
                    {
                        connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkCanBeGenerated, Data = chunk.GetMd5Hash().Bytes });
                        return;
                    }

                    sendAllData:
                    // send data anyway
                    connection.Send(new ChunkDataMessage
                    {
                        Position = pos,
                        Flag = ChunkDataMessageFlag.ChunkWasModified,
                        Data = chunk.CompressedBytes
                    });

                });


            }
            catch (IOException ex)
            {
                // client was disconnected
            }
        }

        void ConnectionMessageLogin(object sender, ProtocolMessageEventArgs<LoginMessage> e)
        {
            var connection = sender as ClientConnection;

            // check if user want to register and this login is busy
            if (e.Message.Register)
            {
                if (!UsersStorage.Register(e.Message.Login, e.Message.Password, 0))
                {
                    connection.Send(new ErrorMessage
                                        {
                                            ErrorCode = ErrorCodes.LoginAlreadyRegistered,
                                            Message = "Such login already registered"
                                        });
                    return;
                }
            }

            // check client version
            if (e.Message.Version != ServerProtocolVersion)
            {
                var error = new ErrorMessage { 
                    ErrorCode = ErrorCodes.VersionMissmatch, 
                    Data = ServerProtocolVersion, 
                    Message = "Wrong client version, expected " + ServerProtocolVersion 
                };
                connection.Send(error);
                connection.Disconnect();
                return;
            }

            // checking login and password
            LoginData? loginData;
            if (UsersStorage.Login(e.Message.Login, e.Message.Password, out loginData))
            {
                Console.WriteLine("{1} logged as ({0}) ", e.Message.Login, connection.Id);

                connection.Authorized = true;
                connection.UserId = loginData.Value.UserId;
                connection.Login = e.Message.Login;

                IDynamicEntity playerEntity;

                if (loginData.Value.State == null)
                {
                    // create new message
                    var serverChar= new ServerPlayerCharacterEntity(connection);

                    serverChar.CharacterName = "Chuck norris";

                    playerEntity = serverChar;

                    var state = new UserState();
                    state.EntityId = playerEntity.EntityId;

                    UsersStorage.SetData(e.Message.Login, state.Save());
                }
                else
                {
                    var state = UserState.Load(loginData.Value.State );
                    // load player entity
                    playerEntity = new ServerPlayerCharacterEntity(connection);
                    
                    var bytes = EntityStorage.LoadEntityBytes(state.EntityId);

                    using (var ms = new MemoryStream(bytes))
                    {
                        var reader = new BinaryReader(ms);
                        playerEntity.Load(reader);
                    }

                }

                connection.Entity = playerEntity;

                connection.Send(new LoginResultMessage { Logged = true });

                var gameInfo = new GameInformationMessage { ChunkSize = new Location3<int>(16, 128, 16), MaxViewRange = 32 };
                connection.Send(gameInfo);
                
                // adding entity to world
                EntityManager.AddEntity(connection.Entity);
            }
            else
            {
                var error = new ErrorMessage
                                {
                                    ErrorCode = ErrorCodes.LoginPasswordIncorrect,
                                    Message = "Wrong login/password combination"
                                };

                Console.WriteLine("Incorrect login information {0} ({1})", e.Message.Login,
                                  connection.Id);

                connection.Send(error, new LoginResultMessage { Logged = false });
            }
        }

        public void Listen()
        {
            Listener.Start();
            Console.WriteLine("Listening at {0} port", SettingsManager.Settings.ServerPort);
        }

        void ListenerIncomingConnection(object sender, IncomingConnectionEventArgs e)
        {
            var conn = new ClientConnection(e.Socket);

            Console.WriteLine("{0} connected", e.Socket.RemoteEndPoint);

            e.Handled = ConnectionManager.Add(conn);

            conn.Listen();

            if (!e.Handled)
                conn.BeginDispose();
        }

        /// <summary>
        /// Stops the server and releases all related resources
        /// </summary>
        public void Dispose()
        {
            ConnectionManager.Dispose();
            Listener.Dispose();
            Chunks.Clear();
            WorldGenerator.Dispose();
        }
    }
}
