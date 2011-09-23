using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Cameras
{
    public interface ICameraPlugin
    {
        /// <summary>
        /// Must point to the real Position, not the interpolated one !
        /// </summary>
        Vector3D CameraWorldPosition { get; }

        /// <summary>
        /// Must point to the real Orientation, not the interpolated one !
        /// </summary>
        Quaternion CameraOrientation { get; }
    }

}
