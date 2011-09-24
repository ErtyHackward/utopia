using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using S33M3Engines.Shared.Math;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Server.Events;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Server.Structs;
using Utopia.Server.Tools;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
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

        private readonly Queue<double> _updateCyclesPerfomance = new Queue<double>();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();

        // ReSharper restore NotAccessedField.Local        
        private readonly object _areaManagerSyncRoot = new object();

        //todo: remove to other class
        private CubeToolLogic _logic;

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
        
        /// <summary>
        /// Gets server clock
        /// </summary>
        public Clock Clock { get; private set; }

        #region Events

        /// <summary>
        /// Occurs when users sends a command
        /// </summary>
        public event EventHandler<PlayerCommandEventArgs> PlayerCommand;

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            var handler = PlayerCommand;
            if (handler != null) handler(this, e);
        }

        #endregion


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
            EntityFactory.Instance.EntityCreated += InstanceEntityCreated;

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

            //todo: remove to other class
            _logic = new CubeToolLogic(LandscapeManager);
            
            // async server events (saving modified chunks, unloading unused chunks)
            _cleanUpTimer = new Timer(CleanUp, null, SettingsManager.Settings.CleanUpInterval, SettingsManager.Settings.CleanUpInterval);
            _saveTimer = new Timer(SaveChunks, null, SettingsManager.Settings.SaveInterval, SettingsManager.Settings.SaveInterval);
            _entityUpdateTimer = new Timer(UpdateDynamic, null, 0, 100);
        }

        private void InstanceEntityCreated(object sender, EntityFactoryEventArgs e)
        {
            // set tool logic
            if (e.Entity is Annihilator || e.Entity is DirtAdder)
            {
                (e.Entity as Tool).ToolLogic = _logic;
            }
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

                    _updateStopwatch.Restart();
                    AreaManager.Update(state);
                    _updateStopwatch.Stop();

                    _updateCyclesPerfomance.Enqueue(_updateStopwatch.ElapsedTicks / ((double)Stopwatch.Frequency / 1000));
                    if (_updateCyclesPerfomance.Count > 10)
                        _updateCyclesPerfomance.Dequeue();


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

            var globalPos = new Vector3I[e.Locations.Length];

            e.Locations.CopyTo(globalPos, 0);
            BlockHelper.ConvertToGlobal(chunk.Position, globalPos);

            // tell entities about blocks change
            AreaManager.InvokeBlocksChanged(new BlocksChangedEventArgs { ChunkPosition = chunk.Position, BlockValues = e.Bytes, Locations = e.Locations, GlobalLocations = globalPos });
        }

        private void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            // note: do not forget to remove events!
            e.Connection.MessageLogin += ConnectionMessageLogin;
            e.Connection.MessageGetChunks += ConnectionMessageGetChunks;
            e.Connection.MessagePosition += ConnectionMessagePosition;
            e.Connection.MessageDirection += ConnectionMessageDirection;
            e.Connection.MessageChat += ConnectionMessageChat;
            e.Connection.MessagePing += ConnectionMessagePing;
            e.Connection.MessageEntityUse += Connection_MessageEntityUse;
        }

        private void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            // stop listening
            e.Connection.MessageLogin -= ConnectionMessageLogin;
            e.Connection.MessageGetChunks -= ConnectionMessageGetChunks;
            e.Connection.MessagePosition -= ConnectionMessagePosition;
            e.Connection.MessageDirection -= ConnectionMessageDirection;
            e.Connection.MessageChat -= ConnectionMessageChat;
            e.Connection.MessagePing -= ConnectionMessagePing;
            e.Connection.MessageEntityUse -= Connection_MessageEntityUse;

            Console.WriteLine("{0} disconnected", e.Connection.RemoteAddress);
            
            if (e.Connection.Authorized)
            {
                // saving the entity
                EntityStorage.SaveEntity(e.Connection.ServerEntity.DynamicEntity);

                // tell everybody that this player is gone
                AreaManager.RemoveEntity(e.Connection.ServerEntity);

                ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} has left the game.", e.Connection.ServerEntity.DynamicEntity.DisplayName), Operator = true });

                e.Connection.ServerEntity.CurrentArea = null;
            }

        }

        void ConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            var connection = (ClientConnection)sender;
            if (e.Message.Login == connection.Login)
            {
                var msg = e.Message.Message;

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                if (msg[0] == '/')
                {
                    if (msg.StartsWith("/services"))
                    {
                        SendChatMessage("Currenty active services: " + string.Join(", ", (from s in Services select s.ServiceName)));
                        return;
                    }

                    if (msg == "/uperf")
                    {
                        SendChatMessage(string.Format("Average cycle perfomance: {0} msec", Math.Round(_updateCyclesPerfomance.Average(), 2)));
                        return;
                    }

                    OnPlayerCommand(new PlayerCommandEventArgs { Connection = connection, Command = msg.Remove(0, 1) });
                    return;
                }

                ConnectionManager.Broadcast(e.Message);
            }
        }

        public void SendChatMessage(string message)
        {
            ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message= message });
        }
        
        private void Connection_MessageEntityUse(object sender, ProtocolMessageEventArgs<EntityUseMessage> e)
        {
            // incoming use message by the player
            // handling entity using (tool or just use)
            
            var connection = (ClientConnection)sender;
            connection.ServerEntity.Use(e.Message);
        }

        void ConnectionMessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            var connection = (ClientConnection)sender;
            // we need respond as fast as possible
            if (e.Message.Request)
            {
                var msg = e.Message;
                msg.Request = false;
                connection.SendAsync(msg);
            }
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.ServerEntity.DynamicEntity.EntityId)
            {
                connection.ServerEntity.DynamicEntity.Rotation = e.Message.Direction;
            }
        }

        void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.ServerEntity.DynamicEntity.EntityId)
            {
                connection.ServerEntity.DynamicEntity.Position = e.Message.Position;
            }
        }
        
        void ConnectionMessageGetChunks(object sender, ProtocolMessageEventArgs<GetChunksMessage> e)
        {
            var connection = (ClientConnection)sender;

            Console.WriteLine("GetChunks!" + e.Message.Range.Position+ " " + e.Message.Range.Size );

            try
            {
                var range = e.Message.Range;

                // list to get indicies
                var positionsList = e.Message.Positions == null ? null : new List<Vector2I>(e.Message.Positions);

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
                                connection.SendAsync(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkMd5Equal, ChunkHash = chunk.GetMd5Hash() });
                                return;
                            }
                        }
                    }
                    
                    if (chunk.PureGenerated)
                    {
                        connection.SendAsync(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkCanBeGenerated, ChunkHash = chunk.GetMd5Hash() });
                        return;
                    }

                    sendAllData:
                    // send data anyway
                    connection.SendAsync(new ChunkDataMessage
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
            var dEntity = new PlayerCharacter();
            dEntity.EntityId = entityId;
            dEntity.Position = new Vector3D(10, 128, 10);
            dEntity.CharacterName = clientConnection.Login;
            dEntity.Equipment.LeftTool = (Tool)EntityFactory.Instance.CreateEntity(EntityClassId.Annihilator);
            dEntity.Equipment.RightTool = (Tool)EntityFactory.Instance.CreateEntity(EntityClassId.DirtAdder);
            
            var serverChar = new ServerPlayerCharacterEntity(clientConnection, dEntity);
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
                    connection.SendAsync(new ErrorMessage
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
                connection.SendAsync(error);
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
                    oldConnection.SendAsync(new ErrorMessage { ErrorCode = ErrorCodes.AnotherInstanceLogged, Message = "Another instance of you was connected. You will be disconnected." });
                    oldConnection.Disconnect();
                }


                connection.Authorized = true;
                connection.UserId = loginData.UserId;
                connection.Login = e.Message.Login;

                ServerDynamicEntity playerEntity;

                #region Getting players character entity
                if (loginData.State == null)
                {
                    // create new message


                    playerEntity = GetNewPlayerEntity(connection,  EntityFactory.Instance.GetUniqueEntityId());

                    var state = new UserState { EntityId = playerEntity.DynamicEntity.EntityId };

                    UsersStorage.SetData(e.Message.Login, state.Save());
                }
                else
                {
                    var state = UserState.Load(loginData.State );
                    // load new player entity
                    playerEntity = new ServerPlayerCharacterEntity(connection, new PlayerCharacter());
                    
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
                            playerEntity.DynamicEntity.Load(reader);
                        }
                        
                    }
                }
                #endregion

                connection.ServerEntity = playerEntity;

                connection.SendAsync(new LoginResultMessage { Logged = true });
                Console.WriteLine("{1} logged as ({0}) EntityId = {2} ", e.Message.Login, connection.Id, connection.ServerEntity.DynamicEntity.EntityId);
                var gameInfo = new GameInformationMessage {
                    ChunkSize = AbstractChunk.ChunkSize, 
                    MaxViewRange = 32,
                    WorldSeed = LandscapeManager.WorldGenerator.WorldParametes.Seed,
                    WaterLevel = LandscapeManager.WorldGenerator.WorldParametes.SeaLevel
                };
                connection.SendAsync(gameInfo);
                connection.SendAsync(new DateTimeMessage { DateTime = Clock.Now, TimeFactor = Clock.TimeFactor });
                connection.SendAsync(new EntityInMessage { Entity = playerEntity.DynamicEntity });

                ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} joined.", e.Message.Login), Operator = true });
                connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Hello, {0}! Welcome to utopia! Have fun!", e.Message.Login), Operator = true });

                // adding entity to world
                AreaManager.AddEntity(playerEntity);
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

                connection.SendAsync(error, new LoginResultMessage { Logged = false });
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
