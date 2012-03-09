using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks;
using S33M3_DXEngine.Main.Interfaces;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IStaticEntityManager : IDrawableComponent
    {
        IWorldChunks WorldChunks { get; set; }
        bool StaticSpriteListDirty { get; set; }
    }
}
