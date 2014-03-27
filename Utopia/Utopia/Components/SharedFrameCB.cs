using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer.Interfaces;
using Ninject;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Shared.Settings;
using Utopia.Worlds.Weather;

namespace Utopia.Components
{
    /// <summary>
    /// Values that are "statics" for a single frame, these are stored inside a shadder constant buffer, and can be use by any shadder
    /// without the need to be reuploaded.
    /// </summary>
    public class SharedFrameCB: DrawableGameComponent
    {
        [StructLayout(LayoutKind.Explicit, Size = 256)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection_focused;   //64 (4*4 float)
            [FieldOffset(64)]
            public Color3 SunColor;                 //12 (3 float)
            [FieldOffset(76)]
            public float fogdist;                   //4 (float)
            [FieldOffset(80)]
            public Vector2 BackBufferSize;
            [FieldOffset(88)]
            public Vector2 Various;
            [FieldOffset(96)]
            public Matrix ViewProjection;           //64 (4*4 float)
            [FieldOffset(160)]
            public float fogType;
            [FieldOffset(164)]
            public Vector3 CameraWorldPosition;
            [FieldOffset(176)]
            public Matrix InvertedOrientation;
            [FieldOffset(240)]
            public Vector2 WeatherGlobalOffset;
            [FieldOffset(248)]
            public float TextureFrameAnimation;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;

        private D3DEngine _engine;
        private CameraManager<ICameraFocused> _cameraManager;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParam;
        private IPlayerManager _playerManager;
        private StaggingBackBuffer _backBuffer;
        private float _animationValue = 0.0f;
        private float _animationSpeed = 0.5f;
        private IWeather _weather;

        public SharedFrameCB(D3DEngine engine,
                             CameraManager<ICameraFocused> cameraManager,
                             ISkyDome skydome,
                             VisualWorldParameters visualWorldParam,
                             IPlayerManager playerManager,
                             IWeather weather,
                             [Named("SkyBuffer")] StaggingBackBuffer backBuffer)
            
        {
            _engine = engine;
            _cameraManager = cameraManager;
            _skydome = skydome;
            _visualWorldParam = visualWorldParam;
            _playerManager = playerManager;
            _backBuffer = backBuffer;
            _weather = weather;

            DrawOrders.UpdateIndex(0, 0);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_engine.Device, "PerFrame");
        }


        public override void Draw(DeviceContext context, int index)
        {
            CBPerFrame.Values.ViewProjection_focused = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D_focused);
            if (_playerManager.IsHeadInsideWater) CBPerFrame.Values.SunColor = new Color3(_skydome.SunColor.Red / 3, _skydome.SunColor.Green / 3, _skydome.SunColor.Blue);
            else CBPerFrame.Values.SunColor = _skydome.SunColor;
            CBPerFrame.Values.fogdist = ((_visualWorldParam.WorldVisibleSize.X) / 2) - 48;
            CBPerFrame.Values.BackBufferSize = _backBuffer.SolidStaggingBackBufferSize;
            CBPerFrame.Values.Various.X = _playerManager.IsHeadInsideWater ? 1.0f : 0.0f;
            CBPerFrame.Values.Various.Y = _animationValue; //Asign animation Value (From 0 => 1 in loop);
            CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
            CBPerFrame.Values.CameraWorldPosition = _cameraManager.ActiveCamera.WorldPosition.ValueInterp.AsVector3();
            CBPerFrame.Values.InvertedOrientation = Matrix.Transpose(Matrix.RotationQuaternion(Quaternion.Invert(_cameraManager.ActiveCamera.Orientation.ValueInterp)));

            CBPerFrame.Values.WeatherGlobalOffset.X = _weather.MoistureOffset;
            CBPerFrame.Values.WeatherGlobalOffset.Y = _weather.TemperatureOffset;

            switch (ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog)
	        {
                case "SkyFog":
                    CBPerFrame.Values.fogType = 0.0f;
                    break;
                case "SimpleFog":
                    CBPerFrame.Values.fogType = 1.0f;
                    break;
                case "NoFog":
                default:
                    CBPerFrame.Values.fogType = 2.0f;
                break;
	        }

            CBPerFrame.IsDirty = true;
            CBPerFrame.Update(context); //Send updated data to Graphical Card
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            CBPerFrame.Values.TextureFrameAnimation += 0.0250f; //1 FPS Default value
            if (CBPerFrame.Values.TextureFrameAnimation >= _visualWorldParam.CubeTextureManager.TexturesAnimationLCM) CBPerFrame.Values.TextureFrameAnimation = 0.0f;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _animationValue += (_animationSpeed * elapsedTime);

            while(_animationValue >= 1.0) _animationValue -= 1.0f;
        }

        public override void BeforeDispose()
        {
            CBPerFrame.Dispose();
        }
    }

}

