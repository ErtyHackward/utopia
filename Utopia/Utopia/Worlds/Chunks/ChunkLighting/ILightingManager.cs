using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Chunks.ChunkLighting
{
    public interface ILightingManager
    {
        void CreateLightSources(VisualChunk chunk, bool Async);
        void PropagateLightSources(VisualChunk chunk, bool Async);
    }
}
