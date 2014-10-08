using System;
using System.IO;
using System.Linq;
using ProtoBuf;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Events;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Server.Utils;
using Utopia.Shared.Structs;
using Utopia.Shared.World.PlanGenerator;

namespace Utopia.Shared.Server.Managers
{
    /// <summary>
    /// Controls server login operations (join, leaving)
    /// </summary>
    public class LoginManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _server;
        private readonly EntityFactory _factory;

        public event EventHandler<ConnectionEventArgs> PlayerLogged;

        protected virtual void OnPlayerLogged(ConnectionEventArgs e)
        {
            var handler = PlayerLogged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when new player entity needed, entity should be placed to PlayerEntity property of the EventArgs class
        /// </summary>
        public event EventHandler<NewPlayerEntityNeededEventArgs> PlayerEntityNeeded;

        private void OnPlayerEntityNeeded(NewPlayerEntityNeededEventArgs e)
        {
            var handler = PlayerEntityNeeded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ConnectionEventArgs> PlayerAuthorized;

        protected virtual void OnPlayerAuthorized(ConnectionEventArgs e)
        {
            var handler = PlayerAuthorized;
            if (handler != null) handler(this, e);
        }

        public GenerationParameters GenerationParameters { get; set; }

        public LoginManager(ServerCore server, EntityFactory factory)
        {
            _server = server;
            _factory = factory;
            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.BeforeConnectionRemoved += ConnectionManagerBeforeConnectionRemoved;

            _server.Scheduler.AddTaskPeriodic("Save entities", DateTime.Now, TimeSpan.FromSeconds(10), SaveEntities);
        }

        private void SaveEntities()
        {
            var entitiesToSave = _server.ConnectionManager.Connections().Where(c => c.ServerEntity != null && c.ServerEntity.NeedSave).Select(c => c.ServerEntity) .ToList();

            using (new PerfLimit("Entities save " + entitiesToSave.Count))
            foreach (var entity in entitiesToSave)
            {
                entity.Save();
            }
        }

        private void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            // note: do not forget to remove events!
            e.Connection.MessageLogin += ConnectionMessageLogin;
            e.Connection.MessageEntityIn += Connection_MessageEntityIn;
        }

        private void ConnectionManagerBeforeConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            // stop listening
            e.Connection.MessageLogin -= ConnectionMessageLogin;
            e.Connection.MessageEntityIn -= Connection_MessageEntityIn;

            logger.Info("{0} disconnected", e.Connection.RemoteAddress);

            if (e.Connection.Authorized && e.Connection.ServerEntity != null)
            {
                logger.Info("Saving entity Id={0} {1}", e.Connection.ServerEntity.DynamicEntity.DynamicId, e.Connection.DisplayName);

                // saving the entity
                _server.EntityStorage.SaveDynamicEntity(e.Connection.ServerEntity.DynamicEntity);

                // tell everybody that this player is gone
                _server.AreaManager.RemoveEntity(e.Connection.ServerEntity);

                _server.ConnectionManager.Broadcast(new ChatMessage { 
                    IsServerMessage = true,
                    DisplayName = "server",
                    Message = string.Format("{0} has left the game.", e.Connection.DisplayName)
                });

                e.Connection.ServerEntity.CurrentArea = null;
            }
        }

        void Connection_MessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            var connection = (ClientConnection)sender;

            _server.ConnectionManager.Broadcast(new ChatMessage 
            { 
                IsServerMessage = true, 
                DisplayName = "server",
                Message = string.Format("{0} joined.", connection.DisplayName)
            });
            connection.Send(new ChatMessage 
            { 
                IsServerMessage = true,
                DisplayName = "server",
                Message = string.Format("Hello, {0}! Welcome to utopia! Have fun!", connection.DisplayName)
            });
            connection.Send(new ChatMessage
            {
                IsServerMessage = true,
                DisplayName = "server",
                Message = string.Format("Players online: {0}", _server.ConnectionManager.Count)
            });

            if (!string.IsNullOrEmpty(_server.SettingsManager.Settings.MessageOfTheDay))
            {
                connection.SendChat("MOTD: " + _server.SettingsManager.Settings.MessageOfTheDay);
            }

            // adding entity to the world
            _server.AreaManager.AddEntity(connection.ServerEntity);

