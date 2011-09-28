using System;
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

namespace Utopia.Entities.Managers
{
    public class StaticEntityManager : DrawableGameComponent, IStaticEntityManager
    {
        #region Private variables
        private HLSLPointSprite3D _effectPointSprite;
        private D3DEngine _d3dEngine;
        private VertexBuffer<VertexPositionColor> _vb;
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

        }

        public override void LoadContent()
        {
            _effectPointSprite = new HLSLPointSprite3D(_d3dEngine, @"D3D\Effects\Basics\PointSprite3D.hlsl", VertexPositionColor.VertexDeclaration);
            CreateBuffer();
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _srv);
            _effectPointSprite.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
            _effectPointSprite.DiffuseTexture.Value = _srv;
        }

        public override void Dispose()
        {
            _effectPointSprite.Dispose();
        }
        #region Private Methods

        private void CreateBuffer()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[] { new VertexPositionColor(new Vector3(0, 100, 0), new Color(5,1,0,0)) };
            _vb = new VertexBuffer<VertexPositionColor>(_d3dEngine, vertices.Length, VertexPositionColor.VertexDeclaration, PrimitiveTopology.PointList, "PointSprite3D");
            _vb.SetData(vertices);
        }
        #endregion

        #region Public Methods
        public override void Update(ref GameTime timeSpent)
        {

        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public override void Draw(int Index)
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effectPointSprite.Begin();

            _effectPointSprite.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _effectPointSprite.CBPerFrame.IsDirty = true;

            Matrix test = Matrix.Identity;
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
