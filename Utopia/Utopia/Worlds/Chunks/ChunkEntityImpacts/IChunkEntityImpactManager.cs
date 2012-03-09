using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Storage;
using Utopia.Network;
using S33M3_Resources.Structs;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public interface IChunkEntityImpactManager : ILandscapeManager2D, IDisposable
    {
        SingleArrayChunkContainer CubesHolder { get; set; }
        IWorldChunks WorldChunks { get; set; }
        void ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId);
        void ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId);
        void LateInitialization(ServerComponent server, SingleArrayChunkContainer cubesHolder, IWorldChunks worldChunks, IChunkStorageManager chunkStorageManager, ILightingManager lightManager);
    }
}
