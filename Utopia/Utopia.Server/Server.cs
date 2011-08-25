using System;
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
using Utopia.Shared.Config;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;

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
        /// Gets main server chunk storage
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
        /// Gets main storage manager
        /// </summary>
        public SQLiteStorageManager Storage { get; private set; }

        /// <summary>
        /// Gets a servers world generator
        /// </summary>
        public WorldGenerator WorldGenerator { get; private set; }
        
        /// <summary>
        /// Create new instance of Server class
        /// </summary>
        public Server()
        {
            Chunks = new Dictionary<IntVector2, ServerChunk>();
            SettingsManager = new XmlSettingsManager<ServerSettings>("utopiaserver.config", SettingsStorage.ApplicationData);
            SettingsManager.Load();

            Listener = new TcpConnectionListener(SettingsManager.Settings.ServerPort);
            Listener.IncomingConnection += ListenerIncomingConnection;
            
            ConnectionManager = new ConnectionManager();
            ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            var dbPath = SettingsManager.Settings.DatabasePath;

            if(string.IsNullOrEmpty(dbPath))
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UtopiaServer\\world.db");
            
            Storage = new SQLiteStorageManager(dbPath);

            // todo: proper terrain generator initialize
            WorldGenerator = new WorldGenerator(new WorldParameters(), new FlatWorldProcessor());

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

                Storage.SaveBlocksData(positions, datas.ToArray());
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
            e.Connection.MessageBlockChange -= ConnectionMessageBlockChange;
            e.Connection.MessagePosition -= ConnectionMessagePosition;
            e.Connection.MessageDirection -= ConnectionMessageDirection;
            e.Connection.MessageChat -= ConnectionMessageChat;
            Console.WriteLine("{0} disconnected", e.Connection.RemoteAddress);
            
            if (e.Connection.Authorized)
            {
                ConnectionManager.Broadcast(new PlayerOutMessage { UserId = e.Connection.UserId });

                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        new PlayerPositionMessage { Position = e.Connection.Position, UserId = e.Connection.UserId }.Write(bw);
                        new PlayerDirectionMessage { Direction = e.Connection.Position, UserId = e.Connection.UserId }.Write(bw);
                        var bytes = ms.GetBuffer();

                        var b2 = new byte[ms.Position];
                        Buffer.BlockCopy(bytes, 0, b2, 0, (int)ms.Position);

                        Storage.SetData(e.Connection.Login, b2);
                    }
                }
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
            e.Connection.MessageBlockChange += ConnectionMessageBlockChange;
            e.Connection.MessagePosition += ConnectionMessagePosition;
            e.Connection.MessageDirection += ConnectionMessageDirection;
            e.Connection.MessageChat += ConnectionMessageChat;
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<PlayerDirectionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.UserId == connection.UserId)
            {
                ConnectionManager.Broadcast(e.Message);
                connection.Direction = e.Message.Direction;
            }
        }

        void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<PlayerPositionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.UserId == connection.UserId)
            {
                ConnectionManager.Broadcast(e.Message);
                connection.Position = e.Message.Position;
            }
        }

        void ConnectionMessageBlockChange(object sender, ProtocolMessageEventArgs<BlockChangeMessage> e)
        {
            var vector = e.Message.BlockPosition;
            Console.WriteLine("BlockChanged {0} {1} {2}", vector.X,vector.Y,vector.Z );

            var chunkPos = BlockHelper.BlockToChunkPosition(e.Message.BlockPosition);

            var chunk = GetChunk(chunkPos);

            var inchunkpos = BlockHelper.GlobalToInternalChunkPosition(e.Message.BlockPosition);

            if (!chunk.NeedSave)
                lock (_saveList)
                    _saveList.Add(chunk);

            chunk.BlockData.SetBlock(inchunkpos, e.Message.BlockType);

            ConnectionManager.Broadcast(e.Message);
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
                        var data = Storage.LoadBlockData(position);

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
                var positionsList = new List<IntVector2>(e.Message.Positions);

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

            if (e.Message.Register)
            {
                if (!Storage.Register(e.Message.Login, e.Message.Password, 0))
                {
                    connection.Send(new ErrorMessage
                                        {
                                            ErrorCode = ErrorCodes.LoginAlreadyRegistered,
                                            Message = "Such login already registered"
                                        });
                    return;
                }
            }

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

            LoginData? loginData;
            if (Storage.Login(e.Message.Login, e.Message.Password, out loginData))
            {
                Console.WriteLine("{1} logged as ({0}) ", e.Message.Login, connection.Id);

                connection.Authorized = true;
                connection.UserId = loginData.Value.UserId;
                connection.Login = e.Message.Login;

                connection.Send(new LoginResultMessage {Logged = true});

                var gameInfo = new GameInformationMessage { ChunkSize = new Location3<int>(16,128,16), MaxViewRange = 32 };
                connection.Send(gameInfo);


                ConnectionManager.Broadcast(new PlayerInMessage { Login = e.Message.Login, UserId = connection.UserId });

                // send initial data
                if (loginData.Value.State == null)
                {
                    ConnectionManager.Broadcast(new PlayerPositionMessage { UserId = loginData.Value.UserId, Position = new Vector3(8, 50, 8) });
                    ConnectionManager.Broadcast(new PlayerDirectionMessage { UserId = loginData.Value.UserId, Direction = new Vector3(-0.15f, -0.97f, -0.15f) });
                    connection.Position = new Vector3 { X = 8, Y = 50, Z = 8 };
                    connection.Direction = new Vector3 { X = -0.15f, Y = -0.97f, Z = -0.15f };
                }
                else
                {
                    ConnectionManager.Broadcast(loginData.Value.State);
                }


                ConnectionManager.Foreach(c =>
                {
                    if (c.UserId != connection.UserId)
                    {
                        connection.Send(new PlayerInMessage {Login = c.Login, UserId = c.UserId});
                        connection.Send(new PlayerPositionMessage { UserId = c.UserId, Position = c.Position });
                        connection.Send(new PlayerDirectionMessage { UserId = c.UserId, Direction = c.Direction });
                    }
                });

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
                //connection.Disconnect();
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
