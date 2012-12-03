using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites3D.Interfaces
{
    public interface ISprite3DProcessor
    {
        void Init(DeviceContext context, ResourceUsage usage);
        void Begin();

        void SetData(DeviceContext context);
        void Set2DeviceAndDraw(DeviceContext context, ICamera camera);
    }
}
