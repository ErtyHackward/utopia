using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Maths;

namespace S33M3Engines.Cameras
{
    public interface ICameraPlugin
    {
        DVector3 CameraWorldPosition { get; }
        Quaternion CameraOrientation { get; }
    }

}
