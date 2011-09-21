using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using S33M3Engines.D3D;
using UtopiaContent.Effects.Terran;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using S33M3Engines.StatesManager;
using SharpDX;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;

namespace Utopia.Entities.Renderer
{
    public class PlayerEntityRenderer : IEntitiesRenderer
    {
        #region Private variables
        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity { get; set; }
        #endregion

        public PlayerEntityRenderer(D3DEngine d3DEngine,
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
            _entityEffect = new HLSLTerran(_d3DEngine, @"Effects/Entities/DynamicEntity.hlsl", VertexCubeSolid.VertexDeclaration);
        }
        #endregion

        #region Public Methods
        public void Draw(int Index)
        {
            //If camera is first person Don't draw the body.
            if (_camManager.ActiveCamera.CameraType == CameraType.FirstPerson) return;

            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _entityEffect.Begin();

            _entityEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _entityEffect.CBPerFrame.IsDirty = true;

            Matrix world = _worldFocusManager.CenterOnFocus(ref VisualEntity.VisualEntity.World);
            world *= Matrix.Translation(5, 0, 0);

            _entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _entityEffect.CBPerDraw.IsDirty = true;
            _entityEffect.Apply();

            VisualEntity.VisualEntity.VertexBuffer.SetToDevice(0);
            _d3DEngine.Context.Draw(VisualEntity.VisualEntity.VertexBuffer.VertexCount, 0);
        }

        public void Update(ref GameTime timeSpent)
        {
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public void Dispose()
        {
            _entityEffect.Dispose();
        }
        #endregion

    }
}
