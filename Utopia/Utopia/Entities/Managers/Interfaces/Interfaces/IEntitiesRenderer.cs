using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Entities.Voxel;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IEntitiesRenderer : IDrawable
    {
        List<IVisualEntityContainer> VisualEntities { get; set; }
        IVisualEntityContainer VisualEntity { get; set; }
    }
}
