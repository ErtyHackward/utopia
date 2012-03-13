using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3CoreComponents.Cameras.Interfaces
{
    public interface ICameraFocused : ICamera
    {
        Matrix ViewProjection3D_focused { get; }
        Matrix View_focused { get; }
    }
}
