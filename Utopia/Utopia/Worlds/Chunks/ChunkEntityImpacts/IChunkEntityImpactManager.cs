using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Storage;
using Utopia.Network;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

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
        bool ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChange, BlockTag blockTag = null);
        bool ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChange, BlockTag blockTag = null);
        void LateInitialization(ServerComponent server, SingleArrayChunkContainer cubesHolder, IWorldChunks worldChunks, IChunkStorageManager chunkStorageManager, ILightingManager lightManager, VisualWorldParameters visualWorldParameters);
        void CheckImpact(TerraCubeWithPosition cube, VisualChunk cubeChunk);
    }
}
