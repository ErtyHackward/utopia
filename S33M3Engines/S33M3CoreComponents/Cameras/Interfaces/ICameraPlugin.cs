using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Cameras.Interfaces
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

        /// <summary>
        /// Must point to the real Orientation, not the interpolated one !
        /// </summary>
        Quaternion CameraYAxisOrientation { get; }

        /// <summary>
        /// Specify when this Camera Plugin is updateing its contains (The camera update must be done AFTER the Plugin !)
        /// </summary>
        int CameraUpdateOrder { get; }
    }

}
