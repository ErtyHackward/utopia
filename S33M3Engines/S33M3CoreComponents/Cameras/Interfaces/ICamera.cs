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
using S33M3Resources.Structs;
using S33M3DXEngine.Debug.Interfaces;

namespace S33M3CoreComponents.Cameras.Interfaces
{
    public interface ICamera : IUpdatable, IDebugInfo
    {
        Matrix Projection { get; }
        Matrix ViewProjection3D { get; }
        FTSValue<Vector3D> WorldPosition { get; }
        FTSValue<Quaternion> Orientation { get; }
        FTSValue<Quaternion> YAxisOrientation { get; }
        Viewport Viewport { get; set; }
        SimpleBoundingFrustum Frustum { get; }
        ICameraPlugin CameraPlugin { get; set; }
        CameraType CameraType { get; set; }
        FTSValue<Vector3> LookAt { get; }
        bool NewlyActivatedCamera { get; set; }
        float NearPlane { get; set; }
        float FarPlane { get; set; }

        /// <summary>
        /// Event that must be raised when the UpdateOrderId is changed
        /// </summary>
        event CameraUpdateOrder CameraUpdateOrderChanged;

        
    }
}
