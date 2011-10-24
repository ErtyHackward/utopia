using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.Maths.Graphics;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Cameras
{
    public interface ICamera : IGameComponent, IUpdateableComponent, IDebugInfo
    {
        Matrix View_focused { get; }
        Matrix Projection3D { get; }
        Matrix Projection2D { get; }
        Matrix ViewProjection3D { get; }
        Matrix ViewProjection3D_focused { get; }
        Vector3D WorldPosition { get; }
        Quaternion Orientation { get; }
        Quaternion YAxisOrientation { get; }
        Viewport Viewport { get; set; }
        BoundingFrustum Frustum { get; }
        ICameraPlugin CameraPlugin { get; set; }
        CameraType CameraType { get; set; }
    }
}
