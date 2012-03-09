using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Settings;
using Utopia.Resources.Effects.Terran;
using Utopia.Effects.Shared;
using S33M3_DXEngine;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_CoreComponents.WorldFocus;
using S33M3_Resources.Struct.Vertex;
using S33M3_DXEngine.Textures;
using S33M3_DXEngine.Main;

namespace Utopia.Entities.Renderer
{
    public class DynamicEntityRenderer : Component, IEntitiesRenderer
    {
        #region Private variables
        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _cubeTexture_View;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParameters;
        private VisualVoxelEntity _entityToRender;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity { get; set; }
        public SharedFrameCB SharedFrameCB { get; set; }
        #endregion

        public DynamicEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    ISkyDome skydome,
                                    VisualWorldParameters visualWorldParameters)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _skydome = skydome;
            _visualWorldParameters = visualWorldParameters;
            _worldFocusManager = worldFocusManager;
        }

        #region Private Methods
        public void Initialize()
        {

        }

        public void LoadContent()
        {
            _entityEffect = ToDispose(new HLSLTerran(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities/DynamicEntity.hlsl", VertexCubeSolid.VertexDeclaration, SharedFrameCB.CBPerFrame));
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTexture_View);

            _entityEffect.TerraTexture.Value = ToDispose(_cubeTexture_View);
            _entityEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
        }
        #endregion

        #region Public Methods
        public void Draw(DeviceContext context, int index)
        {
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
            _entityEffect.Begin(context);

            for (int i = 0; i < VisualEntities.Count; i++)
            {
                _entityToRender = VisualEntities[i].VisualEntity;

                //Draw only the entities that are in Client view range
                if (_entityToRender.Position.X > _visualWorldParameters.WorldRange.Min.X &&
                   _entityToRender.Position.X <= _visualWorldParameters.WorldRange.Max.X &&
                   _entityToRender.Position.Z > _visualWorldParameters.WorldRange.Min.Z &&
                   _entityToRender.Position.Z <= _visualWorldParameters.WorldRange.Max.Z)
                {
                    Matrix world = _worldFocusManager.CenterOnFocus(ref _entityToRender.World);

                    _entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
                    _entityEffect.CBPerDraw.IsDirty = true;
                    _entityEffect.Apply(context);

                    //_entityToRender.VertexBuffer.SetToDevice(0);
                    //_d3DEngine.Context.Draw(VisualEntities[i].VisualEntity.VertexBuffer.VertexCount, 0);
                }
            }
        }

        public void Update(GameTime timeSpend)
        {
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd, ref long timePassed)
        {
        }
        #endregion
    }
}
