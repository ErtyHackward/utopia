using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Entities;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Amib.Threading;
using Utopia.Network;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Storage.Structs;
using Ninject;
using Utopia.Entities.Sprites;
using S33M3CoreComponents.Timers;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private CreateLandScapeDelegate _createLandScapeDelegate;
        private delegate void CreateLandScapeDelegate(VisualChunk chunk);
        private WorldGenerator _worldGenerator;
        private ServerComponent _server;
        private Dictionary<long, ChunkDataMessage> _receivedServerChunks;
        private IChunkStorageManager _chunkStorageManager;
        private TimerManager.GameTimer _timer;
        #endregion


        [Inject] // ==> This property will be automatically SET by NInject with binded WorldGenerator singleton !
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }

        public EntityFactory EntityFactory { get; set; }

        public LandscapeManager(ServerComponent server, IChunkStorageManager chunkStorageManager, TimerManager timerManager)
        {
            _chunkStorageManager = chunkStorageManager;

            _server = server;
            _receivedServerChunks = new Dictionary<long, ChunkDataMessage>(1024);
            _server.ServerConnection.MessageChunkData += ServerConnection_MessageChunkData;
            

            //Add a new Timer trigger
            _timer = timerManager.AddTimer(0, 10000);
            _timer.OnTimerRaised += _timer_OnTimerRaised;

            Initialize();
        }

        public void Dispose()
        {
            _timer.OnTimerRaised -= _timer_OnTimerRaised;
            _server.ServerConnection.MessageChunkData -= ServerConnection_MessageChunkData;
        }

        #region Public methods
        //Create the landscape for the chunk
        public void CreateLandScape(VisualChunk chunk, bool Async)
        {
            CheckServerReceivedData(chunk, Async);
        }

        #endregion

        #region Private methods
        private void Initialize()
        {
            _createLandScapeDelegate = new CreateLandScapeDelegate(createLandScape_threaded);
        }

        //New chunk Received !
        private void ServerConnection_MessageChunkData(object sender, ProtocolMessageEventArgs<ChunkDataMessage> e)
        {
#if DEBUG
            logger.Trace("Chunk received from server id : {0}; Position : {1}", e.Message.Position.GetID(), e.Message.Position);
#endif

            //Bufferize the Data here
            if(_receivedServerChunks.ContainsKey(e.Message.Position.GetID())) _receivedServerChunks.Remove(e.Message.Position.GetID());
            _receivedServerChunks.Add(e.Message.Position.GetID(), e.Message);
        }

        //Perform Maintenance task every 10 seconds
        void _timer_OnTimerRaised()
        {
            ChunkBufferCleanup();
        }

        private void ChunkBufferCleanup()
        {
            var expiredIndex = _receivedServerChunks.Where(x => DateTime.Now.Subtract(x.Value.MessageRecTime).TotalSeconds > 60).ToList();

            for (int i = 0; i < expiredIndex.Count; i++)
            {
                _receivedServerChunks.Remove(expiredIndex[i].Key);
            }
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

                    switch (message.Flag)
                    {
                        //In this case the message contains the data from the landscape !
                        case ChunkDataMessageFlag.ChunkWasModified:
                            
                            chunk.Decompress(EntityFactory, message.Data); //Set the data into the "Big Array"
                            _receivedServerChunks.Remove(chunk.ChunkID); //Remove the chunk from the recieved queue
                            chunk.RefreshBorderChunk();
                            chunk.State = ChunkState.LandscapeCreated;
                            //chunk.CompressedBytes = message.Data;
                            chunk.ThreadStatus = ThreadStatus.Idle;

                            CreateVisualEntities(chunk, chunk);

                            //Save the modified chunk landscape data locally only if the local one is different from the server one
                            Md5Hash hash;
                            bool saveChunk = true;
                            if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.ChunkID, out hash))
                            {
                                if (hash == message.ChunkHash) saveChunk = false;
                            }

                            if (saveChunk)
                            {
                                _chunkStorageManager.StoreData_async(new ChunkDataStorage
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


                            chunk.IsServerRequested = false;


                            break;
                        case ChunkDataMessageFlag.ChunkCanBeGenerated:
                            CreateLandscapeFromGenerator(chunk, Async);

                            if (chunk.StorageRequestTicket != 0)
                            {
                                _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                chunk.StorageRequestTicket = 0;
                            }

                            chunk.IsServerRequested = false;

                            break;
                        case ChunkDataMessageFlag.ChunkMd5Equal:
                            //Do we still have to wait for the chunk from the local storage ??
                            ChunkDataStorage data = _chunkStorageManager.Data[chunk.StorageRequestTicket];
                            if (data != null)
                            {
                                //Data are present !
                                chunk.Decompress(EntityFactory, data.CubeData); //Set the data into the "Big Array"
                                _receivedServerChunks.Remove(chunk.ChunkID); //Remove the chunk from the recieved queue
                                chunk.RefreshBorderChunk();
                                chunk.State = ChunkState.LandscapeCreated;
                                chunk.ThreadStatus = ThreadStatus.Idle;

                                CreateVisualEntities(chunk, chunk);

                                if (chunk.StorageRequestTicket != 0)
                                {
                                    _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                    chunk.StorageRequestTicket = 0;
                                }

                                chunk.IsServerRequested = false;

                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {

#if DEBUG
                logger.Trace("Chunk request to server : " + chunk.ChunkID);
#endif

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
                        Positions = new Vector2I[] { chunk.ChunkPosition },
                        Range = new Range2(chunk.ChunkPosition, Vector2I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
                else
                {
                    //Chunk has never been modified. Request it by the chunkposition to the server
                    _server.ServerConnection.SendAsync(new GetChunksMessage
                    {
                        Range = new Range2(chunk.ChunkPosition, Vector2I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
            }
        }


        //Auto generate a chunk
        private void CreateLandscapeFromGenerator(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            if (Async)
            {
                chunk.ThreadStatus = ThreadStatus.Locked;
                SmartThread.ThreadPool.QueueWorkItem(createLandScape_threaded, chunk, chunk.ThreadPriority);
            }
            else
            {
                _createLandScapeDelegate.Invoke(chunk);
            }
            chunk.RefreshBorderChunk();
        }

        //Create the landscape for the chunk
        private void createLandScape_threaded(VisualChunk visualChunk)
        {
            GeneratedChunk generatedChunk = _worldGenerator.GetChunk(visualChunk.ChunkPosition);
            
            visualChunk.BlockData.SetBlockBytes(generatedChunk.BlockData.GetBlocksBytes());
            //visualChunk.Entities = generatedChunk.Entities;
            
            CreateVisualEntities(generatedChunk, visualChunk);

            visualChunk.State = ChunkState.LandscapeCreated;
            visualChunk.ThreadStatus = ThreadStatus.Idle;
        }

        private void CreateVisualEntities(AbstractChunk source, VisualChunk target)
        {
            target.SetNewEntityCollection(source.Entities);

            //Create the Sprite Entities
            foreach (var spriteEntity in source.Entities.Enumerate<SpriteEntity>())
            {
                target.VisualSpriteEntities.Add(new VisualSpriteEntity(spriteEntity));
            }
            
            source.Entities.IsDirty = false;
        }
        #endregion
    }
}
