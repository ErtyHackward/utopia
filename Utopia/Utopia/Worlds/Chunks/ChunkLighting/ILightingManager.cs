﻿using System;
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

        void CreateChunkLightSources(VisualChunk chunk, bool Async);
        void PropagateChunkLightSources(VisualChunk chunk, bool Async);
        void CreateLightSources(ref Range<int> cubeRange);
        void PropagateLightSources(ref Range<int> cubeRange, bool borderAsLightSource = false);
    }
}
