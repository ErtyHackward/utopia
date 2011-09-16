using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtopiaContent.Effects.Terran;
using S33M3Engines;
using S33M3Engines.WorldFocus;
using S33M3Engines.Cameras;
using Utopia.Entities.Voxel;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.StatesManager;
using SharpDX;
using S33M3Engines.D3D;
using S33M3Engines.Textures;
using SharpDX.Direct3D11;

namespace Utopia.Entities.Renderer
{
    public class DefaultEntityRenderer : IEntitiesRenderer
    {
         #region Private variables
        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _cubeTexture_View;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity { get; set; }
        #endregion

        public DefaultEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager camManager,
                                    WorldFocusManager worldFocusManager)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            Initialize();   
        }

        #region Private Methods
        private void Initialize()
        {
            _entityEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, out _cubeTexture_View);

            _entityEffect.TerraTexture.Value = _cubeTexture_View;
            _entityEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
        }
        #endregion

        #region Public Methods
        public void Draw(int Index)
        {
            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _entityEffect.Begin();

            _entityEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _entityEffect.CBPerFrame.IsDirty = true;

            for (int i = 0; i < VisualEntities.Count; i++)
            {
                Matrix world = _worldFocusManager.CenterOnFocus(ref VisualEntities[i].VisualEntity.World);

                _entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
                _entityEffect.CBPerDraw.IsDirty = true;
                _entityEffect.Apply();

                VisualEntities[i].VisualEntity.VertexBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(VisualEntities[i].VisualEntity.VertexBuffer.VertexCount, 0);
            }
        }

        public void Update(ref GameTime timeSpent)
        {
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public void Dispose()
        {
            _cubeTexture_View.Dispose();
        }
        #endregion
    }
}
