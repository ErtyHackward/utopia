using System;
using System.IO;
using Utopia.Server.Structs;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World.PlanGenerator;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Controls server login operations (join, leaving)
    /// </summary>
    public class LoginManager
    {
        private readonly Server _server;

        /// <summary>
        /// Occurs when new player entity needed, entity should be placed to PlayerEntity property of the EventArgs class
        /// </summary>
        public event EventHandler<NewPlayerEntityNeededEventArgs> PlayerEntityNeeded;

        private void OnPlayerEntityNeeded(NewPlayerEntityNeededEventArgs e)
        {
            var handler = PlayerEntityNeeded;
            if (handler != null) handler(this, e);
        }

        public GenerationParameters GenerationParameters { get; set; }

        public LoginManager(Server server)
        {
            _server = server;
            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;
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


            TraceHelper.Write("{0} disconnected", e.Connection.RemoteAddress);

            if (e.Connection.Authorized)
            {
                // saving the entity
                _server.EntityStorage.SaveDynamicEntity(e.Connection.ServerEntity.DynamicEntity);

                // tell everybody that this player is gone
                _server.AreaManager.RemoveEntity(e.Connection.ServerEntity);

                _server.ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} has left the game.", e.Connection.ServerEntity.DynamicEntity.DisplayName), Operator = true });

                e.Connection.ServerEntity.CurrentArea = null;
            }
        }
        
        private void ConnectionMessageLogin(object sender, ProtocolMessageEventArgs<LoginMessage> e)
        {
            var connection = (ClientConnection)sender;

            // check if user want to register and this login is busy
            if (e.Message.Register)
            {
                if (!_server.UsersStorage.Register(e.Message.Login, e.Message.Password, 0))
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
            if (e.Message.Version != Server.ServerProtocolVersion)
            {
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.VersionMissmatch,
                    Data = Server.ServerProtocolVersion,
                    Message = "Wrong client version, expected " + Server.ServerProtocolVersion
                };
                connection.SendAsync(error);
                connection.Disconnect();
                return;
            }

            // checking login and password
            LoginData loginData;
            if (_server.UsersStorage.Login(e.Message.Login, e.Message.Password, out loginData))
            {
                var oldConnection = _server.ConnectionManager.Find(c => c.UserId == loginData.UserId);
                if (oldConnection != null)
                {
                    oldConnection.SendAsync(new ErrorMessage { ErrorCode = ErrorCodes.AnotherInstanceLogged, Message = "Another instance of you connected. You will be disconnected." });
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
                    playerEntity = GetNewPlayerEntity(connection, DynamicIdHelper.GetNextUniqueId());

                    var state = new UserState { EntityId = playerEntity.DynamicEntity.DynamicId };

                    _server.UsersStorage.SetData(e.Message.Login, state.Save());
                }
                else
                {
                    var state = UserState.Load(loginData.State);
                    // load new player entity
                    playerEntity = new ServerPlayerCharacterEntity(connection, new PlayerCharacter(), _server);

                    var bytes = _server.EntityStorage.LoadEntityBytes(state.EntityId);

                    if (bytes == null)
                    {
                        TraceHelper.Write("{0} entity was corrupted, creating new one...", e.Message.Login);
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
                TraceHelper.Write("{1} logged as ({0}) EntityId = {2} ", e.Message.Login, connection.Id, connection.ServerEntity.DynamicEntity.DynamicId);
                var gameInfo = new GameInformationMessage
                {
                    ChunkSize = AbstractChunk.ChunkSize,
                    MaxViewRange = 32,
                    WorldSeed = _server.LandscapeManager.WorldGenerator.WorldParametes.Seed,
                    WaterLevel = _server.LandscapeManager.WorldGenerator.WorldParametes.SeaLevel,
                    PlanGenerationParameters = GenerationParameters
                };
                connection.SendAsync(gameInfo);
                connection.SendAsync(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });
                connection.SendAsync(new EntityInMessage { Entity = (Entity)playerEntity.DynamicEntity });

                _server.ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} joined.", e.Message.Login), Operator = true });
                connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Hello, {0}! Welcome to utopia! Have fun!", e.Message.Login), Operator = true });

                // adding entity to world
                _server.AreaManager.AddEntity(playerEntity);
            }
            else
            {
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.LoginPasswordIncorrect,
                    Message = "Wrong login/password combination"
                };

                TraceHelper.Write("Incorrect login information {0} ({1})", e.Message.Login,
                                  connection.Id);

                connection.SendAsync(error, new LoginResultMessage { Logged = false });
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
