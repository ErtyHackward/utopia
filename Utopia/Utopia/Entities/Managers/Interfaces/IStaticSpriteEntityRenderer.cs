using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Sprites;
using S33M3_DXEngine.Main.Interfaces;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IStaticSpriteEntityRenderer : IDrawable
    {
        void AddPointSpriteVertex(VisualSpriteEntity spriteVertex);
        void BeginSpriteCollectionRefresh();
        void EndSpriteCollectionRefresh();
    }
}
