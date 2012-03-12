using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3_DXEngine.Main.Interfaces;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_CoreComponents.Maths;
using S33M3_CoreComponents.Maths.Graphics;
using S33M3_Resources.Structs;
using S33M3_DXEngine.Debug.Interfaces;

namespace S33M3_CoreComponents.Cameras.Interfaces
{
    public interface ICamera : IUpdatable, IDebugInfo
    {
        Matrix View { get; }
        Matrix Projection3D { get; }
        Matrix Projection2D { get; }
        Matrix ViewProjection3D { get; }
        Vector3D WorldPosition { get; }
        Quaternion Orientation { get; }
        Quaternion YAxisOrientation { get; }
        Viewport Viewport { get; set; }
        BoundingFrustum Frustum { get; }
        ICameraPlugin CameraPlugin { get; set; }
        CameraType CameraType { get; set; }

        /// <summary>
        /// Event that must be raised when the UpdateOrderId is changed
        /// </summary>
        event CameraUpdateOrder CameraUpdateOrderChanged;
    }
}
