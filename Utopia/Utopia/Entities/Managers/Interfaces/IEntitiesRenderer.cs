using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using Utopia.Effects.Shared;
using S33M3_DXEngine.Main.Interfaces;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IEntitiesRenderer : IDrawable
    {
        List<IVisualEntityContainer> VisualEntities { get; set; }
        IVisualEntityContainer VisualEntity { get; set; }
        void Initialize();
        void LoadContent();
        SharedFrameCB SharedFrameCB { get; set; } 
    }
}
