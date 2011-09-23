using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Cubes;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class ChunkEntityImpactManager : IChunkEntityImpactManager
    {
        #region Private variables
        private Server _server;
        private SingleArrayChunkContainer _cubesHolder;
        private IWorldChunks _worldChunks;
        private IChunkStorageManager _chunkStorageManager;
        private ILightingManager _lightManager;
        #endregion

        #region Public variables/properties
        #endregion

        public ChunkEntityImpactManager(Server server,
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
        }

        public void Dispose()
        {
            _server.ServerConnection.MessageBlockChange -= ServerConnection_MessageBlockChange;
        }

        #region Private methods
        /// <summary>
        /// Event raise when receiving cube change from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerConnection_MessageBlockChange(object sender, ProtocolMessageEventArgs<BlocksChangedMessage> e)
        {
            byte cubeId;
            Vector3I cubePosition, cubeWorldPosition;
            Vector2I chunkWorldPosition = new Vector2I(e.Message.ChunkPosition.X * AbstractChunk.ChunkSize.X, e.Message.ChunkPosition.Y * AbstractChunk.ChunkSize.Z);
            //For each block modified transform the data to get both CubeID and Cube World position, then call the ReplaceBlock that will analyse
            //whats the impact of the block replacement - Draw impact only - to know wish chunks must be refreshed.
            for (int i = 0; i < e.Message.BlockValues.Length; i++)
            {
                cubeId = e.Message.BlockValues[i];
                cubePosition = e.Message.BlockPositions[i];
                cubeWorldPosition = new Vector3I(cubePosition.X + chunkWorldPosition.X, cubePosition.Y, cubePosition.Z + chunkWorldPosition.Y);
            }
        }

        public void ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId)
        {
            //Create the new cube
            TerraCube newCube = new TerraCube(replacementCubeId);

            //Check if the cube is not already the same ? ! ?
            TerraCube existingCube = _cubesHolder.Cubes[_cubesHolder.Index(ref cubeCoordinates)];
            if (existingCube.Id == replacementCubeId) return;

            //Change the cube in the big array
            _cubesHolder.SetCube(ref cubeCoordinates, ref newCube);

            CheckImpact(ref cubeCoordinates, replacementCubeId);

            //Save the modified Chunk in local buffer DB
            VisualChunk impactedChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
            _chunkStorageManager.StoreData_async(new Worlds.Storage.Structs.ChunkDataStorage() { ChunkId = impactedChunk.ChunkID, ChunkX = impactedChunk.ChunkPosition.X, ChunkZ = impactedChunk.ChunkPosition.Y, Md5Hash = null, CubeData = impactedChunk.BlockData.GetBlocksBytes() });
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

            _lightManager.CreateLightSources(ref cubeRange);

            cubeRange.Min.X--;
            cubeRange.Min.Z--;
            cubeRange.Max.X++;
            cubeRange.Max.Z++;

            _lightManager.PropagateLightSources(ref cubeRange, true);

            VisualCubeProfile profile = VisualCubeProfile.CubesProfile[replacementCubeId];

            //Find the chunks that have been impacted around the 8 surrending chunks
            VisualChunk neightboorChunk;
            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
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
        #endregion
    }
}

