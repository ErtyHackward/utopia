using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Storage;
using Utopia.Network;
using S33M3Resources.Structs;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public interface IChunkEntityImpactManager : ILandscapeManager2D, IDisposable
    {
        /// <summary>
        /// Occurs when a single block at the landscape is changed
        /// </summary>
        event EventHandler<LandscapeBlockReplacedEventArgs> BlockReplaced;

        SingleArrayChunkContainer CubesHolder { get; set; }
        IWorldChunks WorldChunks { get; set; }
        void ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId);
        void ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId);
        void LateInitialization(ServerComponent server, SingleArrayChunkContainer cubesHolder, IWorldChunks worldChunks, IChunkStorageManager chunkStorageManager, ILightingManager lightManager);
    }
}
