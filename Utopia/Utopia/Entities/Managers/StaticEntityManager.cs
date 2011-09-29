﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Structs;
using SharpDX.Direct3D;
using S33M3Engines.Buffers;
using S33M3Engines;
using S33M3Engines.Textures;
using SharpDX.Direct3D11;
using S33M3Engines.StatesManager;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using System.Diagnostics;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Managers
{
    public class StaticEntityManager : DrawableGameComponent, IStaticEntityManager
    {
        #region Private variables
        private HLSLPointSprite3D _effectPointSprite;
        private D3DEngine _d3dEngine;
        private VertexBuffer<VertexPointSprite> _vb;
        private ShaderResourceView _srv;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        #endregion

        #region Public variables/properties
        #endregion

        public StaticEntityManager(D3DEngine d3dEngine,
                                   CameraManager camManager,
                                   WorldFocusManager worldFocusManager)
        {
            _d3dEngine = d3dEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
        }

        public override void Initialize()
        {
            offset = 0.03f;
            TextureAnimationOffset.Value = 0.00f;
            TextureAnimationOffset.ValuePrev = 0.00f;
        }

        public override void LoadContent()
        {
            _effectPointSprite = new HLSLPointSprite3D(_d3dEngine, @"D3D\Effects\Basics\PointSprite3D.hlsl", VertexPointSprite.VertexDeclaration);
            CreateBuffer();
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Sprites/", @"sp*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _srv);
            _effectPointSprite.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
            _effectPointSprite.DiffuseTexture.Value = _srv;
        }

        public override void Dispose()
        {
            _effectPointSprite.Dispose();
        }
        #region Private Methods

        private void CreateBuffer()
        {
            VertexPointSprite[] vertices = new VertexPointSprite[] { new VertexPointSprite((byte)0, new ByteVector4(1, 0, 0, 0)) };
            _vb = new VertexBuffer<VertexPointSprite>(_d3dEngine, vertices.Length, VertexPointSprite.VertexDeclaration, PrimitiveTopology.PointList, "PointSprite3D");
            _vb.SetData(vertices);
        }
        #endregion

        #region Public Methods
        private float _windpower;
        private long previousTime, currentTime, previousTimeTex, currentTimeTex;
        private long timeAccumulator, timeAccumulatorTex;
        private long FloodingSpeedTex = (long)(Stopwatch.Frequency / 30);
        float offset = 0.03f;
        FTSValue<float> TextureAnimationOffset = new FTSValue<float>();
        public override void Update(ref GameTime timeSpent)
        {
            //Start Tempo
            currentTimeTex = Stopwatch.GetTimestamp();
            timeAccumulatorTex += currentTimeTex - previousTimeTex;
            previousTimeTex = currentTimeTex;

            if (timeAccumulatorTex < FloodingSpeedTex) return;
            timeAccumulatorTex = 0;

            TextureAnimationOffset.BackUpValue();

            TextureAnimationOffset.Value += offset;
            if (TextureAnimationOffset.Value > 0.3)
            {
                offset *= -1;
            }
            if (TextureAnimationOffset.Value < -0.3)
            {
                offset *= -1;
            }
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            //if (interpolationLd < 0 || interpolationLd > 1) Console.WriteLine("ERROR");
            TextureAnimationOffset.ValueInterp = MathHelper.Lerp(TextureAnimationOffset.ValuePrev, TextureAnimationOffset.Value, interpolationLd);
            //Console.WriteLine(TextureAnimationOffset.Value + " " + TextureAnimationOffset.ValuePrev + " = " + TextureAnimationOffset.ValueInterp + "(" + interpolationLd + ")");
        }

        public override void Draw(int Index)
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effectPointSprite.Begin();

            _effectPointSprite.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _effectPointSprite.CBPerFrame.Values.WindPower = TextureAnimationOffset.ValueInterp;
            _effectPointSprite.CBPerFrame.IsDirty = true;

            Matrix test = Matrix.Translation(0,100,0);
            _worldFocusManager.CenterTranslationMatrixOnFocus(ref test, ref test);

            _effectPointSprite.CBPerDraw.Values.World = Matrix.Transpose(test);
            _effectPointSprite.CBPerDraw.IsDirty = true;

            _effectPointSprite.Apply();

            _vb.SetToDevice(0);
            _d3dEngine.Context.Draw(_vb.VertexCount, 0);
        }

        #endregion
    }
}
