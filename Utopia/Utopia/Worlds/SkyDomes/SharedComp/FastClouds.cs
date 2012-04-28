using System;
using System.Collections.Generic;
using S33M3CoreComponents.Maths.Noises;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using S33M3Resources.VertexFormats;
using SharpDX;
using Utopia.Components;
using Utopia.Effects.Shared;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using UtopiaContent.Effects.Weather;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class FastClouds : DrawableGameComponent
    {
        private readonly D3DEngine _d3DEngine;
        private const float CloudHeight = 3f;
        private const float CloudBlockSize = 40f;

        private ByteColor _topFace, _side1Face, _side2Face, _bottomFace;
        private InstancedVertexBuffer<VertexPosition3Color, VertexPosition2> _instancedBuffer;
        private HLSLClouds3D _effect;
        private StaggingBackBuffer _solidBackBuffer;

        private SimplexNoise _noise;
        private SharedFrameCB _sharedCB;
        private float _brightness = 0.9f;

        private int _cloudBlocksCount;

        /// <summary>
        /// Gets or sets wind speed and direction
        /// </summary>
        public Vector2 WindVector { get; set; }

        public FastClouds(D3DEngine engine, StaggingBackBuffer solidBackBuffer)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            _d3DEngine = engine;
            _solidBackBuffer = solidBackBuffer;
        }

        public override void Initialize()
        {
            _noise = new SimplexNoise(new Random());
            _noise.SetParameters(0.075, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

            _effect = ToDispose(new HLSLClouds3D(_d3DEngine.Device, ClientSettings.EffectPack + @"Weather\Clouds3D.hlsl", _sharedCB.CBPerFrame));
            _effect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipPoint);

            _instancedBuffer = ToDispose(new InstancedVertexBuffer<VertexPosition3Color, VertexPosition2>(_d3DEngine.Device, VertexPosition2.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList));
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _topFace    = new ByteColor(240, 240, 255, 200);
            _side1Face  = new ByteColor(230, 230, 255, 200);
            _side2Face  = new ByteColor(220, 220, 245, 200);
            _bottomFace = new ByteColor(205, 205, 230, 200);

            // create cloud block mesh

            var positions = new List<VertexPosition3Color>();

            var rx = CloudBlockSize / 2;
            var ry = CloudHeight;
            var rz = CloudBlockSize / 2;
            
            // top
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, -rz), _topFace));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, rz), _topFace));
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, rz), _topFace));
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, -rz), _topFace));

            // back
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, -rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, -rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, -rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, -rz), _side1Face));

            // right
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, -rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, -rz), _side2Face));

            // front
            positions.Add(new VertexPosition3Color(new Vector3(rx, ry, rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, rz), _side1Face));
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, rz), _side1Face));

            // left
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, ry, -rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, -rz), _side2Face));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, rz), _side2Face));

            // bottom
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, rz), _bottomFace));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, rz), _bottomFace));
            positions.Add(new VertexPosition3Color(new Vector3(-rx, -ry, -rz), _bottomFace));
            positions.Add(new VertexPosition3Color(new Vector3(rx, -ry, -rz), _bottomFace));

            _instancedBuffer.SetFixedData(positions.ToArray()); 
        }

        public override void Update(GameTime timeSpent)
        {
            
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);
            _effect.Begin(_d3DEngine.ImmediateContext);
            _effect.SolidBackBuffer.Value = _solidBackBuffer.SolidStaggingBackBuffer;

            var world = Matrix.Identity;

            //_worldFocusManager.CenterTranslationMatrixOnFocus(ref world, ref world);

            _effect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(_d3DEngine.ImmediateContext);

            //Set the buffer to the graphical card
            _instancedBuffer.SetToDevice(_d3DEngine.Device.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.DrawInstanced(24, _cloudBlocksCount, 0, 0);
        }
    }
}
