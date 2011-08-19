using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.Maths.Graphics;

namespace S33M3Engines.Cameras
{
    public interface ICamera : IUpdatableComponent, IDebugInfo
    {
        Matrix View { get; }
        Matrix Projection3D { get; }
        Matrix Projection2D { get; }
        Matrix ViewProjection3D { get; }
        DVector3 WorldPosition { get; set; }
        Vector3 LookAt { get; set; }
        Viewport Viewport { get; set; }
        BoundingFrustum Frustum { get; }
        ICameraPlugin CameraPlugin { get; set; }
    }
}
