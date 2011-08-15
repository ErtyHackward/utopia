using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.GUI.NuclexUIPort.Visuals.Flat
{
    public interface IGraphicsDeviceService
    {
         SharpDX.Direct3D11.Device GraphicsDevice { get; set; }
    }

    public class GraphicsDeviceService : IGraphicsDeviceService
    {
         public SharpDX.Direct3D11.Device GraphicsDevice { get; set; }
    }
}
