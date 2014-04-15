using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Storage;
using Utopia.Network;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public interface IChunkEntityImpactManager : ILandscapeManager, IDisposable
    {
        /// <summary>
        /// Occurs when a single block at the landscape is changed
        /// </summary>
        event EventHandler<LandscapeBlockReplacedEventArgs> BlockReplaced;
        event EventHandler<StaticEventArgs> StaticEntityAdd;
        event EventHandler<StaticEventArgs> StaticEntityRemoved;

        SingleArrayChunkContainer CubesHolder { get; set; }
        IWorldChunks WorldChunks { get; set; }
        bool ReplaceBlock(ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChange, BlockTag blockTag = null);
        bool ReplaceBlock(int cubeArrayIndex, ref Vector3I cubeCoordinates, byte replacementCubeId, bool isNetworkChange, BlockTag blockTag = null);
        void LateInitialization(ServerComponent server, SingleArrayChunkContainer cubesHolder, IWorldChunks worldChunks, IChunkStorageManager chunkStorageManager, ILightingManager lightManager, VisualWorldParameters wp);
        void CheckImpact(VisualChunkBase cubeChunk, Range3I cubeRange);

        void AddEntity(IStaticEntity entity, uint sourceDynamicId = 0);
        IStaticEntity RemoveEntity(EntityLink entity, uint sourceDynamicId = 0);

        void ProcessMessageEntityOut(ProtocolMessageEventArgs<EntityOutMessage> e);
        void ProcessMessageEntityIn(ProtocolMessageEventArgs<EntityInMessage> e);
    }
}
