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

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private WorldGenerator _worldGenerator;
        private ServerComponent _server;
        private Dictionary<long, ChunkDataMessage> _receivedServerChunks;
        private IChunkStorageManager _chunkStorageManager;
        private TimerManager.GameTimer _timer;
        private VoxelModelManager _voxelModelManager;
        #endregion


        [Inject] // ==> This property will be automatically SET by NInject with binded WorldGenerator singleton !
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }

        public EntityFactory EntityFactory { get; set; }

        public LandscapeManager(ServerComponent server, IChunkStorageManager chunkStorageManager, TimerManager timerManager, VoxelModelManager voxelModelManager)
        {
            _chunkStorageManager = chunkStorageManager;
            _voxelModelManager = voxelModelManager;

            _server = server;
            _receivedServerChunks = new Dictionary<long, ChunkDataMessage>(1024);
            _server.MessageChunkData += ServerConnection_MessageChunkData;
            
            //Add a new Timer trigger
            _timer = timerManager.AddTimer(0, 10000);
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

            Int64 chunkId = VisualChunk.ComputeChunkId(e.Message.Position.X, e.Message.Position.Y);
#if DEBUG
            logger.Debug("Chunk received from server id : {0}; Position : {1}, Flag :{2}", chunkId, e.Message.Position, e.Message.Flag);
#endif

            //Bufferize the Data here
            
            if (_receivedServerChunks.ContainsKey(chunkId)) 
                _receivedServerChunks.Remove(chunkId);
            _receivedServerChunks.Add(chunkId, e.Message);
        }

        //Perform Maintenance task every 10 seconds
        private void _timer_OnTimerRaised()
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

        private void CheckServerReceivedData(VisualChunk chunk)
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
                            //CreateVisualEntities(chunk, chunk);

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


                            break;
                        case ChunkDataMessageFlag.ChunkCanBeGenerated:
                            CreateLandscapeFromGenerator(chunk);

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
                                chunk.Decompress(EntityFactory, data.CubeData); //Set the data into the "Big Array"
                                _receivedServerChunks.Remove(chunk.ChunkID); //Remove the chunk from the recieved queue

                                //CreateVisualEntities(chunk, chunk);

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
                    chunk.State = ChunkState.LandscapeCreated;
                }
            }
            else
            {

#if DEBUG
                logger.Trace("Chunk request to server : " + chunk.ChunkID);
#endif
                //Request Chunk to the server, to see its server State (Was it modified by someone Else ???)

                chunk.IsServerRequested = true;
                Md5Hash hash;
                if (_chunkStorageManager.ChunkHashes.TryGetValue(chunk.ChunkID, out hash))
                {
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one. This way the server won't have to send back the chunk data
                    chunk.StorageRequestTicket = _chunkStorageManager.RequestDataTicket_async(chunk.ChunkID);

                    //We have already in the store manager a modified version of the chunk, do the server request with these information
                    _server.ServerConnection.Send(new GetChunksMessage
                    {
                        HashesCount = 1,
                        Md5Hashes = new Md5Hash[] { hash },
                        Positions = new Vector2I[] { chunk.ChunkPosition },
                        Range = new Range2I(chunk.ChunkPosition, Vector2I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
                else
                {
                    //Chunk has never been modified. Request it by the chunkposition to the server
                    _server.ServerConnection.Send(new GetChunksMessage
                    {
                        Range = new Range2I(chunk.ChunkPosition, Vector2I.One),
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    });
                }
            }
        }

        //Create the landscape for the chunk
        private void CreateLandscapeFromGenerator(VisualChunk visualChunk)
        {
            GeneratedChunk generatedChunk = _worldGenerator.GetChunk(visualChunk.ChunkPosition);

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
