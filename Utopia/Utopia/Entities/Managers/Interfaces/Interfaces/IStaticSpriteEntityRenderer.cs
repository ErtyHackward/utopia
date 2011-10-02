using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IStaticSpriteEntityRenderer : IDrawable
    {
        void AddPointSpriteVertex(ref VertexPointSprite spriteVertex);
        void BeginSpriteCollectionRefresh();
    }
}
