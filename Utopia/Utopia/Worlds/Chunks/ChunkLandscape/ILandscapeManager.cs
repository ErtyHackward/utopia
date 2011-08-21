using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public interface ILandscapeManager : IDisposable
    {
        void CreateLandScape(VisualChunk chunk, bool Async);
    }
}
