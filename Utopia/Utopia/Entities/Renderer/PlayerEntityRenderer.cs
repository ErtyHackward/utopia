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
using Utopia.Shared.Entities.Interfaces;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine.Buffers;
using Utopia.Shared.Entities.Concrete;
using S33M3CoreComponents.Maths;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Performs actual drawing of the player entity model
    /// </summary>
    public class PlayerEntityRenderer : BaseComponent, IEntitiesRenderer
    {
        #region Private variables
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private SingleArrayChunkContainer _chunkContainer;
        private readonly VoxelModelManager _modelManager;
        private readonly InputsManager _inputsManager;
        public SharedFrameCB SharedFrameCB { get; set;}

        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();
        private FTSValue<Quaternion> _headRotation = new FTSValue<Quaternion>();
        private FTSValue<Quaternion> _bodyRotation = new FTSValue<Quaternion>();
        private FTSValue<Color3> _modelLight = new FTSValue<Color3>();

        private VisualVoxelEntity _visualVoxelEntity;
        private PlayerCharacter _playerCharacter;
        private VisualVoxelModel _playerModel;
        private VoxelModelInstance _playerModelInstance;
        private HLSLVoxelModelInstanced _voxelEffect;
        private ISkyDome _skyDome;

        private ToolRenderer _rightHandToolRenderer;

        private bool _isWalking;
        #endregion

        #region Public variables/properties
        //Give the possibility to assign the VixualVoxelEntity to the renderer via the VoxelEntityContainer
        public IVisualVoxelEntityContainer VoxelEntityContainer
        {
            set
            {
                _visualVoxelEntity = value.VisualVoxelEntity; //Extract the VixualVoxelBody to store it. (== MUST be a PlayerCharacter)
                _playerCharacter.Equipment.ItemEquipped -= Equipment_ItemEquipped;
                _playerCharacter = (PlayerCharacter)_visualVoxelEntity.VoxelEntity;
                RegisterEquipementEvents(_playerCharacter);
                SetUpRenderer();
            }
        }

        public VoxelModelInstance ModelInstance { get { return _playerModelInstance; } }

        #endregion

        public PlayerEntityRenderer(D3DEngine d3DEngine,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters,
                                    VoxelModelManager modelManager,
                                    InputsManager inputsManager,
                                    ISkyDome skyDome,
                                    SingleArrayChunkContainer chunkContainer,
                                    PlayerCharacter playerCharacter
                                    )
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _modelManager = modelManager;
            _inputsManager = inputsManager;
            _skyDome = skyDome;
            _chunkContainer = chunkContainer;
            _playerCharacter = playerCharacter;

            RegisterEquipementEvents(_playerCharacter);
        }

        public override void BeforeDispose()
        {

        }

        #region Private Methods
        private void SetUpRenderer()
        {
            //Set the default world Position at the time the entity is binded to the renderer
            _worldPosition.Initialize(_visualVoxelEntity.VoxelEntity.Position);
        }

        private void RegisterEquipementEvents(PlayerCharacter player)
        {
            player.Equipment.ItemEquipped += Equipment_ItemEquipped;
        }

        private void Equipment_ItemEquipped(object sender, Shared.Entities.Inventory.CharacterEquipmentEventArgs e)
        {
            if (e.Slot == Shared.Entities.Inventory.EquipmentSlotType.RightHand)
            {
                _rightHandToolRenderer.Tool = (ITool)e.EquippedItem.Item;   //Equiped item MUST be a tool !
            }
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
        }

        public void LoadContent(DeviceContext context)
        {
            _rightHandToolRenderer = new ToolRenderer(_d3DEngine, _camManager, _playerCharacter, _modelManager);
            //Load the default Player model
            _playerModel = _modelManager.GetModel("Player");
            _voxelEffect = new HLSLVoxelModelInstanced(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VertexVoxelInstanced.VertexDeclaration);
            if (_playerModel != null)
            {
                _playerModel.BuildMesh();
                _playerModelInstance = _playerModel.VoxelModel.CreateInstance();
            }

            //Set current tool handled by character
            _rightHandToolRenderer.Tool = (ITool)_playerCharacter.Equipment.RightTool;
        }

        public void Update(GameTime timeSpend)
        {
            //Back Up the previous values that needs to be interpolated
            _worldPosition.BackUpValue();
            _headRotation.BackUpValue();
            _bodyRotation.BackUpValue();

            //Assign newly computed position/rotations value from character.
            _worldPosition.Value = _playerCharacter.Position;
            _headRotation.Value = _playerCharacter.HeadRotation;
            _bodyRotation.Value = _playerCharacter.BodyRotation;

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
            //Interpolated between frame movements
            Quaternion.Slerp(ref _headRotation.ValuePrev, ref _headRotation.Value, interpolationLd, out _headRotation.ValueInterp);
            Quaternion.Slerp(ref _bodyRotation.ValuePrev, ref _bodyRotation.Value, interpolationLd, out _bodyRotation.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            if (_playerModelInstance != null)
            {
                _playerModelInstance.HeadRotation = _headRotation.ValueInterp;

                _playerModelInstance.Rotation = _bodyRotation.ValueInterp;

                // update model animation
                _playerModelInstance.Interpolation(timePassed);

                // update model color, get the cube where model is
                var block = _chunkContainer.GetCube(_worldPosition.ValueInterp);
                if (block.Id == 0)
                {
                    // we take the max color
                    var sunPart = (float) block.EmissiveColor.A/255;
                    var sunColor = _skyDome.SunColor * sunPart;
                    var resultColor = Color3.Max(block.EmissiveColor.ToColor3(), sunColor);

                    _modelLight.Value = resultColor;
                    
                    if (_modelLight.ValueInterp != _modelLight.Value)
                    {
                        Color3.Lerp(ref _modelLight.ValueInterp, ref _modelLight.Value, timePassed/100f, out _modelLight.ValueInterp);
                    }
                }
            }
        }


        public void Draw(DeviceContext context, int index)
        {
            _rightHandToolRenderer.Draw(context, index);

            //If camera is first person Don't draw the body.
            if (_camManager.ActiveCamera.CameraType == CameraType.FirstPerson) return;

            //Applying Correct Render States
            if (_playerModel != null)
            {
                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

                _voxelEffect.Begin(context);
                _voxelEffect.CBPerFrame.Values.LightDirection = _skyDome.LightDirection;
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply(context);

                _playerModelInstance.World = Matrix.Scaling(1f / 16) * Matrix.Translation(_worldPosition.ValueInterp.AsVector3());
                _playerModelInstance.LightColor = _modelLight.ValueInterp;

                _playerModel.DrawInstanced(context, _voxelEffect, new[] { _playerModelInstance });
            }


            
        }
        #endregion
    }
}
