using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public interface ILandscapeManager : IDisposable
    {
        WorldGenerator WorldGenerator { get; set; }
        void CreateLandScape(VisualChunk chunk, bool Async);
    }
}
