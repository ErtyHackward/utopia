using S33M3DXEngine.Main.Interfaces;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Entities.Voxel;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntitiesRenderer : IDrawable
    {
        IVisualVoxelEntityContainer VoxelEntityContainer { set; }
        void Initialize();
        void LoadContent(DeviceContext context);
        SharedFrameCB SharedFrameCB { get; set; } 
    }
}
