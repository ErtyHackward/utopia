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
        void Activate(ShaderResourceView backbuffer);
        void Deactivate();
        void Render(SharpDX.Direct3D11.DeviceContext context);
    }
}
