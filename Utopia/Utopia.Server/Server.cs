using System;
using System.IO;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
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
        
        #region Properties
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

        /// <summary>
        /// Gets perfomance manager
        /// </summary>
        public PerformanceManager PerformanceManager { get; private set; }

        /// <summary>
        /// Gets command processor
        /// </summary>
        public CommandsManager CommandsManager { get; private set; }

        /// <summary>
        /// Gets current gameplay provider
        /// </summary>
        public GameplayProvider Gameplay { get; private set; }

        /// <summary>
        /// Gets chat manager
        /// </summary>
        public ChatManager ChatManager { get; private set; }

        /// <summary>
        /// Gets entity manager
        /// </summary>
        public EntityManager EntityManager { get; private set; }

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

            var settings = SettingsManager.Settings;

            Clock = new Clock(DateTime.Now, TimeSpan.FromMinutes(20));

            ConnectionManager = new ConnectionManager();
            ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            // connections
            Listener = new TcpConnectionListener(SettingsManager.Settings.ServerPort);
            Listener.IncomingConnection += ListenerIncomingConnection;

            Scheduler = new ScheduleManager(Clock);

            LandscapeManager = new LandscapeManager(this, chunksStorage, worldGenerator, settings.ChunkLiveTimeMinutes, settings.CleanUpInterval, settings.SaveInterval);

            AreaManager = new AreaManager(this);
            
            EntityFactory.Instance.SetLastId(EntityStorage.GetMaximumId());
            
            Services = new ServiceManager(this);
            
            PerformanceManager = new PerformanceManager(AreaManager);

            CommandsManager = new CommandsManager(this);

            ChatManager = new ChatManager(this);

            EntityManager = new EntityManager(this);

            // use DI
            Gameplay = new GameplayProvider(this);

        }

        private void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            // note: do not forget to remove events!
            e.Connection.MessageLogin += ConnectionMessageLogin;
        }

        private void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            // stop listening
            e.Connection.MessageLogin -= ConnectionMessageLogin;
            

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

        private ServerPlayerCharacterEntity GetNewPlayerEntity(ClientConnection clientConnection, uint entityId)
        {
            return new ServerPlayerCharacterEntity(
                clientConnection, 
                Gameplay.CreateNewPlayerCharacter(clientConnection.Login, entityId), 
                this
                );
        }

        private void ConnectionMessageLogin(object sender, ProtocolMessageEventArgs<LoginMessage> e)
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
                    playerEntity = new ServerPlayerCharacterEntity(connection, new PlayerCharacter(), this);
                    
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
                connection.SendAsync(new EntityInMessage { Entity = (Entity)playerEntity.DynamicEntity });

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
