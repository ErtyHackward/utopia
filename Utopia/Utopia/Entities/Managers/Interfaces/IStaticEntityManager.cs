using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IStaticEntityManager : IDrawableComponent
    {
        IWorldChunks WorldChunks { get; set; }
        bool StaticSpriteListDirty { get; set; }
    }
}
