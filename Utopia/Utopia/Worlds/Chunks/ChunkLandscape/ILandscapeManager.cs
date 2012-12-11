using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities;
using Utopia.Shared.World;
using Ninject;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public interface ILandscapeManager : IDisposable
    {
        //This property will be injected
        WorldGenerator WorldGenerator { get; set; }

        EntityFactory EntityFactory { get; set; }

        IWorldChunks WorldChunks { get; set; }

        void CreateLandScape(VisualChunk chunk);
    }
}
