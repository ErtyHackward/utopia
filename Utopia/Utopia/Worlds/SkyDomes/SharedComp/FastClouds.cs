using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths.Noises;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using S33M3Resources.VertexFormats;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Effects.Shared;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Worlds.GameClocks;
using UtopiaContent.Effects.Weather;
using S33M3DXEngine.Threading;
using Amib.Threading;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class FastClouds : DrawableGameComponent
    {
        // instance buffer structure
        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct VertexPosition2Cloud : IVertexType
        {
            public Vector2 Position;

            public VertexPosition2Cloud(Vector2 position)
            {
                Position = position;
            }

            public static readonly VertexDeclaration VertexDeclaration;

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get
                {
                    return VertexDeclaration;
                }
            }

            static VertexPosition2Cloud()
            {
                // !!! The VertexDeclaration must incorporate the Fixed vertex Part !!!!
                var elements = new[] { 
                    new InputElement("POSITION",  0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0), 
                    new InputElement("COLOR",     0, Format.R8G8B8A8_UNorm,  InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                    new InputElement("POSITION",  1, Format.R32G32_Float,    InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1)
                };

                VertexDeclaration = new VertexDeclaration(elements);
            }

        }

        private readonly D3DEngine _d3DEngine;
        private const float CloudHeight = 5f;
        private const float CloudBlockSize = 40f;
        private const float CloudGridSize = 64;

        private ByteColor _topFace, _side1Face, _side2Face, _bottomFace;
        private InstancedVertexBuffer<VertexPosition3Color, VertexPosition2Cloud> _instancedBuffer;
        private IndexBuffer<ushort> _indexBuffer;
        private HLSLFastClouds _effect;
        private readonly StaggingBackBuffer _solidBackBuffer;
        private readonly IClock _worldclock;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly CameraManager<ICameraFocused> _cameraManager;

        private SimplexNoise _noise;
        private SharedFrameCB _sharedCB;
        private float _brightness = 0.9f;
        private Vector2 _offset;
        private Vector2 _smallOffset;
        private Vector2 _cameraPrevious;

        private List<VertexPosition2Cloud> _clouds;
        private bool _newCloudGenerated;
        private IWorkItemResult _threadState;

        private int _cloudBlocksCount;

        /// <summary>
        /// Gets or sets wind speed and direction
        /// </summary>
        public Vector2 WindVector { get; set; }

        public FastClouds(D3DEngine engine, StaggingBackBuffer solidBackBuffer, IClock worldclock, WorldFocusManager worldFocusManager, CameraManager<ICameraFocused> cameraManager)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            _d3DEngine = engine;
            _solidBackBuffer = solidBackBuffer;
            _worldclock = worldclock;
            _worldFocusManager = worldFocusManager;
            _cameraManager = cameraManager;
            WindVector = new Vector2(2, 1);
        }

        public void LateInitialization(SharedFrameCB sharedCB)
        {
            _sharedCB = sharedCB;
        }

        private void FormClouds()
        {

            // move the big grid
            _offset -= new Vector2((int)(_smallOffset.X / CloudBlockSize), (int)(_smallOffset.Y / CloudBlockSize));

            for (var x = 0; x < CloudGridSize; x++)
            {
                for (var y = 0; y < CloudGridSize; y++)
                {
                    if (_noise.GetNoise2DValue(_offset.X + x, _offset.Y + y, 2, 0.25f).Value < 0.5f)
                    {
                        _clouds.Add(new VertexPosition2Cloud(new Vector2(x * CloudBlockSize, y * CloudBlockSize)));
                    }
                }
            }
            _newCloudGenerated = true;
        }

        public override void Initialize()
        {
            _noise = new SimplexNoise(new Random());
            _noise.SetParameters(0.075, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

            _effect = ToDispose(new HLSLFastClouds(_d3DEngine.Device, ClientSettings.EffectPack + @"Weather\FastClouds.hlsl", VertexPosition2Cloud.VertexDeclaration, _sharedCB.CBPerFrame));
            _effect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipPoint);

            _instancedBuffer = ToDispose(new InstancedVertexBuffer<VertexPosition3Color, VertexPosition2Cloud>(_d3DEngine.Device, VertexPosition2Cloud.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList));

            _indexBuffer = ToDispose(new IndexBuffer<ushort>(_d3DEngine.Device, 36, Format.R16_UInt, "_cloudIB", 10, ResourceUsage.Dynamic));

            _clouds = new List<VertexPosition2Cloud>();
            _newCloudGenerated = false;
        }

        public override void LoadContent(DeviceContext context)
        {
            _topFace    = new ByteColor(240, 240, 255, 200);
            _side1Face  = new ByteColor(230, 230, 255, 200);
            _side2Face  = new ByteColor(220, 220, 245, 200);
            _bottomFace = new ByteColor(205, 205, 230, 200);

            // create cloud block mesh

            var positions = new List<VertexPosition3Color>();
            var indices = new List<ushort>();
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

            for (ushort i = 0; i < 24; i+=4)
            {
                indices.AddRange(new [] { (ushort)(i + 2), i, (ushort)(i + 1), i, (ushort)(i + 2), (ushort)(i + 3) });
            }


            _indexBuffer.SetData(_d3DEngine.Device.ImmediateContext, indices.ToArray());

            _cameraPrevious = new Vector2((float)_cameraManager.ActiveCamera.WorldPosition.X, (float)_cameraManager.ActiveCamera.WorldPosition.Z);

            FormClouds();
        }

        public override void Update(GameTime timeSpent)
        {
            _brightness = _worldclock.ClockTime.SmartTimeInterpolation(0.2f);

            if ((Math.Abs(_smallOffset.X) > CloudBlockSize || Math.Abs(_smallOffset.Y) > CloudBlockSize)
                && (_threadState == null || _threadState.IsCompleted)
                && _newCloudGenerated == false
                )
            {
                // rebuild the grid in thread
                _threadState = SmartThread.ThreadPool.QueueWorkItem(FormClouds);
            }

            if (_newCloudGenerated)
            {
                if (Math.Abs((int)(_smallOffset.X / CloudBlockSize)) > 0)
                    _smallOffset.X = _smallOffset.X % CloudBlockSize;

                if (Math.Abs((int)(_smallOffset.Y / CloudBlockSize)) > 0)
                    _smallOffset.Y = _smallOffset.Y % CloudBlockSize;

                _instancedBuffer.SetInstancedData(_d3DEngine.Device.ImmediateContext, _clouds.ToArray());
                _cloudBlocksCount = _clouds.Count;
                _clouds.Clear();
                _newCloudGenerated = false;
            }

        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            var cameraCurrent = new Vector2((float)_cameraManager.ActiveCamera.WorldPosition.X, (float)_cameraManager.ActiveCamera.WorldPosition.Z);
            _smallOffset += _cameraPrevious - cameraCurrent;
            _cameraPrevious = cameraCurrent;

            _smallOffset.X += (float)elapsedTime / 1000 * WindVector.X;
            _smallOffset.Y += (float)elapsedTime / 1000 * WindVector.Y;
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);
            _effect.Begin(_d3DEngine.ImmediateContext);
            _effect.SolidBackBuffer.Value = _solidBackBuffer.SolidStaggingBackBuffer;

            var world = Matrix.Identity * Matrix.Translation(-(CloudGridSize / 2 * CloudBlockSize) + _smallOffset.X + (float)_cameraManager.ActiveCamera.WorldPosition.X, 140, -(CloudGridSize / 2 * CloudBlockSize) + _smallOffset.Y + (float)_cameraManager.ActiveCamera.WorldPosition.Z);
            
            _worldFocusManager.CenterTranslationMatrixOnFocus(ref world, ref world);

            

            _effect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _effect.CBPerDraw.Values.Brightness = _brightness;
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(_d3DEngine.ImmediateContext);

            //Set the buffers to the graphical card
            _instancedBuffer.SetToDevice(_d3DEngine.Device.ImmediateContext, 0);
            _indexBuffer.SetToDevice(_d3DEngine.Device.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.DrawIndexedInstanced(36, _cloudBlocksCount, 0, 0, 0);
        }
    }

    
}
