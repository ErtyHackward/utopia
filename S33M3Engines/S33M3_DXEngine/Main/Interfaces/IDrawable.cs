using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3_DXEngine.Main.Interfaces
{
    public interface IDrawable : IUpdatable
    {
        void Draw(DeviceContext context, int index);
    }
}
