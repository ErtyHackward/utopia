using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main.Interfaces;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Maths.Graphics;
using S33M3Resources.Structs;
using S33M3DXEngine.Debug.Interfaces;

namespace S33M3CoreComponents.Cameras.Interfaces
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
        Vector3 LookAt { get; }

        /// <summary>
        /// Event that must be raised when the UpdateOrderId is changed
        /// </summary>
        event CameraUpdateOrder CameraUpdateOrderChanged;
    }
}
