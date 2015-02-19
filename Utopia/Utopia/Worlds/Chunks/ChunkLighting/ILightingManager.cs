using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Chunks.ChunkLighting
{
    public interface ILightingManager
    {
        byte LightPropagateSteps { get; }
        IWorldChunks2D WorldChunk { get; set; }

        void CreateChunkLightSources(VisualChunk2D chunk);
        void PropagateInnerChunkLightSources(VisualChunk2D chunk);
        void PropagateOutsideChunkLightSources(VisualChunk2D chunk);
        void CreateLightSources(ref Range3I cubeRange, byte maxHeight = 0);
        void PropagateLightSources(ref Range3I cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false, byte maxHeight = 0);
    }
}
