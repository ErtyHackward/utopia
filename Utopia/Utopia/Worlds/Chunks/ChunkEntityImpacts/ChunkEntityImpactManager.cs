using System;
using System.IO;
using ProtoBuf;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Structs.Landscape;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3DXEngine.Threading;
using System.Collections.Generic;
using Utopia.Shared.Configuration;
using Utopia.Shared.World;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class ChunkEntityImpactManager : LandscapeManager<VisualChunk>, IChunkEntityImpactManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private bool _initialized;
        private ServerComponent _server;
        private SingleArrayChunkContainer _cubesHolder;
        private IWorldChunks _worldChunks;
        private IChunkStorageManager _chunkStorageManager;
        private ILightingManager _lightManager;
        private List<TerraCubePositionTag> _onHoldNetworkMsg = new List<TerraCubePositionTag>(1000);
        private VisualWorldParameters _visualWorldParameters;
        #endregion

        #region Public variables/properties
        public SingleArrayChunkContainer CubesHolder
        {
            get { return _cubesHolder; }
            set { _cubesHolder = value; }
        }

        public IWorldChunks WorldChunks
        {
            get { return _worldChunks; }
            set { _worldChunks = value; }
        }
        #endregion

        /// <summary>
        /// Occurs when a single block at the landscape is changed
        /// </summary>
        public event EventHandler<LandscapeBlockReplacedEventArgs> BlockReplaced;
        private void OnBlockReplaced(LandscapeBlockReplacedEventArgs e)
        {
            if (BlockReplaced != null) BlockReplaced(this, e);
        }

        public event EventHandler<StaticEventArgs> StaticEntityAdd;
        private void OnStaticEntityAdd(StaticEventArgs e)
        {
            if (StaticEntityAdd != null) StaticEntityAdd(this, e);
        }

        public event EventHandler<StaticEventArgs> StaticEntityRemoved;
        private void OnStaticEntityRemoved(StaticEventArgs e)
        {
            if (StaticEntityRemoved != null) StaticEntityRemoved(this, e);
        }
        
        public ChunkEntityImpactManager()
            : base(null)
        {
            _initialized = false;
        }

        public void LateInitialization( ServerComponent server,
                                        SingleArrayChunkContainer cubesHolder,
                                        IWorldChunks worldChunks,
                                        IChunkStorageManager chunkStorageManager,
                                        ILightingManager lightManager,
                                        VisualWorldParameters visualWorldParameters
                                        )
        {
            _server = server;
            _lightManager = lightManager;
            _worldChunks = worldChunks;
            _chunkStorageManager = chunkStorageManager;
            _server.MessageBlockChange += ServerConnection_MessageBlockChange;
            _visualWorldParameters = visualWorldParameters;
            _wp = _visualWorldParameters.WorldParameters;
            _cubesHolder = cubesHolder;
            

            _initialized = true;
        }

        public void Dispose()
        {
            if (_initialized)
            {
                _server.MessageBlockChange -= ServerConnection_MessageBlockChange;
            }
        }

        #region Private methods
        /// <summary>
        /// Event raise when receiving cube change from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerConnection_MessageBlockChange(object sender, ProtocolMessageEventArgs<BlocksChangedMessage> e)
        {
            //The server change modification will be queued inside a single concurrency thread pool. (Only one running at the same time)
            ThreadsManager.RunAsync(() => ReplaceBlockThreaded(e), singleConcurrencyRun: true);
        }

        private void ReplaceBlockThreaded(ProtocolMessageEventArgs<BlocksChangedMessage> e)
        {
            //For each block modified transform the data to get both CubeID and Cube World position, then call the ReplaceBlock that will analyse
            //whats the impact of the block replacement - Draw impact only - to know wish chunks must be refreshed.
            _onHoldNetworkMsg.Clear();
            for (int i = 0; i < e.Message.BlockValues.Length; i++)
            {
                BlockTag tag = e.Message.Tags != null ? e.Message.Tags[i] : null;

                if (ReplaceBlock(ref e.Message.BlockPositions[i], e.Message.BlockValues[i], true, tag) == false)
                {
                    _onHoldNetworkMsg.Add(new TerraCubePositionTag(e.Message.BlockPositions[i], e.Message.BlockValues[i], tag, _visualWorldParameters.WorldParameters.Configuration));
                }
            }

            //If it was not possible to Do the block changes on chunk then retry until you can !
            while (_onHoldNetworkMsg.Count > 0)
            {
                System.Threading.Thread.Sleep(5);
                for (int i = _onHoldNetworkMsg.Count - 1; i >= 0; i--)
                {
                    if (ReplaceBlock(ref _onHoldNetworkMsg[i].Position, _onHoldNetworkMsg[i].Cube.Id, true, _onHoldNetworkMsg[i].Tag) == true)
                    {
                        _onHoldNetworkMsg.RemoveAt(i);
                    }
                }
            }
        }

        public void ProcessMessageEntityOut(ProtocolMessageEventArgs<EntityOutMessage> e)
        {
            // Only take into account static entity
            if (e.Message.Link.IsStatic)
            {
                //The server change modification will be queued inside a single concurrency thread pool. (Only one running at the same time)
                ThreadsManager.RunAsync(() => RemoveEntity(e.Message.Link, e.Message.TakerEntityId), singleConcurrencyRun: true);
            }
        }

        public void ProcessMessageEntityIn(ProtocolMessageEventArgs<EntityInMessage> e)
        {
            //Only take into account static entity
            if (e.Message.Link.IsStatic)
            {
                ThreadsManager.RunAsync(() => AddEntity((IStaticEntity)e.Message.Entity, e.Message.SourceEntityId), singleConcurrencyRun: true);
            }
        }

        private void SendChunkForBuffering(VisualChunk impactedChunk)
        {
            // Save the modified Chunk in local buffer DB, only the structure is saved, not the Lighting data.
            
            impactedChunk.CompressedDirty = true;
            Md5Hash chunkHash = impactedChunk.GetMd5Hash();
            byte[] chunkDataCompressed = impactedChunk.Compress();

            _chunkStorageManager.StoreData_async(new Storage.Structs.ChunkDataStorage { ChunkPos = impactedChunk.Position, Md5Hash = chunkHash, CubeData = chunkDataCompressed });

        }
        #endregion

        #region Public methods
        public void AddEntity(IStaticEntity entity, uint sourceDynamicId = 0)
        {
            Vector3I entityBlockPosition;
            //If the entity is of type IBlockLinkedEntity, then it needs to be store inside the chunk where the LinkedEntity belong.
            var blockLinkedItem = entity as IBlockLinkedEntity;

            if (blockLinkedItem != null && blockLinkedItem.Linked)
            {
                entityBlockPosition = ((IBlockLinkedEntity)entity).LinkedCube;
            }
            else
            {
                entityBlockPosition = (Vector3I)entity.Position;
            }

            var impactedChunk = GetChunkFromBlock(entityBlockPosition);
            if (impactedChunk == null)
                return;

            impactedChunk.Entities.Add(entity, sourceDynamicId);

            //Raise event (Playing sound)
            OnStaticEntityAdd(new StaticEventArgs() { Entity = entity });

            // Save the modified Chunk in local buffer DB
            SendChunkForBuffering(impactedChunk);
        }

        public IStaticEntity RemoveEntity(EntityLink entity, uint sourceDynamicId = 0)
        {
            IStaticEntity entityRemoved;
            var impactedChunk = GetChunk(entity.ChunkPosition);

            if (impactedChunk == null)
                return null;
            impactedChunk.Entities.RemoveById(entity.Tail[0], sourceDynamicId, out entityRemoved);

            //Raise event (Playing sound)
            OnStaticEntityRemoved(new StaticEventArgs() { 
                Entity = entityRemoved, 
                SourceEntityId = sourceDynamicId 
            });

            // Save the modified Chunk in local buffer DB
            SendChunkForBuffering(impactedChunk);

            return entityRemoved;
        }

        public bool ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChange, BlockTag blockTag = null)
        {
            return ReplaceBlock(_cubesHolder.Index(ref cubeCoordinates), ref cubeCoordinates, replacementCubeId, isNetworkChange, blockTag);
        }

        public bool ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChanged, BlockTag blockTag = null)
        {
            VisualChunk impactedChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);

            if (impactedChunk.State != ChunkState.DisplayInSyncWithMeshes && isNetworkChanged)
            {
                return false;
            }

            try
            {
                // Check if the cube is not already the same ? ! ?
                var existingCube = _cubesHolder.Cubes[cubeArrayIndex];

                var inChunkPos = BlockHelper.GlobalToInternalChunkPosition(cubeCoordinates);

                if (existingCube.Id == replacementCubeId)
                {
                    // tag change event
                    // some tags changes requires chunk mesh rebuild (LiquidTag), some not (DamageTag)
                    // we will update the mesh only if at least one tag (current or previous) requires mesh update

                    var needChunkMeshUpdate = false;

                    var oldTag = impactedChunk.BlockData.GetTag(inChunkPos);
                
                    if (oldTag != null && oldTag.RequireChunkMeshUpdate)
                        needChunkMeshUpdate = true;

                    if (blockTag != null && blockTag.RequireChunkMeshUpdate)
                        needChunkMeshUpdate = true;

                    if (!needChunkMeshUpdate)
                    {
                        impactedChunk.BlockData.SetTag(blockTag, inChunkPos);
                        return true;
                    }
                }

                // Change the cube in the big array
                impactedChunk.BlockData.SetBlock(inChunkPos, replacementCubeId, blockTag);

                // Start Chunk Visual Impact to decide what needs to be redraw, will be done in async mode, 
                // quite heavy, will also restart light computations for the impacted chunk range.
                var cube = new TerraCubeWithPosition(cubeCoordinates, replacementCubeId, _visualWorldParameters.WorldParameters.Configuration);

#if PERFTEST
            if (Utopia.Worlds.Chunks.WorldChunks.perf.Actif == false)
            {
                Utopia.Worlds.Chunks.WorldChunks.perf.Actif = true;
                Utopia.Worlds.Chunks.WorldChunks.perf.CollectedData = new List<string>();
                Utopia.Worlds.Chunks.WorldChunks.perf.AddData("Started New User Action");
                Utopia.Worlds.Chunks.WorldChunks.perf.sw.Restart();
            }
#endif

                impactedChunk.UpdateOrder = 1;
                ThreadsManager.RunAsync(() => CheckImpact(cube, impactedChunk), ThreadsManager.ThreadTaskPriority.High);

                // Raise event for sound
                OnBlockReplaced(new LandscapeBlockReplacedEventArgs { 
                    IsLocalPLayerAction = !isNetworkChanged, 
                    Position = cubeCoordinates, 
                    NewBlockType = replacementCubeId, 
                    PreviousBlock = existingCube 
                });
                
                return true;
            }
            finally
            {
                // Save the modified Chunk in local buffer DB
                SendChunkForBuffering(impactedChunk);
            }
        }

        /// <summary>
        /// Check the impact of the block/Item replacement.
        /// Mostly will check wish of the surrounding visualchunks must be refresh (lighting or cube masked face reason).
        /// This is only for drawing purpose, not landscape modification here.
        /// </summary>
        /// <param name="cubeCoordinates">The cube where the modification has been realized</param>
        /// <param name="replacementCubeId">The type of the modified cube</param>
        public void CheckImpact(TerraCubeWithPosition cube, VisualChunkBase cubeChunk)
        {
            Vector3I mainChunkId;

#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("CheckImpact BEGIN");
#endif

            //Compute the Range impacted by the cube change
            var cubeRange = new Range3I
            {
                Position = new Vector3I(cube.Position.X - _lightManager.LightPropagateSteps, 0, cube.Position.Z - _lightManager.LightPropagateSteps),
                Size = new Vector3I((_lightManager.LightPropagateSteps * 2) + 1, _worldChunks.VisualWorldParameters.WorldVisibleSize.Y, (_lightManager.LightPropagateSteps * 2) + 1)
            };

            //recompute the light sources without the range
            _lightManager.CreateLightSources(ref cubeRange);
            cubeRange.Position.X--;
            cubeRange.Position.Z--;
            cubeRange.Size.X += 2;
            cubeRange.Size.Z += 2;

            //Propagate the light, we add one cube around the previous Range !! <= !!
            _lightManager.PropagateLightSources(ref cubeRange, true, true);

            BlockProfile profile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.Cube.Id];

            //Find the chunks that have been impacted around the 8 surrounding chunks
            cubeChunk.State = ChunkState.OuterLightSourcesProcessed;
            cubeChunk.UpdateOrder = !profile.IsBlockingLight ? 1 : 2;
            mainChunkId = cubeChunk.Position;
            //Console.WriteLine(cubeChunk.ChunkID + " => " + cubeChunk.UpdateOrder);

