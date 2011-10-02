using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Renderer.Interfaces;
using S33M3Engines.Cameras;
using S33M3Engines;
using Utopia.Worlds.SkyDomes;
using S33M3Engines.WorldFocus;
using Utopia.Shared.World;
using Utopia.Entities.Voxel;
using SharpDX.Direct3D11;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Textures;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using SharpDX;
using SharpDX.Direct3D;
using S33M3Engines.Maths;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Renderer
{
    public class StaticSpriteEntityRenderer : IStaticSpriteEntityRenderer
    {
        #region Private variables
        private HLSLStaticEntitySprite _effectPointSprite;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _spriteTexture_View;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParameters;
        private VertexBuffer<VertexPointSprite> _vb;
        private VertexPointSprite[] _vertices; //Default to support a maximum of 50.000 elements !
        private int _verticeArrayMaxSize = 50000;
        private int _verticeCount;
        #endregion

        #region Public variables/properties
        #endregion

        public StaticSpriteEntityRenderer(D3DEngine d3dEngine,
                                    CameraManager camManager,
                                    WorldFocusManager worldFocusManager,
                                    ISkyDome skydome,
                                    VisualWorldParameters visualWorldParameters)
        {
            _d3dEngine = d3dEngine;
            _camManager = camManager;
            _skydome = skydome;
            _visualWorldParameters = visualWorldParameters;
            _worldFocusManager = worldFocusManager;
            Initialize();   
        }

        #region Private Methods
        private void Initialize()
        {
            _effectPointSprite = new HLSLStaticEntitySprite(_d3dEngine, @"Effects\Entities\StaticEntitySprite.hlsl", VertexPointSprite.VertexDeclaration);
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Sprites/", @"sp*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _spriteTexture_View);
            _effectPointSprite.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
            _effectPointSprite.DiffuseTexture.Value = _spriteTexture_View;
            _vertices = new VertexPointSprite[_verticeArrayMaxSize];
            _verticeCount = -1;
        }

        #endregion

        #region Public Methods
        public void Draw(int Index)
        {
            if (_vb == null || _vb.VertexCount == 0) return;

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effectPointSprite.Begin();

            _effectPointSprite.CBPerFrame.Values.WorldFocus = Matrix.Transpose(_worldFocusManager.CenterOnFocus(ref MMatrix.Identity));
            _effectPointSprite.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _effectPointSprite.CBPerFrame.Values.WindPower = 1; //TextureAnimationOffset.ValueInterp;
            _effectPointSprite.CBPerFrame.Values.SunColor = _skydome.SunColor;
            _effectPointSprite.CBPerFrame.Values.fogdist = ((_visualWorldParameters.WorldVisibleSize.X) / 2) - 48; ;
            _effectPointSprite.CBPerFrame.IsDirty = true;

            _effectPointSprite.Apply();

            _vb.SetToDevice(0);
            _d3dEngine.Context.Draw(_vb.VertexCount, 0);
        }

        public void BeginSpriteCollectionRefresh()
        {
            _verticeCount = 0;
        }

        public void AddPointSpriteVertex(ref VertexPointSprite spriteVertex)
        {
            _verticeCount++;
            if (_verticeCount >= _verticeArrayMaxSize) return;
            _vertices[_verticeCount] = spriteVertex;
        }

        public void Update(ref GameTime timeSpent)
        {
            //Udpate the Dynamic Vertex Buffer
            if (_verticeCount == 0) return;
            if (_vb == null)
            {
                _vb = new VertexBuffer<VertexPointSprite>(_d3dEngine, _verticeCount, VertexPointSprite.VertexDeclaration, PrimitiveTopology.PointList, "StaticSprite", ResourceUsage.Dynamic, 5);
                _vb.SetData(_vertices, 0, _verticeCount);
            }
            else
            {
                _vb.SetData(_vertices, 0, _verticeCount);
            }
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {

        }

        public void Dispose()
        {
            _vertices = null;
            _spriteTexture_View.Dispose();
            _effectPointSprite.Dispose();
        }
        #endregion
    }
}