            OnPlayerLogged(new ConnectionEventArgs { Connection = connection });
        }
        
        private void ConnectionMessageLogin(object sender, ProtocolMessageEventArgs<LoginMessage> e)
        {
            var connection = (ClientConnection)sender;

            // check client version
            if (e.Message.Version != ServerConnection.ProtocolVersion)
            {
                logger.Error("Protocol version mismatch Client: {0} Server: {1}", e.Message.Version, ServerConnection.ProtocolVersion);
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.VersionMismatch,
                    Data = ServerConnection.ProtocolVersion,
                    Message = string.Format("Wrong client version ({0}) expected {1}" ,e.Message.Version, ServerConnection.ProtocolVersion)
                };
                connection.Send(error);
                connection.Disconnect();
                return;
            }

            if (string.IsNullOrEmpty(e.Message.Login) || string.IsNullOrEmpty(e.Message.Password))
            {
                logger.Error("Invalid login or password");
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.LoginPasswordIncorrect,
                    Message = "Invalid login or password"
                };
                connection.Send(error);
                connection.Disconnect();
                return;
            }

            // checking login and password
            LoginData loginData;

            var login = e.Message.Login.ToLower();

            if (_server.UsersStorage.Login(login, e.Message.Password, out loginData))
            {
                TimeSpan banTimeLeft;

                if (_server.UsersStorage.IsBanned(login, out banTimeLeft))
                {
                    var error = new ErrorMessage
                    {
                        ErrorCode = ErrorCodes.LoginPasswordIncorrect,
                        Message = "You are banned. Time left: " + banTimeLeft
                    };

                    connection.Send(error);
                    logger.Error("User banned {0} ({1})", e.Message.Login,
                                      connection.Id);
                    
                    connection.Disconnect();
                    return;
                }

                var oldConnection = _server.ConnectionManager.Find(c => c.UserId == loginData.UserId);
                if (oldConnection != null)
                {
                    logger.Info("Disconnecting previous instance");
                    oldConnection.Send(new ErrorMessage { 
                        ErrorCode = ErrorCodes.AnotherInstanceLogged, 
                        Message = "Another instance of you connected. You will be disconnected." 
                    });
                    oldConnection.Disconnect();
                }

                connection.Authorized  = true;
                connection.UserId      = loginData.UserId;
                connection.UserRole    = loginData.Role;
                connection.Login       = login;
                connection.DisplayName = e.Message.DisplayName;

                ServerPlayerCharacterEntity playerEntity;
               
                #region Getting player entity
                if (loginData.State == null)
                {
                    logger.Info("No state. Creating new entity and the state for " + e.Message.DisplayName);
                    // create new message
                    playerEntity = GetNewPlayerEntity(connection, DynamicIdHelper.GetNextUniqueId());
                    var state = new UserState { EntityId = playerEntity.DynamicEntity.DynamicId };
                    _server.UsersStorage.SetData(login, state.Save());
                }
                else
                {
                    var state = UserState.Load(loginData.State);
                    // load new player entity
                    playerEntity = new ServerPlayerCharacterEntity(connection, new PlayerCharacter(), _server);

                    var bytes = _server.EntityStorage.LoadEntityBytes(state.EntityId);

                    if (bytes == null)
                    {
                        logger.Warn("{0} entity is absent, creating new one... Id={1}", e.Message.DisplayName, state.EntityId);
                        playerEntity = GetNewPlayerEntity(connection, state.EntityId);
                    }
                    else
                    {
                        using (var ms = new MemoryStream(bytes))
                        {
                            playerEntity.DynamicEntity = Serializer.Deserialize<PlayerCharacter>(ms);
                        }

                    }
                }

                playerEntity.PlayerCharacter.IsReadOnly = loginData.Role == UserRole.Guest;
                playerEntity.PlayerCharacter.CanFly = loginData.Role == UserRole.Administrator;

                //Check playerEntity.DynamicEntity Initialisation
                var player = playerEntity.DynamicEntity as PlayerCharacter;
                if (player != null)
                {
                    if (player.Health.MaxValue == 0 || player.Health.EntityOwnerId == 0)
                    {
                        player.Health.MaxValue = 100;
                        player.Health.CurrentValue = 100;
                        player.HealthState = DynamicEntityHealthState.Normal;
                        player.DisplacementMode = EntityDisplacementModes.Walking;

                        logger.Info("PlayerCharacter was missing Health initialization + health state. EntityId : {0}", player.DynamicId);
                    }
                    if (player.Stamina.MaxValue == 0 || player.Stamina.EntityOwnerId == 0)
                    {
                        player.Stamina.MaxValue = 100;
                        player.Stamina.CurrentValue = 100;
                        logger.Info("PlayerCharacter was missing Stamina initialization. EntityId : {0}", player.DynamicId);
                    }
                    if (player.Oxygen.MaxValue == 0 || player.Oxygen.EntityOwnerId == 0)
                    {
                        player.Oxygen.MaxValue = 100;
                        player.Oxygen.CurrentValue = 100;
                        logger.Info("PlayerCharacter was missing Oxygen initialization. EntityId : {0}", player.DynamicId);
                    }
                }

                #endregion
                
                _server.EntityFactory.PrepareEntity(playerEntity.DynamicEntity);
                connection.ServerEntity = playerEntity;

                connection.Send(new LoginResultMessage { Logged = true });
                logger.Info("{1} {0} logged as {3} EntityId = {2} ", e.Message.Login, connection.Id, connection.ServerEntity.DynamicEntity.DynamicId, e.Message.DisplayName);
                var gameInfo = new GameInformationMessage
                {
                    ChunkSize = AbstractChunk.ChunkSize,
                    MaxViewRange = 32,
                    WorldParameter = _server.LandscapeManager.WorldGenerator.WorldParameters,
                    GlobalState = _server.GlobalStateManager.GlobalState,
                    AreaSize = MapArea.AreaSize
                };

                connection.Send(gameInfo);
                connection.Send(new EntityInMessage { Entity = playerEntity.DynamicEntity, Link = playerEntity.DynamicEntity.GetLink() });
                connection.Send(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });

                OnPlayerAuthorized(new ConnectionEventArgs { Connection = connection });
            }
            else
            {
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.LoginPasswordIncorrect,
                    Message = "Wrong login/password combination"
                };

                logger.Error("Incorrect login information {0} ({1})", e.Message.Login,
                                  connection.Id);

                connection.Send(error, new LoginResultMessage { Logged = false });
                connection.Disconnect();
            }
        }

        private ServerPlayerCharacterEntity GetNewPlayerEntity(ClientConnection clientConnection, uint entityId)
        {
            var eArgs = new NewPlayerEntityNeededEventArgs { Connection = clientConnection, EntityId = entityId };
            OnPlayerEntityNeeded(eArgs);

            return new ServerPlayerCharacterEntity(
                clientConnection,
                eArgs.PlayerEntity,
                _server
                );
        }
    }

    public class NewPlayerEntityNeededEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
        public uint EntityId { get; set; }
        public DynamicEntity PlayerEntity { get; set; }
    }
}