#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.cubeChunkID = mainChunkId;
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("Modified chunk is : " + mainChunkId);
#endif

            VisualChunk NeightBorChunk;
            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.Position != mainChunkId && NeightBorChunk.State > ChunkState.OuterLightSourcesProcessed)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UpdateOrder);
            }

#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("CheckImpact END");
#endif
        }

        public override VisualChunk GetChunk(Vector3I position)
        {
            VisualChunk chunk;
            _worldChunks.GetSafeChunkFromChunkCoord(position, out chunk);
            return chunk;
        }

        public override TerraCube GetCubeAt(Vector3I vector3I)
        {
            return _cubesHolder.GetCube(vector3I).Cube;
        }

        public override ILandscapeCursor GetCursor(Vector3I blockPosition)
        {
            SingleArrayLandscapeCursor cursor = new SingleArrayLandscapeCursor(this, blockPosition, _wp.Configuration);
            if (cursor.isError) return null;
            else return cursor;
        }

        #endregion

    }

    public class LandscapeBlockReplacedEventArgs : EventArgs
    {
        public Vector3I Position { get; set; }
        public byte NewBlockType { get; set; }
        public TerraCube PreviousBlock { get; set; }
        public bool IsLocalPLayerAction { get; set; }
    }

    public class StaticEventArgs : EventArgs
    {
        public IStaticEntity Entity { get; set; }

        /// <summary>
        /// Dynamic entity that took or put the entity
        /// </summary>
        public uint SourceEntityId { get; set; }
    }
}


