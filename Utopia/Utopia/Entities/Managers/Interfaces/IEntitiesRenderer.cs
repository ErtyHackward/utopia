using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using S33M3DXEngine.Main.Interfaces;
using SharpDX.Direct3D11;
using Utopia.Components;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IEntitiesRenderer : IDrawable
    {
        IVisualVoxelEntityContainer VoxelEntityContainer { set; }
        void Initialize();
        void LoadContent(DeviceContext context);
        SharedFrameCB SharedFrameCB { get; set; } 
    }
}
