using System;
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

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class ChunkEntityImpactManager : IChunkEntityImpactManager
    {
        #region Private variables
        private bool _initialized;
        private ServerComponent _server;
        private SingleArrayChunkContainer _cubesHolder;
        private IWorldChunks _worldChunks;
        private IChunkStorageManager _chunkStorageManager;
        private ILightingManager _lightManager;
        private List<TerraCubePositionTag> _onHoldNetworkMsg = new List<TerraCubePositionTag>(1000);
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
            var handler = BlockReplaced;
            if (handler != null) handler(this, e);
        }

        public ChunkEntityImpactManager()
        {
            _initialized = false;
        }

        public void LateInitialization( ServerComponent server,
                                        SingleArrayChunkContainer cubesHolder,
                                        IWorldChunks worldChunks,
                                        IChunkStorageManager chunkStorageManager,
                                        ILightingManager lightManager)
        {

            _server = server;
            _lightManager = lightManager;
            _worldChunks = worldChunks;
            _chunkStorageManager = chunkStorageManager;
            _server.MessageBlockChange += ServerConnection_MessageBlockChange;
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
                    _onHoldNetworkMsg.Add(new TerraCubePositionTag(e.Message.BlockPositions[i], e.Message.BlockValues[i], tag));
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
        #endregion

        #region Public methods
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

            //Get Cube Profile
            CubeProfile cubeProfile = RealmConfiguration.CubeProfiles[replacementCubeId];

            ////Check if the cube is not already the same ? ! ?
            TerraCube existingCube = _cubesHolder.Cubes[cubeArrayIndex];
            if (existingCube.Id == replacementCubeId)
            {
                if (cubeProfile.IsTaggable)
                {
                    BlockTag ExistingTag = impactedChunk.BlockData.GetTag(BlockHelper.GlobalToInternalChunkPosition(cubeCoordinates));
                    if (ExistingTag == blockTag)
                    {
                        return true; //The block & tags are the sames !
                    }
                }
                else return true; //The block are the sames !
            }

            //Change the cube in the big array
            impactedChunk.BlockData.SetBlock(cubeCoordinates, replacementCubeId);

            //Update chunk tag collection if needed
            if (cubeProfile.IsTaggable)
            {
                impactedChunk.BlockData.SetTag(blockTag, BlockHelper.GlobalToInternalChunkPosition(cubeCoordinates));
            }

            //Start Chunk Visual Impact to decide what needs to be redraw, will be done in async mode, quite heavy, will also restart light computations for the impacted chunk range.
            TerraCubeWithPosition cube = new TerraCubeWithPosition(cubeCoordinates, replacementCubeId);
            //SmartThread.ThreadPool.QueueWorkItem(CheckImpactThreaded, cube, Amib.Threading.WorkItemPriority.Highest);
            CheckImpact(cube, impactedChunk);

            //Raise event for sound
            OnBlockReplaced(new LandscapeBlockReplacedEventArgs { Position = cubeCoordinates, NewBlockType = replacementCubeId, PreviousBlock = existingCube.Id });

            //Save the modified Chunk in local buffer DB, only the structure is saved, not the Lighting data.
            //Is it Worth ????
            impactedChunk.CompressedDirty = true;
            Md5Hash chunkHash;
            byte[] chunkDataCompressed = impactedChunk.CompressAndComputeHash(out chunkHash);
            _chunkStorageManager.StoreData_async(new Storage.Structs.ChunkDataStorage { ChunkId = impactedChunk.ChunkID, ChunkX = impactedChunk.ChunkPosition.X, ChunkZ = impactedChunk.ChunkPosition.Y, Md5Hash = chunkHash, CubeData = chunkDataCompressed });

            return true;
        }

        /// <summary>
        /// Check the impact of the block/Item replacement.
        /// Mostly will check wish of the surrounding visualchunks must be refresh (lighting or cube masked face reason).
        /// This is only for drawing purpose, not landscape modification here.
        /// </summary>
        /// <param name="cubeCoordinates">The cube where the modification has been realized</param>
        /// <param name="replacementCubeId">The type of the modified cube</param>
        public void CheckImpact(TerraCubeWithPosition cube, VisualChunk cubeChunk)
        {
            Int64 mainChunkId;
            List<VisualChunk> impactedChunks = new List<VisualChunk>(9);

            //Compute the Range impacted by the cube change
            Range3I cubeRange = new Range3I()
            {
                Position = new Vector3I(cube.Position.X - _lightManager.LightPropagateSteps, 0, cube.Position.Z - _lightManager.LightPropagateSteps),
                Size = new Vector3I((_lightManager.LightPropagateSteps * 2) + 1, _worldChunks.VisualWorldParameters.WorldVisibleSize.Y, (_lightManager.LightPropagateSteps * 2) + 1)
            };

            //recompute the light sources without the range
            _lightManager.CreateLightSources(ref cubeRange);
            Console.WriteLine("LightSource : " + cubeRange.Position + " " + cubeRange.Max);
            cubeRange.Position.X--;
            cubeRange.Position.Z--;
            cubeRange.Size.X += 2;
            cubeRange.Size.Z += 2;


            //Propagate the light, we add one cube around the previous Range !! <= !!
            Console.WriteLine("PropagateLightSources : " + cubeRange.Position + " " + cubeRange.Max);
            _lightManager.PropagateLightSources(ref cubeRange, true, true);
            
            CubeProfile profile = RealmConfiguration.CubeProfiles[cube.Cube.Id];

            //Find the chunks that have been impacted around the 8 surrounding chunks
            cubeChunk.State = ChunkState.OuterLightSourcesProcessed;
            cubeChunk.UpdateOrder = !profile.IsBlockingLight ? 1 : 2;
            mainChunkId = cubeChunk.ChunkID;

            impactedChunks.Add(cubeChunk);

            //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
            VisualChunk NeightBorChunk;
            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z + _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X + _lightManager.LightPropagateSteps, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

            NeightBorChunk = _worldChunks.GetChunk(cube.Position.X - _lightManager.LightPropagateSteps, cube.Position.Z - _lightManager.LightPropagateSteps);
            if (NeightBorChunk.ChunkID != mainChunkId)
            {
                NeightBorChunk.State = ChunkState.OuterLightSourcesProcessed;
                NeightBorChunk.UpdateOrder = !profile.IsBlockingLight ? 2 : 1;
                //Console.WriteLine(NeightBorChunk.ChunkID + " => " + NeightBorChunk.UserChangeOrder);
                impactedChunks.Add(NeightBorChunk);
            }

        }


        public IChunkLayout2D GetChunk(Vector2I chunkPosition)
        {
            return _worldChunks.GetChunk(chunkPosition.X * AbstractChunk.ChunkSize.X, chunkPosition.Y * AbstractChunk.ChunkSize.Z);
        }

        public IChunkLayout2D GetChunk(Vector3I chunkPosition)
        {
            return _worldChunks.GetChunk(ref chunkPosition);
        }

        public ILandscapeCursor GetCursor(Vector3I blockPosition)
        {
            return new SingleArrayLandscapeCursor(this, blockPosition);
        }

        public ILandscapeCursor GetCursor(Vector3D entityPosition)
        {
            return GetCursor(new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z)));
        }
        #endregion


    }

    public class LandscapeBlockReplacedEventArgs : EventArgs
    {
        public Vector3I Position { get; set; }
        public byte NewBlockType { get; set; }
        public byte PreviousBlock { get; set; }
    }
}


