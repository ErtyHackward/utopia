using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;
using System.Threading.Tasks;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Amib.Threading;
using Utopia.Network;
using Utopia.Net.Messages;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Storage.Structs;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        #region Private variable
        private CreateLandScapeDelegate _createLandScapeDelegate;
        private delegate object CreateLandScapeDelegate(object chunk);
        private WorldGenerator _worldGenerator;
        private Server _server;
        private bool _serverCreation;
        private Dictionary<long, ChunkDataMessage> _receivedServerChunks;
        private IChunkStorageManager _chunkStorageManager;
        #endregion

        #region Public variables/properties
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }
        #endregion

        public LandscapeManager(Server server, IChunkStorageManager chunkStorageManager)
        {
            _chunkStorageManager = chunkStorageManager;

            if (server.Connected)
            {
                _server = server;
                _serverCreation = true;
                _receivedServerChunks = new Dictionary<long, ChunkDataMessage>(1024);
                _server.ServerConnection.MessageChunkData += ServerConnection_MessageChunkData;
            }
            else
            {
                _serverCreation = false;
            }

            Initialize();
        }

        public void Dispose()
        {
            _server.ServerConnection.MessageChunkData -= ServerConnection_MessageChunkData;
        }

        #region Public methods
        //Create the landscape for the chunk
        public void CreateLandScape(VisualChunk chunk, bool Async)
        {
            if (_serverCreation == false)// Not server connected, the chunks will be auto generated
            {
                //Has the chunk already been Change ? 
                if (!CheckSinglePlayerChunkGenerated(chunk))
                {
                    CreateLandscapeFromGenerator(chunk, Async);
                }
            }
            else 
            {
                CheckServerReceivedData(chunk, Async);
            }
        }

        private bool CheckSinglePlayerChunkGenerated(VisualChunk chunk)
        {
            if (chunk.StorageRequestTicket != 0 || _chunkStorageManager.ChunkHashes.ContainsKey(chunk.ChunkID))
            {
                //The chunk is stored inside the DB ==> Request the Data !
                if (chunk.StorageRequestTicket == 0)
                {
                    chunk.StorageRequestTicket = _chunkStorageManager.RequestDataTicket_async(chunk.ChunkID);
                }
                else
                {
                    //Is the data received from DB ???
                    ChunkDataStorage data = _chunkStorageManager.Data[chunk.StorageRequestTicket];
                    if (data != null)
                    {
                        chunk.BlockData.SetBlockBytes(data.CubeData);
                        chunk.RefreshBorderChunk();
                        chunk.State = ChunkState.LandscapeCreated;
                        chunk.ThreadStatus = ThreadStatus.Idle;

                        _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                        chunk.StorageRequestTicket = 0;
                    }
                }
                
                return true;
            }
            return false;
        }

        private void CheckServerReceivedData(VisualChunk chunk, bool Async)
        {
            //Is this chunk server requested ==> Then check if the result is buffered
            if (chunk.IsServerRequested)
            {
                ChunkDataMessage message;
                //Have we receive the Server data
                if (_receivedServerChunks.TryGetValue(chunk.ChunkID, out message))
                {                   
                    chunk.IsServerRequested = false;
                    switch (message.Flag)
                    {
                        //In this case the message contains the data from the landscape !
                        case ChunkDataMessageFlag.ChunkWasModified:
                            chunk.Decompress(message.Data); //Set the data into the "Big Array"
                            _receivedServerChunks.Remove(chunk.ChunkID); //Remove the chunk from the recieved queue
                            chunk.RefreshBorderChunk();
                            chunk.State = ChunkState.LandscapeCreated;
                            chunk.ThreadStatus = ThreadStatus.Idle;
                            
                            //Save the modified chunk landscape data locally only if the local one is different from the server one
                            Md5Hash hash;
                            bool SaveChunk = true;
                            if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.ChunkID, out hash))
                            {
                                if (hash == message.ChunkHash) SaveChunk = false;
                            }

                            if (SaveChunk)
                            {
                                _chunkStorageManager.StoreData_async(new Storage.Structs.ChunkDataStorage()
                                                                        {
                                                                            ChunkId = chunk.ChunkID,
                                                                            ChunkX = chunk.ChunkPosition.X,
                                                                            ChunkZ = chunk.ChunkPosition.Y,
                                                                            Md5Hash = message.ChunkHash,
                                                                            CubeData = message.Data
                                                                        }
                                                                     );
                            }

                            if (chunk.StorageRequestTicket != 0)
                            {
                                _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                chunk.StorageRequestTicket = 0;
                            }
                            break;
                        case ChunkDataMessageFlag.ChunkCanBeGenerated:
                            CreateLandscapeFromGenerator(chunk, Async);


                            if (chunk.StorageRequestTicket != 0)
                            {
                                _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                chunk.StorageRequestTicket = 0;
                            }

                            break;
                        case ChunkDataMessageFlag.ChunkMd5Equal:
                            //Do we still have to wait for the chunk from the storage ??
                            ChunkDataStorage data = _chunkStorageManager.Data[chunk.StorageRequestTicket];
                            if (data != null)
                            {
                                //Data are present !
                                chunk.Decompress(data.CubeData); //Set the data into the "Big Array"
                                _receivedServerChunks.Remove(chunk.ChunkID); //Remove the chunk from the recieved queue
                                chunk.RefreshBorderChunk();
                                chunk.State = ChunkState.LandscapeCreated;
                                chunk.ThreadStatus = ThreadStatus.Idle;

                                if (chunk.StorageRequestTicket != 0)
                                {
                                    _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                    chunk.StorageRequestTicket = 0;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                chunk.IsServerRequested = true;
                Md5Hash hash;
                if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.ChunkID, out hash))
                {
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one.
                     chunk.StorageRequestTicket = _chunkStorageManager.RequestDataTicket_async(chunk.ChunkID);

                    //We have already in the store manager a modified version of the chunk, do the server request with these information
                    _server.ServerConnection.SendAsync(new GetChunksMessage
                    {
                        HashesCount = 1,
                        Md5Hashes = new Md5Hash[] { hash },
                        Positions = new IntVector2[] { chunk.ChunkPosition },
                        Range = new Range2(chunk.ChunkPosition, IntVector2.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
                else
                {
                    //Chunk has never been modified. Request it by the chunkposition to the server
                    _server.ServerConnection.SendAsync(new GetChunksMessage
                    {
                        Range = new Range2(chunk.ChunkPosition, IntVector2.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }


            }
        }

        #endregion

        #region Private methods
        private void Initialize()
        {
            _createLandScapeDelegate = new CreateLandScapeDelegate(createLandScape_threaded);
        }

        //New chunk Received !
        private void ServerConnection_MessageChunkData(object sender, Net.Connections.ProtocolMessageEventArgs<Net.Messages.ChunkDataMessage> e)
        {
            //Bufferize the Data here
            //Console.WriteLine("== New Chunk data receive : " + e.Message.Position.ToString());
            if(_receivedServerChunks.ContainsKey(e.Message.Position.GetID())) _receivedServerChunks.Remove(e.Message.Position.GetID());
            _receivedServerChunks.Add(e.Message.Position.GetID(), e.Message);
        }

        //TODO call from Time to Time ?? how ?? When ??
        private void ChunkBufferCleanup()
        {
            IEnumerable<long> expiredIndex = _receivedServerChunks.Where(x => DateTime.Now.Subtract(x.Value.MessageRecTime).TotalSeconds > 10).Select(x => x.Key);
            foreach (var index in expiredIndex) _receivedServerChunks.Remove(index);
        }

        //Auto generate a chunk
        private void CreateLandscapeFromGenerator(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            if (Async)
            {
                WorkQueue.DoWorkInThread(new WorkItemCallback(createLandScape_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            }
            else
            {
                _createLandScapeDelegate.Invoke(chunk);
            }

            chunk.RefreshBorderChunk();
        }

        //Create the landscape for the chunk
        private object createLandScape_threaded(object chunk)
        {
            VisualChunk visualChunk = (VisualChunk)chunk;
            GeneratedChunk generatedChunk = _worldGenerator.GetChunk(visualChunk.ChunkPosition);
            
            visualChunk.BlockData.SetBlockBytes(generatedChunk.BlockData.GetBlocksBytes());

            visualChunk.State = ChunkState.LandscapeCreated;
            visualChunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }

        #endregion
    }
}
