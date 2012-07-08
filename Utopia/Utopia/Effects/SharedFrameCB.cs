﻿using System;
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
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;
using Utopia.Components;

namespace Utopia.Effects.Shared
{
    /// <summary>
    /// What is this?
    /// </summary>
    public class SharedFrameCB: DrawableGameComponent
    {
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;   //64 (4*4 float)
            [FieldOffset(64)]
            public Vector3 SunColor;        //12 (3 float)
            [FieldOffset(76)]
            public float fogdist;           //4 (float)
            [FieldOffset(80)]
            public Vector2 BackBufferSize;
            [FieldOffset(88)]
            public Vector2 Various;
        }

        private D3DEngine _engine;
        private CameraManager<ICameraFocused> _cameraManager;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParam;
        private PlayerEntityManager _playerManager;
        private StaggingBackBuffer _backBuffer;
        private float _animationValue = 0.0f;
        private float _animationSpeed = 0.0005f;

        public CBuffer<CBPerFrame_Struct> CBPerFrame;

        public SharedFrameCB(D3DEngine engine,
                             CameraManager<ICameraFocused> cameraManager,
                             ISkyDome skydome,
                             VisualWorldParameters visualWorldParam,
                             PlayerEntityManager playerManager,
                             [Named("PlayerEntityRenderer")] IEntitiesRenderer playerEntityRenderer,
                             StaggingBackBuffer backBuffer)
            
        {
            _engine = engine;
            _cameraManager = cameraManager;
            _skydome = skydome;
            _visualWorldParam = visualWorldParam;
            _playerManager = playerManager;
            _backBuffer = backBuffer;

            //Self Injecting to avoid Cyclical problem
            playerEntityRenderer.SharedFrameCB = this;

            DrawOrders.UpdateIndex(0, 0);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_engine.Device, "PerFrame");
        }

        void _backBuffer_BackBufferResized(object sender, EventArgs e)
        {

        }

        public override void Draw(DeviceContext context, int index)
        {
            CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D_focused);
            if (_playerManager.IsHeadInsideWater) CBPerFrame.Values.SunColor = new Vector3(_skydome.SunColor.X / 3, _skydome.SunColor.Y / 3, _skydome.SunColor.Z);
            else CBPerFrame.Values.SunColor = _skydome.SunColor;
            CBPerFrame.Values.fogdist = ((_visualWorldParam.WorldVisibleSize.X) / 2) - 48;
            CBPerFrame.Values.BackBufferSize = _backBuffer.SolidStaggingBackBufferSize;
            CBPerFrame.Values.Various.X = _playerManager.IsHeadInsideWater ? 1.0f : 0.0f;
            CBPerFrame.Values.Various.Y = _animationValue; //Asign animation Value (From 0 => 1 in loop);
            CBPerFrame.IsDirty = false;

            CBPerFrame.Update(context); //Send updated data to Graphical Card
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
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

