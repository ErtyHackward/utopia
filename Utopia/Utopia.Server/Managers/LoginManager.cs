﻿using System;
using System.IO;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

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
            e.Connection.MessageClientInitialized += Connection_MessageClientInitialized; 
        }

        private void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            // stop listening
            e.Connection.MessageLogin -= ConnectionMessageLogin;


            Console.WriteLine("{0} disconnected", e.Connection.RemoteAddress);

            if (e.Connection.Authorized)
            {
                // saving the entity
                _server.EntityStorage.SaveEntity(e.Connection.ServerEntity.DynamicEntity);

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
                    playerEntity = GetNewPlayerEntity(connection, EntityFactory.Instance.GetUniqueEntityId());

                    var state = new UserState { EntityId = playerEntity.DynamicEntity.EntityId };

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

                //var gameInfo = new GameInformationMessage
                //{
                //    ChunkSize = AbstractChunk.ChunkSize,
                //    MaxViewRange = 32,
                //    WorldSeed = _server.LandscapeManager.WorldGenerator.WorldParametes.Seed,
                //    WaterLevel = _server.LandscapeManager.WorldGenerator.WorldParametes.SeaLevel
                //};
                //connection.SendAsync(gameInfo);
                //connection.SendAsync(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });
                //connection.SendAsync(new EntityInMessage { Entity = (Entity)playerEntity.DynamicEntity });

                //_server.ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} joined.", e.Message.Login), Operator = true });
                //connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Hello, {0}! Welcome to utopia! Have fun!", e.Message.Login), Operator = true });

                //// adding entity to world
                //_server.AreaManager.AddEntity(playerEntity);
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

        //Executed in the second phase of the login by the client.
        //It signal that the client is ready to fully initialized and ready to received Game data messages
        void Connection_MessageClientInitialized(object sender, ProtocolMessageEventArgs<ClientInitializedMessage> e)
        {
            var connection = (ClientConnection)sender;

            if (!connection.Authorized)
            {
                var error = new ErrorMessage
                {
                    ErrorCode = ErrorCodes.VersionMissmatch,
                    Data = Server.ServerProtocolVersion,
                    Message = "You are not authorized to connect to the server."
                };
                connection.SendAsync(error);
                connection.Disconnect();
                return;
            }

            var gameInfo = new GameInformationMessage
            {
                ChunkSize = AbstractChunk.ChunkSize,
                MaxViewRange = 32,
                WorldSeed = _server.LandscapeManager.WorldGenerator.WorldParametes.Seed,
                WaterLevel = _server.LandscapeManager.WorldGenerator.WorldParametes.SeaLevel
            };
            connection.SendAsync(gameInfo);
            connection.SendAsync(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });
            connection.SendAsync(new EntityInMessage { Entity = (Entity)connection.ServerEntity.DynamicEntity });

            _server.ConnectionManager.Broadcast(new ChatMessage { Login = "server", Message = string.Format("{0} joined.", connection.Login), Operator = true });
            connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Hello, {0}! Welcome to utopia! Have fun!", connection.Login), Operator = true });

            // adding entity to world
            _server.AreaManager.AddEntity(connection.ServerEntity);
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
