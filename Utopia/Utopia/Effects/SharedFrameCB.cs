using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer.Interfaces;
using Ninject;
using S33M3_DXEngine.Main;
using S33M3_DXEngine;
using S33M3_CoreComponents.Cameras;
using S33M3_DXEngine.Effects.HLSLFramework;
using S33M3_CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;

namespace Utopia.Effects.Shared
{
    /// <summary>
    /// What is this?
    /// </summary>
    public class SharedFrameCB: DrawableGameComponent
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;   //64 (4*4 float)
            [FieldOffset(64)]
            public Vector3 SunColor;        //12 (3 float)
            [FieldOffset(76)]
            public float fogdist;           //4 (float)
        }

        private D3DEngine _engine;
        private CameraManager<ICameraFocused> _cameraManager;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParam;
        private PlayerEntityManager _playerManager;

        public CBuffer<CBPerFrame_Struct> CBPerFrame;

        public SharedFrameCB(D3DEngine engine,
                             CameraManager<ICameraFocused> cameraManager,
                             ISkyDome skydome,
                             VisualWorldParameters visualWorldParam,
                             PlayerEntityManager playerManager,
                             [Named("PlayerEntityRenderer")] IEntitiesRenderer playerEntityRenderer,
                             [Named("DefaultEntityRenderer")] IEntitiesRenderer dynamicEntityRenderer)
            
        {
            _engine = engine;
            _cameraManager = cameraManager;
            _skydome = skydome;
            _visualWorldParam = visualWorldParam;
            _playerManager = playerManager;

            //Self Injecting to avoid Cyclical problem
            playerEntityRenderer.SharedFrameCB = this;
            dynamicEntityRenderer.SharedFrameCB = this;

            DrawOrders.UpdateIndex(0, 0);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_engine.Device, "PerFrame");
        }

        public override void Draw(DeviceContext context, int index)
        {
            CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D_focused);
            if (_playerManager.IsHeadInsideWater) CBPerFrame.Values.SunColor = new Vector3(_skydome.SunColor.X / 3, _skydome.SunColor.Y / 3, _skydome.SunColor.Z);
            else CBPerFrame.Values.SunColor = _skydome.SunColor;
            CBPerFrame.Values.fogdist = ((_visualWorldParam.WorldVisibleSize.X) / 2) - 48;
            CBPerFrame.IsDirty = false;

            CBPerFrame.Update(context); //Send updated data to Graphical Card
        }

        public override void Dispose()
        {
            CBPerFrame.Dispose();
        }
    }

}
