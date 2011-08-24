using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.Cube;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Lighting;
using Utopia.Planets.Terran.Chunk;
using Utopia.Planets.Terran.Flooding;
using Utopia.PlugIn;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLighting;

namespace Utopia.Planets.Terran
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

        public static void Init(SingleArrayChunkContainer cubesHolder, ILightingManager lightManager, IWorldChunks worldChunks)
        {
            _cubesHolder = cubesHolder;
            _lightManager = lightManager;
            _worldChunks = worldChunks;
        }

          public static void ReplaceBlocks( TerraCubeWithPosition[] coordinatesAndReplacement)
          {
              foreach (var locationBlock in coordinatesAndReplacement)
              {
                  //TODO here i have to copy the value to satisfy the use of ref in ReplaceBlock, cause i intended the location to be readonly  
                  // Locations should not be modifiable here, so they should not be passed by ref ?? 
                  Location3<int> location = locationBlock.Position;

                  ReplaceBlock(ref location, locationBlock.Cube.Id);
              }
          }

        public static void ReplaceBlock(ref Location3<int> cubeCoordinates, byte replacementCubeId)
        {
            TerraCube newCube = new TerraCube(replacementCubeId);

            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                if (!WorldPlugins.Plugins.WorldPlugins[i].EntityBlockReplaced(ref cubeCoordinates, ref newCube)) return;
            }

            _cubesHolder.SetCube(ref cubeCoordinates, ref newCube);

            LigthingImpact(ref cubeCoordinates, replacementCubeId);
        }

        private static void LigthingImpact(ref Location3<int> cubeCoordinates, ushort replacementCubeId)
        {
            Int64 mainChunkId;

            //Compute the Range impacted by the cube change
            Range<int> cubeRange = new Range<int>()
            {
                Min = new Location3<int>(cubeCoordinates.X - TerraLighting.LightPropagateSteps, 0, cubeCoordinates.Z - TerraLighting.LightPropagateSteps),
                Max = new Location3<int>(cubeCoordinates.X + TerraLighting.LightPropagateSteps, LandscapeBuilder.Worldsize.Y, cubeCoordinates.Z + TerraLighting.LightPropagateSteps)
            };

            _lightManager.CreateLightSources(ref cubeRange);

            cubeRange.Min.X--;
            cubeRange.Min.Z--;
            cubeRange.Max.X++;
            cubeRange.Max.Z++;

            _lightManager.PropagateLightSources(ref cubeRange, true);

            RenderCubeProfile profile = RenderCubeProfile.CubesProfile[replacementCubeId];

            //Find the chunks that have been impacted = Max of 4
            VisualChunk neightboorChunk;
            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
            neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
            neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
            neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 2 : 1;
            mainChunkId = neightboorChunk.ChunkID;
            //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + TerraLighting.LightPropagateSteps, cubeCoordinates.Z);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - TerraLighting.LightPropagateSteps, cubeCoordinates.Z);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z + TerraLighting.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X, cubeCoordinates.Z - TerraLighting.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + TerraLighting.LightPropagateSteps, cubeCoordinates.Z + TerraLighting.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - TerraLighting.LightPropagateSteps, cubeCoordinates.Z + TerraLighting.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X + TerraLighting.LightPropagateSteps, cubeCoordinates.Z - TerraLighting.LightPropagateSteps);
            if (neightboorChunk.ChunkID != mainChunkId)
            {
                neightboorChunk.State = ChunkState.LandscapeLightsPropagated;
                neightboorChunk.ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
                //Console.WriteLine(neightboorChunk.ChunkID + " => " + neightboorChunk.UserChangeOrder);
            }

            neightboorChunk = _worldChunks.GetChunk(cubeCoordinates.X - TerraLighting.LightPropagateSteps, cubeCoordinates.Z - TerraLighting.LightPropagateSteps);
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
