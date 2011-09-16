using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using S33M3Engines.Shared.Math;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
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
        


        // ReSharper disable NotAccessedField.Local
        private Timer _cleanUpTimer;
        private Timer _saveTimer;
        private Timer _entityUpdateTimer;
        // ReSharper restore NotAccessedField.Local        
        private readonly object _areaManagerSyncRoot = new object();



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
        /// Gets main users storage
        /// </summary>
        public IUsersStorage UsersStorage { get; private set; }

        /// <summary>
        /// Gets main entity storage
        /// </summary>
        public IEntityStorage EntityStorage { get; private set; }

        /// <summary>
        /// Gets entity manager
        /// </summary>
        public AreaManager AreaManager { get; private set; }

        /// <summary>
        /// Gets server game services
        /// </summary>
        public ServiceManager Services { get; private set; }

        /// <summary>
        /// Gets landscape manager
        /// </summary>
        public LandscapeManager LandscapeManager { get; private set; }

        /// <summary>
        /// Gets schedule manager for dalayed and periodic operations.
        /// </summary>
        public ScheduleManager Scheduler { get; private set; }


        public Clock Clock { get; private set; }

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
            UsersStorage = usersStorage;
            EntityStorage = entityStorage;

            AreaManager = new AreaManager();
            
            EntityFactory.Instance.SetLastId(EntityStorage.GetMaximumId());

            // connections
            Listener = new TcpConnectionListener(SettingsManager.Settings.ServerPort);
            Listener.IncomingConnection += ListenerIncomingConnection;
            
            ConnectionManager = new ConnectionManager();
            ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            Services = new ServiceManager(this);

            LandscapeManager = new LandscapeManager(chunksStorage, worldGenerator);
            LandscapeManager.ChunkLoaded += LandscapeManagerChunkLoaded;
            LandscapeManager.ChunkUnloaded += LandscapeManagerChunkUnloaded;
            
            Clock = new Clock(DateTime.Now, TimeSpan.FromMinutes(20));

            Scheduler = new ScheduleManager(Clock);
            
            // async server events (saving modified chunks, unloading unused chunks)
            _cleanUpTimer = new Timer(CleanUp, null, SettingsManager.Settings.CleanUpInterval, SettingsManager.Settings.CleanUpInterval);
            _saveTimer = new Timer(SaveChunks, null, SettingsManager.Settings.SaveInterval, SettingsManager.Settings.SaveInterval);
            _entityUpdateTimer = new Timer(UpdateDynamic, null, 0, 100);
        }

        private DateTime _lastUpdate = DateTime.MinValue;

        // update dynamic entities
        private void UpdateDynamic(object o)
        {
            if (Monitor.TryEnter(_areaManagerSyncRoot))
            {
                try
                {
                    var state = new DynamicUpdateState
                                    {
                                        ElapsedTime = _lastUpdate == DateTime.MinValue ? TimeSpan.Zero : Clock.Now - _lastUpdate,
                                        CurrentTime = Clock.Now
                                    };

                    _lastUpdate = Clock.Now;

                    //var sw = Stopwatch.StartNew();
                    AreaManager.Update(state);
                    //sw.Stop();
                    //Console.WriteLine("cycle take {0}", sw.ElapsedTicks / ((double)Stopwatch.Frequency/1000));

                }
                finally
                {
                    Monitor.Exit(_areaManagerSyncRoot);
                }
            }
            else
            {
                Console.WriteLine("Warning! Server is overloaded. Try to decrease dynamic entities count");
            }
        }

        // another thread
        private void CleanUp(object o)
        {
            LandscapeManager.CleanUp(SettingsManager.Settings.ChunkLiveTimeMinutes);
        }

        // this functions executes in other thread
        private void SaveChunks(object obj)
        {
            LandscapeManager.SaveChunks();
        }

        void LandscapeManagerChunkUnloaded(object sender, LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged -= ChunkBlocksChanged;
        }

        void LandscapeManagerChunkLoaded(object sender, LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged += ChunkBlocksChanged;
        }

        void ChunkBlocksChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            var chunk = (ServerChunk)sender;

            chunk.LastAccess = DateTime.Now;
            // tell entities about blocks change
            AreaManager.InvokeBlocksChanged(new BlocksChangedEventArgs { ChunkPosition = chunk.Position, BlockValues = e.Bytes, Locations = e.Locations });
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
                AreaManager.RemoveEntity(e.Connection.Entity);

                e.Connection.Entity.CurrentArea = null;
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
            e.Connection.MessagePing += ConnectionMessagePing;
        }

        void ConnectionMessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            var connection = (ClientConnection)sender;
            // we need respond as fast as possible
            if (e.Message.Request)
            {
                var msg = e.Message;
                msg.Request = false;
                connection.Send(msg);
            }
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
        
        void ConnectionMessageGetChunks(object sender, ProtocolMessageEventArgs<GetChunksMessage> e)
        {
            var connection = (ClientConnection)sender;

            try
            {
                var range = e.Message.Range;

                // list to get indicies
                var positionsList = e.Message.Positions == null ? null : new List<IntVector2>(e.Message.Positions);

                range.Foreach( pos => {

                    var chunk = LandscapeManager.GetChunk(pos);
                    
                    if (e.Message.Flag == GetChunksMessageFlag.AlwaysSendChunkData)
                    {
                        goto sendAllData;
                    }
                    
                    // do we have hashes from client?
                    if (e.Message.HashesCount > 0 && positionsList != null)
                    {
                        int hashIndex = positionsList.IndexOf(pos);

                        if (hashIndex != -1) 
                        {
                            if (e.Message.Md5Hashes[hashIndex] == chunk.GetMd5Hash())
                            {
                                connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkMd5Equal, ChunkHash = chunk.GetMd5Hash() });
                                return;
                            }
                        }
                    }
                    
                    if (chunk.PureGenerated)
                    {
                        connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkCanBeGenerated, ChunkHash = chunk.GetMd5Hash() });
                        return;
                    }

                    sendAllData:
                    // send data anyway
                    connection.Send(new ChunkDataMessage
                    {
                        Position = pos,
                        ChunkHash = chunk.GetMd5Hash(),
                        Flag = ChunkDataMessageFlag.ChunkWasModified,
                        Data = chunk.Compress()
                    });

                });


            }
            catch (IOException)
            {
                // client was disconnected
            }
        }

        private ServerPlayerCharacterEntity GetNewPlayerEntity(ClientConnection clientConnection, uint entityId)
        {
            var serverChar = new ServerPlayerCharacterEntity(clientConnection);
            serverChar.EntityId = entityId;
            serverChar.Position = new DVector3(10, 128, 10);
            serverChar.CharacterName = "Chuck norris";
            return serverChar;
        }

        void ConnectionMessageLogin(object sender, ProtocolMessageEventArgs<LoginMessage> e)
        {
            var connection = (ClientConnection)sender;
            
            // check if user want to register and this login is busy
            if (e.Message.Register)
            {
                if (!UsersStorage.Register(e.Message.Login, e.Message.Password, 0))
                {
                    connection.Send(new ErrorMessage
                                        {
                                            ErrorCode = ErrorCodes.LoginAlreadyRegistered,
                                            Message = "Such login is already registered"
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
            LoginData loginData;
            if (UsersStorage.Login(e.Message.Login, e.Message.Password, out loginData))
            {
                var oldConnection = ConnectionManager.Find(c => c.UserId == loginData.UserId);
                if (oldConnection != null)
                {
                    oldConnection.Send(new ErrorMessage { ErrorCode = ErrorCodes.AnotherInstanceLogged, Message = "Another instance of you was connected. You will be disconnected." });
                    oldConnection.Disconnect();
                }


                connection.Authorized = true;
                connection.UserId = loginData.UserId;
                connection.Login = e.Message.Login;

                IDynamicEntity playerEntity;

                #region Getting players character entity
                if (loginData.State == null)
                {
                    // create new message


                    playerEntity = GetNewPlayerEntity(connection,  EntityFactory.Instance.GetUniqueEntityId());

                    var state = new UserState { EntityId = playerEntity.EntityId };

                    UsersStorage.SetData(e.Message.Login, state.Save());
                }
                else
                {
                    var state = UserState.Load(loginData.State );
                    // load new player entity
                    playerEntity = new ServerPlayerCharacterEntity(connection);
                    
                    var bytes = EntityStorage.LoadEntityBytes(state.EntityId);

                    if (bytes == null)
                    {
                        Console.WriteLine("{0} entity was corrupted, creating new one...", e.Message.Login);
                        playerEntity = GetNewPlayerEntity(connection, state.EntityId);
                    }
                    else
                    {
                        using (var ms = new MemoryStream(bytes))
                        {
                            var reader = new BinaryReader(ms);
                            playerEntity.Load(reader);
                        }
                        
                    }
                }
                #endregion

                connection.Entity = playerEntity;

                connection.Send(new LoginResultMessage { Logged = true });
                Console.WriteLine("{1} logged as ({0}) EntityId = {2} ", e.Message.Login, connection.Id, connection.Entity.EntityId);
                var gameInfo = new GameInformationMessage {
                    ChunkSize = AbstractChunk.ChunkSize, 
                    MaxViewRange = 32,
                    WorldSeed = LandscapeManager.WorldGenerator.WorldParametes.Seed,
                    WaterLevel = LandscapeManager.WorldGenerator.WorldParametes.SeaLevel
                };
                connection.Send(gameInfo);
                connection.Send(new DateTimeMessage { DateTime = Clock.Now, TimeFactor = Clock.TimeFactor });
                connection.Send(new EntityInMessage { Entity = playerEntity });
                // adding entity to world
                AreaManager.AddEntity(connection.Entity);
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
            LandscapeManager.Dispose();
        }
    }
}
