using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Models;
using Utopia.Worlds.Chunks.ChunkLandscape;
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
        private SingleArrayChunkContainer _chunkContainer;
        private readonly VoxelModelManager _modelManager;
        private readonly InputsManager _inputsManager;
        private readonly PlayerEntityManager _entityManager;
        private ShaderResourceView _cubeTexture_View;
        public SharedFrameCB SharedFrameCB { get; set;}

        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();
        private FTSValue<Quaternion> _headRotation = new FTSValue<Quaternion>();
        private FTSValue<Quaternion> _bodyRotation = new FTSValue<Quaternion>();
        private FTSValue<Color3> _modelLight = new FTSValue<Color3>();

        private BoundingBox3D renderer;
        private IVisualEntityContainer _visualEntity;
        private HLSLVertexPositionColor _dummyEntityRenderer;
        private VisualVoxelModel _model;
        private VoxelModelInstance _playerModelInstance;
        private HLSLVoxelModel _voxelEffect;

        private bool _isWalking;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> VisualEntities { get; set; }
        public IVisualEntityContainer VisualEntity
        {
            get { return _visualEntity; }
            set { _visualEntity = value; SetUpRenderer(); }
        }

        public VoxelModelInstance ModelInstance { get { return _playerModelInstance; } }

        [Inject]
        public ISkyDome SkyDome { get; set; }

        [Inject]
        public PlayerEntityManager PlayerManager { get; set; }
        
        [Inject]
        public SingleArrayChunkContainer ChunkContainer
        {
            get { return _chunkContainer; }
            set { _chunkContainer = value; }
        }

        #endregion

        public PlayerEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters,
                                    VoxelModelManager modelManager,
                                    InputsManager inputsManager)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _modelManager = modelManager;
            _inputsManager = inputsManager;


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

            _voxelEffect = new HLSLVoxelModel(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            if (_model != null)
            {
                _model.BuildMesh();
            }

            if (_model != null)
                _playerModelInstance = _model.VoxelModel.CreateInstance();
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
                _voxelEffect.CBPerFrame.Values.LightIntensity = 1f;
                _voxelEffect.CBPerFrame.Values.LightDirection = SkyDome.LightDirection;
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply(context);

                _voxelEffect.CBPerModel.Values.LightColor = _modelLight.ValueInterp;
                _voxelEffect.CBPerModel.Values.World = Matrix.Transpose(Matrix.Scaling(1f / 16) * Matrix.Translation(_worldPosition.ValueInterp.AsVector3()));
                _voxelEffect.CBPerModel.IsDirty = true;

                _model.Draw(context, _voxelEffect, _playerModelInstance);
            }
            else
            {
                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled,
                                             DXStates.DepthStencils.DepthEnabled);
                renderer.Draw(context, _camManager.ActiveCamera);
            }
        }

        public void Update(GameTime timeSpend)
        {
            _worldPosition.BackUpValue();
            _worldPosition.Value = _visualEntity.VisualEntity.Position;


            var playerChar = (PlayerCharacter)VisualEntity.VisualEntity.Entity;

            _headRotation.BackUpValue();
            _headRotation.Value = playerChar.HeadRotation;

            _bodyRotation.BackUpValue();
            _bodyRotation.Value = playerChar.BodyRotation;
            
            

            var moveKeysPressed = ( _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward) ||
                                    _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Backward) ||
                                    _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeLeft) ||
                                    _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeRight));

            if (!_isWalking && moveKeysPressed)
            {
                _isWalking = true;
                if (_playerModelInstance.CanPlay("Walk"))
                {
                    _playerModelInstance.Play("Walk", true);
                }
            }

            if (_isWalking && !moveKeysPressed)
            {
                _isWalking = false;
                _playerModelInstance.Stop();
            }

            
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            Quaternion.Lerp(ref _headRotation.ValuePrev, ref _headRotation.Value, interpolationLd, out _headRotation.ValueInterp);
            Quaternion.Lerp(ref _bodyRotation.ValuePrev, ref _bodyRotation.Value, interpolationLd, out _bodyRotation.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            renderer.Update(_worldPosition.ValueInterp.AsVector3() + new Vector3(0, _visualEntity.VisualEntity.Entity.Size.Y/2.0f, 0), Vector3.One);

            if (_playerModelInstance != null)
            {
                _playerModelInstance.HeadRotation = _headRotation.ValueInterp;

                _playerModelInstance.Rotation = _bodyRotation.ValueInterp;

                // update model animation
                _playerModelInstance.Update(timePassed);

                // update model color, get the cube where model is
                var block = _chunkContainer.GetCube(_worldPosition.ValueInterp);
                if (block.Id == 0)
                {
                    // we take the max color
                    var sunPart = (float) block.EmissiveColor.A/255;
                    var sunColor = SkyDome.SunColor*sunPart;
                    var resultColor = Color3.Max(block.EmissiveColor.ToColor3(), sunColor);

                    _modelLight.Value = resultColor;
                    
                    if (_modelLight.ValueInterp != _modelLight.Value)
                    {
                        Color3.Lerp(ref _modelLight.ValueInterp, ref _modelLight.Value, timePassed/100f, out _modelLight.ValueInterp);
                    }
                }
            }

            
        }

        public override void BeforeDispose()
        {
        }
        #endregion
    }
}
