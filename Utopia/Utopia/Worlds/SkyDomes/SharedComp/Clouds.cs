using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Buffers;
using SharpDX;
using S33M3Engines.Struct.Vertex;
using UtopiaContent.Effects.Weather;
using SharpDX.Direct3D11;
using Utopia.Settings;
using S33M3Engines.Maths;
using Utopia.Worlds.Weather;
using Utopia.Shared.World;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using S33M3Engines;
using S33M3Engines.Cameras;
using Utopia.Shared.Chunks;
using S33M3Engines.Shared.Math;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class Clouds : DrawableGameComponent
    {
        #region Private Variables
        private D3DEngine _d3dEngine;
        private ShaderResourceView _cloudMap;
        private HLSLClouds2D _cloudEffect2D;
        private VertexBuffer<VertexCubeCloud> _cloudVB2D;
        private IndexBuffer<ushort> _cloudIB;
        private IWeather _weather;
        private WorldParameters _worldParam;
        private Location3<int> _visibleWorldSize;
        private int _nbrLayer;
        private int _cloudThicknes = 3;
        private Matrix _world = Matrix.Identity;
        private Vector2 _uvOffset = new Vector2();
        private float _moveOffset;
        private DVector3 _previousCameraPosition;
        private CameraManager _camManager;
        #endregion

        #region Public properties
        #endregion
        public Clouds(D3DEngine d3dEngine, CameraManager camManager , IWeather weather, WorldParameters worldParam)
        {
            _d3dEngine = d3dEngine;
            _worldParam = worldParam;
            _weather = weather;
            _camManager = camManager;

            _visibleWorldSize = new Location3<int>()
            {
                X = AbstractChunk.ChunkSize.X * worldParam.WorldChunkSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * worldParam.WorldChunkSize.Z,
            };
        }

        #region Public methods
        public override void Initialize()
        {
            _nbrLayer = ClientSettings.Current.Settings.GraphicalParameters.CloudsLayers;
            _cloudMap = ShaderResourceView.FromFile(_d3dEngine.Device, @"Textures\Weather\clouds.png");
            Init2D();
        }

        public override void Update(ref GameTime TimeSpend)
        {
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        public override void Draw()
        {
            if (_camManager.ActiveCamera.WorldPosition.Y > 500) return;
            //Matrix.Translation((float)_game.ActivCamera.WorldPosition.X, 0, (float)_game.ActivCamera.WorldPosition.Z, out _world);
            //MathHelper.CenterOnFocus(ref _world, ref _world, ref _game.WorldFocus);
            _world.M42 = -(float)_camManager.ActiveCamera.WorldPosition.Y;

            if (_previousCameraPosition.X == 0 && _previousCameraPosition.Y == 0 && _previousCameraPosition.Z == 0) _previousCameraPosition = _camManager.ActiveCamera.WorldPosition;

            double XOffsetCameraMove = _camManager.ActiveCamera.WorldPosition.X - _previousCameraPosition.X;
            double ZOffsetCameraMove = _camManager.ActiveCamera.WorldPosition.Z - _previousCameraPosition.Z;

            _uvOffset.X += ((float)XOffsetCameraMove / _moveOffset) + (_weather.Wind.WindFlow.X / 50000);
            _uvOffset.Y += ((float)ZOffsetCameraMove / _moveOffset) + (_weather.Wind.WindFlow.Z / 50000);

            //if (_uvOffset.X > 1 || _uvOffset.X < -1) _uvOffset.X = 0;
            //if (_uvOffset.Y > 1 || _uvOffset.Y < -1) _uvOffset.Y = 0;

            _previousCameraPosition = _camManager.ActiveCamera.WorldPosition;


            Draw2D();

        }

        public override void Dispose()
        {
            _cloudMap.Dispose();
            _cloudEffect2D.Dispose();
            _cloudIB.Dispose();
            _cloudVB2D.Dispose();

        }
        #endregion

        #region private methods
        private void Init2D()
        {
            _cloudEffect2D = new HLSLClouds2D(_d3dEngine, @"Effects\Weather\Clouds2D.hlsl", VertexCubeCloud.VertexDeclaration);

            _cloudEffect2D.CloudTexture.Value = _cloudMap;
            _cloudEffect2D.cloudSampler.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipPoint);

            VertexCubeCloud[] _vb = new VertexCubeCloud[4 * _nbrLayer];
            ushort[] _ib = new ushort[6 * _nbrLayer];

            Vector3 Posi;
            Vector2 TextCoord;
            int nbrVertex = 0;
            int nbrIndices = 0;
            int MeshVertexOffset = 0;
            float YlayerPosi;

            float DeltaPositionMapping = 0.30f;
            _moveOffset = _visibleWorldSize.X * (_visibleWorldSize.X / ((1 - (2 * DeltaPositionMapping)) * 100));

            var t = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexCubeCloud));


            for (int i = 0; i < _nbrLayer; i++)
            {
                YlayerPosi = -i * (_cloudThicknes / (float)_nbrLayer);

                //Create indices
                TextCoord = new Vector2(DeltaPositionMapping, 1 - DeltaPositionMapping);
                Posi = new Vector3(-_visibleWorldSize.X * 2, 120 + YlayerPosi, _visibleWorldSize.Z * 2);
                _vb[nbrVertex] = new VertexCubeCloud(ref Posi, ref TextCoord, (byte)YlayerPosi);
                nbrVertex++;

                TextCoord = new Vector2(DeltaPositionMapping, DeltaPositionMapping);
                Posi = new Vector3(-_visibleWorldSize.X * 2, 120 + YlayerPosi, -_visibleWorldSize.Z * 2);
                _vb[nbrVertex] = new VertexCubeCloud(ref Posi, ref TextCoord, (byte)YlayerPosi);
                nbrVertex++;

                TextCoord = new Vector2(1 - DeltaPositionMapping, 1 - DeltaPositionMapping);
                Posi = new Vector3(_visibleWorldSize.X * 2, 120 + YlayerPosi, _visibleWorldSize.Z * 2);
                _vb[nbrVertex] = new VertexCubeCloud(ref Posi, ref TextCoord, (byte)YlayerPosi);
                nbrVertex++;

                TextCoord = new Vector2(1 - DeltaPositionMapping, DeltaPositionMapping);
                Posi = new Vector3(_visibleWorldSize.X * 2, 120 + YlayerPosi, -_visibleWorldSize.Z * 2);
                _vb[nbrVertex] = new VertexCubeCloud(ref Posi, ref TextCoord, (byte)YlayerPosi);
                nbrVertex++;

                _cloudVB2D = new VertexBuffer<VertexCubeCloud>(_d3dEngine, 4, VertexCubeCloud.VertexDeclaration, PrimitiveTopology.TriangleList);
                _cloudVB2D.SetData(_vb);

                //Create Vertices
                _ib[nbrIndices] = (ushort)(MeshVertexOffset); nbrIndices++;
                _ib[nbrIndices] = (ushort)(MeshVertexOffset + 2); nbrIndices++;
                _ib[nbrIndices] = (ushort)(MeshVertexOffset + 1); nbrIndices++;
                _ib[nbrIndices] = (ushort)(MeshVertexOffset + 3); nbrIndices++;
                _ib[nbrIndices] = (ushort)(MeshVertexOffset + 1); nbrIndices++;
                _ib[nbrIndices] = (ushort)(MeshVertexOffset + 2); nbrIndices++;

                MeshVertexOffset = nbrVertex;
            }

            _cloudIB = new IndexBuffer<ushort>(_d3dEngine, 6, SharpDX.DXGI.Format.R16_UInt);
            _cloudIB.SetData(_ib);
        }

        private void Draw2D()
        {
            _cloudEffect2D.Begin();
            _cloudEffect2D.CBPerDraw.Values.WorldViewProj = Matrix.Transpose(_world * _camManager.ActiveCamera.ViewProjection3D);
            _cloudEffect2D.CBPerDraw.Values.UVOffset = _uvOffset;
            if (_nbrLayer == 1) _cloudEffect2D.CBPerDraw.Values.nbrLayers = 2; else _cloudEffect2D.CBPerDraw.Values.nbrLayers = _nbrLayer;
            _cloudEffect2D.CBPerDraw.IsDirty = true;
            _cloudEffect2D.Apply();

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            if (_cloudVB2D != null)
            {
                _cloudVB2D.SetToDevice(0);
                _cloudIB.SetToDevice(0);
                _d3dEngine.Context.DrawIndexed(_cloudIB.IndicesCount, 0, 0);
            }
        }
        #endregion
    }
}
