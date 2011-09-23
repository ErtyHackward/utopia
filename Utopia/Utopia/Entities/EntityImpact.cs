using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Storage;
using Utopia.Network;

namespace Utopia.Entities
{
    public static class EntityImpact
    {
        [Flags]
        public enum ReplaceBlockResult
        {
            LigthingImpact = 1
        }

        private static SingleArrayChunkContainer _cubesHolder;
        private static ILightingManager _lightManager;
        private static IWorldChunks _worldChunks;
        private static IChunkStorageManager _chunkStorageManager;
        private static Server _server;

        public static void Init(SingleArrayChunkContainer cubesHolder, ILightingManager lightManager, IWorldChunks worldChunks, IChunkStorageManager chunkStorageManager, Server server)
        {
            _server = server;
            _chunkStorageManager = chunkStorageManager;
            _cubesHolder = cubesHolder;
            _lightManager = lightManager;
            _worldChunks = worldChunks;
        }

        public static void CleanUp()
        {
            _server = null;
            _chunkStorageManager = null;
            _cubesHolder = null;
            _lightManager = null;
            _worldChunks = null;
        }

          public static void ReplaceBlocks(TerraCubeWithPosition[] coordinatesAndReplacement)
          {
              foreach (var locationBlock in coordinatesAndReplacement)
              {
                  //TODO here i have to copy the value to satisfy the use of ref in ReplaceBlock, cause i intended the location to be readonly  
                  // Locations should not be modifiable here, so they should not be passed by ref ?? 
                  Vector3I location = locationBlock.Position;

                  ReplaceBlock(ref location, locationBlock.Cube.Id);
              }
          }

        public static void ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId)
        {
            TerraCube newCube = new TerraCube(replacementCubeId);

            _cubesHolder.SetCube(ref cubeCoordinates, ref newCube);
            LigthingImpact(ref cubeCoordinates, replacementCubeId);

            //Save the modified Chunk if single Player
            if (!_server.Connected)
            {
                VisualChunk neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
                _chunkStorageManager.StoreData_async(new Worlds.Storage.Structs.ChunkDataStorage() { ChunkId = neightboorChunk.ChunkID, ChunkX = neightboorChunk.ChunkPosition.X, ChunkZ = neightboorChunk.ChunkPosition.Y, Md5Hash = null, CubeData = neightboorChunk.BlockData.GetBlocksBytes()});
            }
        }

        private static void LigthingImpact(ref Vector3I cubeCoordinates, ushort replacementCubeId)
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

            //Find the chunks that have been impacted = Max of 4
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

    }
}
