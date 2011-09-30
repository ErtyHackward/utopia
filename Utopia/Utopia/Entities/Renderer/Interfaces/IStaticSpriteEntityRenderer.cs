using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.D3D;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IStaticSpriteEntityRenderer : IDrawable
    {
        VisualSpriteEntity[] SpriteEntities { get; set; }
        int SpriteEntitiesNbr { get; set; }
    }
}
