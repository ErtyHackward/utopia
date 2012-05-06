using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using System.Runtime.InteropServices;
using S33M3DXEngine;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3CoreComponents.Cameras;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras.Interfaces;

namespace Samples.GameComp
{
    /// <summary>
    /// The aime of this component, is to compute the various variables that are shared for the entire Frame.
    /// And store their values in a specific Constant Buffer ready to be re-used by other shaders
    /// </summary>
    public class SharedFrameCB : DrawableGameComponent
    {
        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;   //64 (4*4 float)
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;


        private D3DEngine _engine;
        private CameraManager<ICamera> _cameraManager;


        public SharedFrameCB(D3DEngine engine,
                             CameraManager<ICamera> cameraManager)
        {
            _engine = engine;
            _cameraManager = cameraManager;

            DrawOrders.UpdateIndex(0, 0); //Force the Draw call order to maximum priority, because its draw call must be done before everything else ! (As its used by all other components)

            CBPerFrame = ToDispose(new CBuffer<CBPerFrame_Struct>(_engine.Device, "PerFrameShared"));
        }

        public override void Draw(DeviceContext context, int index)
        {
            CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
            CBPerFrame.IsDirty = true;

            CBPerFrame.Update(context); //Send updated data to Graphical Card
        }
    }
}
