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

namespace Utopia.Entities.Renderer
{
    public class StaticSpriteEntityRenderer : IStaticSpriteEntityRenderer
    {
         #region Private variables
        private HLSLPointSprite3D _effectPointSprite;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _spriteTexture_View;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParameters;
        private VisualEntity _entityToRender;
        private VertexBuffer<VertexPointSprite> _vb;
        private List<VertexPointSprite> _vertices;
        #endregion

        #region Public variables/properties
        public List<VisualSpriteEntity> SpriteEntities { get; set; }
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
            _effectPointSprite = new HLSLPointSprite3D(_d3dEngine, @"D3D\Effects\Basics\PointSprite3D.hlsl", VertexPointSprite.VertexDeclaration);
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Sprites/", @"sp*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _spriteTexture_View);
            _effectPointSprite.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
            _effectPointSprite.DiffuseTexture.Value = _spriteTexture_View;
            _vertices = new List<VertexPointSprite>();
        }


        #endregion

        #region Public Methods
        public void Draw(int Index)
        {
            if (_vb == null || _vb.VertexCount == 0) return;

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effectPointSprite.Begin();

            Matrix test = Matrix.Identity;
            test = _worldFocusManager.CenterOnFocus(ref test);

            _effectPointSprite.CBPerFrame.Values.WorldFocus = Matrix.Transpose(test);
            _effectPointSprite.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _effectPointSprite.CBPerFrame.Values.WindPower = 1; //TextureAnimationOffset.ValueInterp;
            _effectPointSprite.CBPerFrame.IsDirty = true;

            _effectPointSprite.Apply();

            _vb.SetToDevice(0);
            _d3dEngine.Context.Draw(_vb.VertexCount, 0);
        }

        public void Update(ref GameTime timeSpent)
        {
            _vertices.Clear();
            for (int i = 0; i < SpriteEntities.Count; i++)
            {
                _vertices.Add(SpriteEntities[i].Vertex);
            }

            //Udpate the Dynamic Vertex Buffer
            if (_vertices.Count == 0) return;
            if (_vb == null)
            {
                _vb = new VertexBuffer<VertexPointSprite>(_d3dEngine, _vertices.Count, VertexPointSprite.VertexDeclaration, PrimitiveTopology.PointList, "StaticSprite", ResourceUsage.Dynamic, 10);
                _vb.SetData(_vertices.ToArray());
            }
            else
            {
                _vb.SetData(_vertices.ToArray());
            }
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {

        }

        public void Dispose()
        {
            _spriteTexture_View.Dispose();
            _effectPointSprite.Dispose();
        }
        #endregion
    }
}
