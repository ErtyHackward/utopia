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
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Textures;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.Main;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Shared.GameDXStates;

namespace Utopia.Entities.Renderer
{
    public class PlayerEntityRenderer : Component, IEntitiesRenderer
    {
        #region Private variables
        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _cubeTexture_View;
        public SharedFrameCB SharedFrameCB { get; set;} 
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity { get; set; }
        #endregion

        public PlayerEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
        }

        #region Private Methods
        public void Initialize()
        {
        }

        public void LoadContent(DeviceContext context)
        {
            _entityEffect = ToDispose(new HLSLTerran(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities/DynamicEntity.hlsl", VertexCubeSolid.VertexDeclaration, SharedFrameCB.CBPerFrame));
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTexture_View);

            _entityEffect.TerraTexture.Value = _cubeTexture_View;
            _entityEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
        }

        public void UnloadContent()
        {

        }

        #endregion

        #region Public Methods
        public void Draw(DeviceContext context, int index)
        {
            //If camera is first person Don't draw the body.
            if (_camManager.ActiveCamera.CameraType == CameraType.FirstPerson) return;

            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

            _entityEffect.Begin(context);

            //_entityEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            //_entityEffect.CBPerFrame.Values.SunColor = _skydome.SunColor;
            //_entityEffect.CBPerFrame.Values.fogdist = ((_visualWorldParameters.WorldVisibleSize.X) / 2) - 48;
            //_entityEffect.CBPerFrame.IsDirty = true;

            Matrix world = _worldFocusManager.CenterOnFocus(ref VisualEntity.VisualEntity.World);

            _entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _entityEffect.CBPerDraw.IsDirty = true;
            _entityEffect.Apply(context);

            //VisualEntity.VisualEntity.VertexBuffer.SetToDevice(0);
            //_d3DEngine.Context.Draw(VisualEntity.VisualEntity.VertexBuffer.VertexCount, 0);
        }

        public void Update(GameTime timeSpend)
        {
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
        }

        public override void Dispose()
        {
            _cubeTexture_View.Dispose();
            _entityEffect.Dispose();
            base.Dispose();
        }
        #endregion
    }
}
