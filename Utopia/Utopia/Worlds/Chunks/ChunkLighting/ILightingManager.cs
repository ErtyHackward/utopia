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
        IWorldChunks WorldChunk { get; set; }

        void CreateChunkLightSources(VisualChunk chunk);
        void PropagateInnerChunkLightSources(VisualChunk chunk);
        void PropagateOutsideChunkLightSources(VisualChunk chunk);
        void CreateLightSources(ref Range3I cubeRange, VisualChunk chunk);
        void PropagateLightSources(ref Range3I cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false);
    }
}
