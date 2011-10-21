using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Interfaces;
using S33M3Engines.Shared.Math;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class ChunkEntityImpactManager : IChunkEntityImpactManager
    {
        #region Private variables
        private bool _initialized;
        private Server _server;
        private SingleArrayChunkContainer _cubesHolder;
        private IWorldChunks _worldChunks;
        private IChunkStorageManager _chunkStorageManager;
        private ILightingManager _lightManager;
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

        public ChunkEntityImpactManager()
        {
            _initialized = false;
        }

        public void LateInitialization(Server server,
                                        SingleArrayChunkContainer cubesHolder,
                                        IWorldChunks worldChunks,
                                        IChunkStorageManager chunkStorageManager,
                                        ILightingManager lightManager)
        {

            _server = server;
            _lightManager = lightManager;
            _worldChunks = worldChunks;
            _chunkStorageManager = chunkStorageManager;
            _server.ServerConnection.MessageBlockChange += ServerConnection_MessageBlockChange;
            _cubesHolder = cubesHolder;
            _initialized = true;
        }

        public void Dispose()
        {
            if (_initialized)
            {
                _server.ServerConnection.MessageBlockChange -= ServerConnection_MessageBlockChange;
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
            //For each block modified transform the data to get both CubeID and Cube World position, then call the ReplaceBlock that will analyse
            //whats the impact of the block replacement - Draw impact only - to know wish chunks must be refreshed.
            for (int i = 0; i < e.Message.BlockValues.Length; i++)
            {
                ReplaceBlock(ref e.Message.BlockPositions[i], e.Message.BlockValues[i]);
            }
        }

        /// <summary>
        /// Check the impact of the block replacement.
        /// Mostly will check wish of the surrending visualchunks must be refresh (lighting or cube masked face reason).
        /// This is only for drawing purpose, not landscape modification here.
        /// </summary>
        /// <param name="cubeCoordinates">The cube that has been modified</param>
        /// <param name="replacementCubeId">The type of the modified cube</param>
        private void CheckImpact(ref Vector3I cubeCoordinates, byte replacementCubeId)
        {
            Int64 mainChunkId;

            //Compute the Range impacted by the cube change
            Range<int> cubeRange = new Range<int>()
            {
                Min = new Vector3I(cubeCoordinates.X - _lightManager.LightPropagateSteps, 0, cubeCoordinates.Z - _lightManager.LightPropagateSteps),
                Max = new Vector3I(cubeCoordinates.X + _lightManager.LightPropagateSteps, _worldChunks.VisualWorldParameters.WorldVisibleSize.Y, cubeCoordinates.Z + _lightManager.LightPropagateSteps)
            };

            //Refresh the Visual Entity if needed !
            VisualChunk neightboorChunk;
            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
            if (neightboorChunk.Entities.IsDirty)
            {
                neightboorChunk.RefreshVisualEntities();
            }

            _lightManager.CreateLightSources(ref cubeRange);

            cubeRange.Min.X--;
            cubeRange.Min.Z--;
            cubeRange.Max.X++;
            cubeRange.Max.Z++;

            _lightManager.PropagateLightSources(ref cubeRange, true, true);

            VisualCubeProfile profile = VisualCubeProfile.CubesProfile[replacementCubeId];

            //Find the chunks that have been impacted around the 8 surrending chunks
            neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
            neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
            neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 2 : 1;
            mainChunkId = neightboorChunk.ChunkID;
            //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + _lightManager.LightPropagateSteps, cubeCoordinates.Z);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - _lightManager.LightPropagateSteps, cubeCoordinates.Z);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z + _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z - _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + _lightManager.LightPropagateSteps, cubeCoordinates.Z + _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - _lightManager.LightPropagateSteps, cubeCoordinates.Z + _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + _lightManager.LightPropagateSteps, cubeCoordinates.Z - _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - _lightManager.LightPropagateSteps, cubeCoordinates.Z - _lightManager.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

        }
        #endregion

        #region Public methods
        public void ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId)
        {
            ReplaceBlock(_cubesHolder.Index(ref cubeCoordinates), ref cubeCoordinates, replacementCubeId);
        }

        public void ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId)
        {
            //Create the new cube
            TerraCube newCube = new TerraCube(replacementCubeId);

            //Check if the cube is not already the same ? ! ?
            TerraCube existingCube = _cubesHolder.Cubes[cubeArrayIndex];
            if (existingCube.Id == replacementCubeId) return;

            //Change the cube in the big array
            _cubesHolder.SetCube(cubeArrayIndex, ref cubeCoordinates, ref newCube);

            CheckImpact(ref cubeCoordinates, replacementCubeId);

            //Save the modified Chunk in local buffer DB
            //Is it Worth ????
            VisualChunk impactedChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
            impactedChunk.CompressedDirty = true;
            Md5Hash chunkHash;
            byte[] chunkDataCompressed = impactedChunk.CompressAndComputeHash(out chunkHash);
            _chunkStorageManager.StoreData_async(new Storage.Structs.ChunkDataStorage { ChunkId = impactedChunk.ChunkID, ChunkX = impactedChunk.ChunkPosition.X, ChunkZ = impactedChunk.ChunkPosition.Y, Md5Hash = chunkHash, CubeData = chunkDataCompressed });
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
}

