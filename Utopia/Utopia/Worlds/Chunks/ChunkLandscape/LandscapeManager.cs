using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Utopia.Network;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Storage.Structs;
using Ninject;
using S33M3CoreComponents.Timers;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Inventory;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Interfaces;
using SharpDX;
using Utopia.Shared.LandscapeEntities;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager2D
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private readonly object _syncRoot = new object();
        private WorldGenerator _worldGenerator;
        private ServerComponent _server;
        private Dictionary<Vector3I, ChunkDataMessage> _receivedServerChunks;
        private IChunkStorageManager _chunkStorageManager;
        private TimerManager.GameTimer _timer;
        private VoxelModelManager _voxelModelManager;
        private LandscapeBufferManager _landscapeEntityManager;
        #endregion


        [Inject] // ==> This property will be automatically SET by NInject with binded WorldGenerator singleton !
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }

        public IWorldChunks2D WorldChunks { get; set; }

        public EntityFactory EntityFactory { get; set; }

        public LandscapeManager(ServerComponent server, IChunkStorageManager chunkStorageManager, TimerManager timerManager, VoxelModelManager voxelModelManager, LandscapeBufferManager landscapeEntityManager)
        {
            _chunkStorageManager = chunkStorageManager;
            _voxelModelManager = voxelModelManager;

            _server = server;
            _receivedServerChunks = new Dictionary<Vector3I, ChunkDataMessage>(1024);
            _server.MessageChunkData += ServerConnection_MessageChunkData;
            _landscapeEntityManager = landscapeEntityManager;
            
            //Add a new Timer trigger
            _timer = timerManager.AddTimer(10000);
            _timer.OnTimerRaised += _timer_OnTimerRaised;

            Initialize();
        }

        public void Dispose()
        {
            _timer.OnTimerRaised -= _timer_OnTimerRaised;
            _server.MessageChunkData -= ServerConnection_MessageChunkData;
        }

        #region Public methods
        //Create the landscape for the chunk
        public void CreateLandScape(VisualChunk chunk)
        {
            CheckServerReceivedData(chunk);
        }

        #endregion

        #region Private methods
        private void Initialize()
        {
        }

        //New chunk Received !
        private void ServerConnection_MessageChunkData(object sender, ProtocolMessageEventArgs<ChunkDataMessage> e)
        {            
#if DEBUG
                logger.Trace("Chunk received from server; Position : {0}, Flag :{1}, Hash {2}", e.Message.Position, e.Message.Flag, e.Message.ChunkHash);
#endif

            //Bufferize the Data here   
            lock (_syncRoot)
            {
                if (_receivedServerChunks.ContainsKey(e.Message.Position)) _receivedServerChunks.Remove(e.Message.Position);
                _receivedServerChunks.Add(e.Message.Position, e.Message);
            }
        }

        //Perform Maintenance task every 10 seconds
        private void _timer_OnTimerRaised(float elapsedTimeInS)
        {
            ChunkBufferCleanup();
        }

        private void ChunkBufferCleanup()
        {
            lock (_syncRoot)
            {
                //Remove Data received from the Server that have not been processed for 1 minutes !
                var expiredIndex = _receivedServerChunks.Where(x => DateTime.Now.Subtract(x.Value.MessageRecTime).TotalSeconds > 60).ToList();
                for (int i = 0; i < expiredIndex.Count; i++)
                {
                    //Remove Data not used,but received from the server
                    logger.Trace("Data received from server never used {0} {1} (Not requested ??) for 60 seconds, was cleaned up !", expiredIndex[i].Value.Position, expiredIndex[i].Value.Flag );
                    _receivedServerChunks.Remove(expiredIndex[i].Key);
                }
            }

            //Check for chunk that are waiting for server data for more than 30 seconds.
            foreach (var chunk in WorldChunks.Chunks.Where(x => x.IsServerRequested))
            {
                if (DateTime.Now.Subtract(chunk.ServerRequestTime).Seconds > 20)
                {
                    chunk.IsServerRequested = false;
                    logger.Warn("Requested Chunk {0} {1} (Resync scenario : {2}) did not received server data for 20s !! NOT Normal (Chunk will be requested again !)", chunk.Position, chunk.State, chunk.IsServerResyncMode);
                }
            }
        }

        private void CheckServerReceivedData(VisualChunk chunk)
        {
            //Is this chunk server requested ==> Then check if the result is buffered
            if (chunk.IsServerRequested)
            {
                ChunkDataMessage message;
                //Have we receive the Server data
                if (_receivedServerChunks.TryGetValue(chunk.Position, out message))
                {
                    if (chunk.IsServerResyncMode)
                    {
                        if (message.Flag != ChunkDataMessageFlag.ChunkWasModified)
                        {
                            //Do nothing we are still in sync !
                            chunk.IsServerRequested = false;
                            chunk.IsServerResyncMode = false;
                            lock (_syncRoot) _receivedServerChunks.Remove(chunk.Position); //Remove the sync from the recieved queue
                            //logger.Debug("Chunk Processed {0} - Resync Mode  !", chunk.Position);
                            return;
                        }
                        else
                        {
                            chunk.State = ChunkState.MeshesChanged;
                            var chunkHash = chunk.GetMd5Hash();
                            logger.Debug("[Chunk Resync] {0} - was DESYNC with server [{1}] !", chunk.Position, message.Flag.ToString());
                        }
                    }

                    switch (message.Flag)
                    {
                        //In this case the message contains the data from the landscape !
                        case ChunkDataMessageFlag.ChunkWasModified:

                            var networkChunk = new CompressibleChunk(new InsideDataProvider());
                            networkChunk.Decompress(message.Data);

                            chunk.Consume(networkChunk); //Set the data into the "Big Array"
                            EntityFactory.PrepareEntities(chunk.Entities);

                            lock (_syncRoot)
                            {
                                _receivedServerChunks.Remove(chunk.Position); //Remove the chunk from the recieved queue
                                //logger.Debug("Chunk Processed {0} - ChunkWasModified  !", chunk.Position);
                            }

                            //Save the modified chunk landscape data locally only if the local one is different from the server one
                            Md5Hash hash;
                            bool saveChunk = true;
                            if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.Position, out hash))
                            {
                                if (hash == message.ChunkHash) saveChunk = false;
                            }

                            if (saveChunk)
                            {
                                _chunkStorageManager.StoreData_async(new ChunkDataStorage
                                                                         {
                                                                             ChunkPos = chunk.Position,
                                                                             Md5Hash = message.ChunkHash,
                                                                             CubeData = chunk.Compress()
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

                            CreateLandscapeFromGenerator(chunk);
                            lock (_syncRoot)
                            {
                                _receivedServerChunks.Remove(chunk.Position);
                                //logger.Debug("Chunk Processed {0} - ChunkCanBeGenerated  !", chunk.Position);
                            }
                            if (chunk.StorageRequestTicket != 0)
                            {
                                _chunkStorageManager.FreeTicket(chunk.StorageRequestTicket);
                                chunk.StorageRequestTicket = 0;
                            }
                            break;
                        case ChunkDataMessageFlag.ChunkMd5Equal:
                            //Do we still have to wait for the chunk from the local storage ??
                            ChunkDataStorage data = _chunkStorageManager.Data[chunk.StorageRequestTicket];
                            if (data != null)
                            {
                                //Data are present !
                                chunk.Decompress(data.CubeData); //Set the data into the "Big Array"
                                lock (_syncRoot)
                                {
                                    _receivedServerChunks.Remove(chunk.Position); //Remove the chunk from the recieved queue
                                    //logger.Debug("Chunk Processed {0} - ChunkMd5Equal  !", chunk.Position);
                                }
                                EntityFactory.PrepareEntities(chunk.Entities);

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

                    chunk.RefreshBorderChunk();
                    chunk.IsServerRequested = false;
                    chunk.IsServerResyncMode = false;
                    chunk.State = ChunkState.LandscapeCreated;
                }
            }
            else
            {

#if DEBUG
                logger.Trace("Chunk request to server : " + chunk.Position);
#endif
                //Request Chunk to the server, to see its server State (Was it modified by someone Else ???)

                chunk.IsServerRequested = true;
                Md5Hash hash;
                if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.Position, out hash))
                {
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one. This way the server won't have to send back the chunk data
                    chunk.StorageRequestTicket = _chunkStorageManager.RequestDataTicket_async(chunk.Position);

                    //We have already in the store manager a modified version of the chunk, do the server request with these information
                    _server.ServerConnection.Send(new GetChunksMessage
                    {
                        HashesCount = 1,
                        Md5Hashes = new[] { hash },
                        Positions = new[] { chunk.Position },
                        Range = new Range3I(chunk.Position, Vector3I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
                else
                {
                    //Chunk has never been modified. Request it by the chunkposition to the server
                    _server.ServerConnection.Send(new GetChunksMessage
                    {
                        Range = new Range3I(chunk.Position, Vector3I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
            }
        }

        //Create the landscape for the chunk
        private void CreateLandscapeFromGenerator(VisualChunk visualChunk)
        {
            var generatedChunk = _worldGenerator.GetChunk(visualChunk.Position);

            //Assign The Block generated to the Chunk
            visualChunk.BlockData.SetBlockBytes(generatedChunk.BlockData.GetBlocksBytes(), generatedChunk.BlockData.GetTags());
            visualChunk.BlockData.ColumnsInfo = generatedChunk.BlockData.ColumnsInfo;
            visualChunk.BlockData.ChunkMetaData = generatedChunk.BlockData.ChunkMetaData;
            //Copy the entities
            visualChunk.Entities.Import(generatedChunk.Entities, true);
        }

        #endregion
    }
}
