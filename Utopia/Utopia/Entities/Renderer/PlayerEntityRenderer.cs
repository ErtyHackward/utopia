﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.Entities.Models;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Entities.Renderer.Interfaces;
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
using Utopia.Shared.Settings;
using S33M3_DXEngine.Main;
using Utopia.Resources.ModelComp;
using S33M3Resources.Effects.Basics;
using S33M3Resources.Structs;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Performs actual drawing of the player entity model
    /// </summary>
    public class PlayerEntityRenderer : BaseComponent, IEntitiesRenderer
    {
        #region Private variables
        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private readonly VoxelModelManager _modelManager;
        private ShaderResourceView _cubeTexture_View;
        public SharedFrameCB SharedFrameCB { get; set;}

        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();

        private BoundingBox3D renderer;
        private IVisualEntityContainer _visualEntity;
        private HLSLVertexPositionColor _dummyEntityRenderer;
        private VisualVoxelModel _model;
        private VoxelModelInstance _playerModelInstance;
        private HLSLVoxelModel _voxelEffect;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity
        {
            get { return _visualEntity; }
            set { _visualEntity = value; SetUpRenderer(); }
        }
        #endregion

        public PlayerEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters,
                                    VoxelModelManager modelManager)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _modelManager = modelManager;

            _dummyEntityRenderer = new HLSLVertexPositionColor(_d3DEngine.Device);

        }

        private void SetUpRenderer()
        {
            renderer = new BoundingBox3D(_d3DEngine, _worldFocusManager, _visualEntity.VisualEntity.Entity.Size, _dummyEntityRenderer, Colors.Red);
            _worldPosition = new FTSValue<Vector3D>(_visualEntity.VisualEntity.Position);
        }

        #region Private Methods
        public void Initialize()
        {

        }

        public void LoadContent(DeviceContext context)
        {
            _model = _modelManager.GetModel("Player");
            if (_model != null)
                _playerModelInstance = _model.VoxelModel.CreateInstance();

            _voxelEffect = new HLSLVoxelModel(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            if (_model != null)
            {
                _model.BuildMesh();
            }
            //_entityEffect = ToDispose(new HLSLTerran(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities/DynamicEntity.hlsl", VertexCubeSolid.VertexDeclaration, SharedFrameCB.CBPerFrame));
            //ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTexture_View);

            //_entityEffect.TerraTexture.Value = _cubeTexture_View;
            //_entityEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
            //_entityEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
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
            if (_model != null)
            {
                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

                _voxelEffect.Begin(context);
                _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(Matrix.Scaling(1f / 16) * Matrix.Translation(_worldPosition.ValueInterp.AsVector3()));
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(  _camManager.ActiveCamera.ViewProjection3D);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply(context);

                _model.Draw(context, _voxelEffect, _playerModelInstance);
            }
            else
            {
                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled,
                                             DXStates.DepthStencils.DepthEnabled);
                renderer.Draw(context, _camManager.ActiveCamera);
            }

            //_entityEffect.Begin(context);

            //Matrix world = _worldFocusManager.CenterOnFocus(ref VisualEntity.VisualEntity.World);

            //_entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            //_entityEffect.CBPerDraw.IsDirty = true;
            //_entityEffect.Apply(context);

            //VisualEntity.VisualEntity.VertexBuffer.SetToDevice(0);
            //context.Draw(VisualEntity.VisualEntity.VertexBuffer.VertexCount, 0);
        }

        public void Update(GameTime timeSpend)
        {
            _worldPosition.BackUpValue();
            _worldPosition.Value = _visualEntity.VisualEntity.Position;

            //_playerModelInstance.HeadRotation = _visualEntity.VisualEntity.Position
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            renderer.Update(_worldPosition.ValueInterp.AsVector3() + new Vector3(0, _visualEntity.VisualEntity.Entity.Size.Y / 2.0f, 0), Vector3.One);
        }

        public override void BeforeDispose()
        {
            //_cubeTexture_View.Dispose();
            //_entityEffect.Dispose();
        }
        #endregion
    }
}
