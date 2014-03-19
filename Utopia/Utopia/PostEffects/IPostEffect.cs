using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.PostEffects
{
    public interface IPostEffect
    {
        string Name { get; set; }
        void Initialize(Device device);
        void Activate(ShaderResourceView backbuffer, PostEffectComponent parent);
        void RefreshBackBuffer(ShaderResourceView backbuffer);
        void Deactivate();
        void Render(SharpDX.Direct3D11.DeviceContext context);
        void FTSUpdate(GameTime timeSpent);
        void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime);
    }
}
