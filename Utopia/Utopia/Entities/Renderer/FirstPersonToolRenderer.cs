using System.Collections.Generic;
using Ninject;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Models;
using Utopia.Worlds.SkyDomes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3CoreComponents.Meshes;
using S33M3DXEngine.Buffers;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Textures;
using S33M3DXEngine;
using Utopia.Shared.Settings;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Entities.Dynamic;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Configuration;
using Utopia.Shared.World;
using Utopia.Resources.Effects.Entities;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders a specific equipped tool by an entity, this tool can be a texture block or an IVoxelEntity.
    /// If the tool is an IVoxel entity then also draws the arm of the player model
    /// Works only in first person mode
    /// </summary>
    public class FirstPersonToolRenderer : DrawableGameComponent
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private enum ToolRenderingType
        {
            Cube,
            Voxel
        }

        #region Private Variables
        private readonly D3DEngine _d3dEngine;
        private CameraManager<ICameraFocused> _camManager;
        private ToolRenderingType _renderingType;
        private IItem _tool;


        private readonly VoxelModelManager _voxelModelManager;
        private VisualVoxelModel _toolVoxelModel;
        private VisualVoxelModel _playerModel;
        private VoxelModelInstance _toolVoxelInstance;
        private HLSLVoxelModel _voxelModelEffect;
        private SingleArrayChunkContainer _chunkContainer;

        private VoxelModelPart _handVoxelModel;
        private VoxelModelPartState _handState;
        private VisualVoxelFrame _handVisualVoxelModel;

        private CubeRenderer _cubeRenderer;

        private FTSValue<Color3> _lightColor = new FTSValue<Color3>();
        private ISkyDome _skyDome;

        private bool _animation = false;
        private bool _animationStated;
        private Quaternion _animationRotation;
        private Vector3 _animationOffset;
        private PlayerCharacter _player;

        #endregion

        #region Public Properties
        public IItem Tool
        {
            get { return _tool; }
            set { _tool = value; ToolChange(); }
        }

        public PlayerCharacter PlayerCharacter
        {
            get { return _player; }
            set { 
                if (_player == value)
                    return;

                if (_player != null)
                {
                    _player.Equipment.ItemEquipped -= EquipmentItemEquipped;
                    _player.Use -= _player_Use;
                }

                _player = value;

                if (_player != null)
                {
                    _player.Equipment.ItemEquipped += EquipmentItemEquipped;
                    _player.Use += _player_Use;
                }
            }
        }

        #endregion

        public FirstPersonToolRenderer( 
                D3DEngine d3DEngine,
                CameraManager<ICameraFocused> camManager,
                PlayerEntityManager playerEntityManager,
                VoxelModelManager voxelModelManager, 
                VisualWorldParameters visualWorldParameters,
                SingleArrayChunkContainer chunkContainer,
                ISkyDome skyDome)
        {
            _d3dEngine = d3DEngine;
            
            _camManager = camManager;
            _voxelModelManager = voxelModelManager;
            _chunkContainer = chunkContainer;
            _skyDome = skyDome;
            
            PlayerCharacter = playerEntityManager.PlayerCharacter;
            playerEntityManager.PlayerEntityChanged += _player_PlayerEntityChanged;

            _cubeRenderer = new CubeRenderer(d3DEngine, visualWorldParameters);

            _animationRotation = Quaternion.Identity;

            DrawOrders.UpdateIndex(0, 5000);

            this.IsDefferedLoadContent = true;
        }

        void _player_PlayerEntityChanged(object sender, PlayerEntityChangedEventArgs e)
        {
            PlayerCharacter = e.PlayerCharacter;
        }

        void _player_Use(object sender, Shared.Entities.Events.EntityUseEventArgs e)
        {
            if (e.Tool != null && e.Tool != PlayerCharacter.HandTool)
            {
                _animation = true;
                _animationStated = true;
            }
        }
        
        public override void BeforeDispose()
        {
            // unsubscribe events
            PlayerCharacter = null;

            _cubeRenderer.Dispose();
        }


        #region Public Methods

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_player == null)
                return;

            // update model color, get the cube where model is
            var result = _chunkContainer.GetCube(_player.Position);
            if (result.IsValid && result.Cube.Id == WorldConfiguration.CubeId.Air)
            {
                // we take the max color
                var sunPart = (float)result.Cube.EmissiveColor.A / 255;
                var sunColor = _skyDome.SunColor * sunPart;
                var resultColor = Color3.Max(result.Cube.EmissiveColor.ToColor3(), sunColor);

                _lightColor.Value = resultColor;

                if (_lightColor.ValueInterp != _lightColor.Value)
                {
                    Color3.Lerp(ref _lightColor.ValueInterp, ref _lightColor.Value, elapsedTime * 10.0f, out _lightColor.ValueInterp);
                }
            }

            // play animation
            if (_animation)
            {
                const float speed = 21f;
                if (elapsedTime == 0.0f) elapsedTime = 0.0001f;
                Quaternion finalRotation = Quaternion.RotationYawPitchRoll(0, MathHelper.PiOver2, 0);
                Vector3 finalOffset = new Vector3(0,0,2);

                if (_animationStated)
                {
                    Quaternion.Slerp(ref _animationRotation, ref finalRotation, (float)elapsedTime * speed, out _animationRotation);
                    Vector3.Lerp(ref _animationOffset, ref finalOffset, (float)elapsedTime * speed, out _animationOffset);

                    if (_animationRotation.EqualsEpsilon(finalRotation, 0.1f))
                        _animationStated = false;
                }
                else
                {
                    var identity = Quaternion.Identity;
                    var nullOffset = new Vector3();
                    Quaternion.Slerp(ref _animationRotation, ref identity, (float)elapsedTime * speed, out _animationRotation);
                    Vector3.Lerp(ref _animationOffset, ref nullOffset, (float)elapsedTime * speed, out _animationOffset);
                }

                if (_animationRotation == Quaternion.Identity)
                    _animation = false;
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            if (_camManager.ActiveCamera.CameraType != CameraType.FirstPerson) return;

            //No tool equipped or not in first person mode, render nothing !
            if (_tool == null)
            {
                //DrawingArm(context);
            }
            else
            {
                DrawingTool(context);
            }
        }
        #endregion

        #region Private Methods
        public override void Initialize()
        {

        }

        public override void LoadContent(DeviceContext context)
        {
            _cubeRenderer.LoadContent(context);

            _voxelModelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));

            Tool = _player.Equipment.RightTool;

            _playerModel = _voxelModelManager.GetModel(_player.ModelName);
            if (_playerModel != null)
            {
                if (!_playerModel.Initialized)
                {
                    _playerModel.BuildMesh();
                }

                if (_player.ModelInstance == null)
                {
                    _player.ModelInstance = _playerModel.VoxelModel.CreateInstance();

                    //Get Voxel Arm of the character.
                    var armPartIndex = _player.ModelInstance.VoxelModel.GetArmIndex();
                    _handVoxelModel = _player.ModelInstance.VoxelModel.Parts[armPartIndex];
                    _handState = _player.ModelInstance.VoxelModel.GetMainState().PartsStates[armPartIndex];
                    _handVisualVoxelModel = _playerModel.VisualVoxelFrames[armPartIndex];
                }
            }
        }

        private void EquipmentItemEquipped(object sender, Shared.Entities.Inventory.CharacterEquipmentEventArgs e)
        {
            if (e.EquippedItem != null) 
                Tool = e.EquippedItem.Item;
            else 
                Tool = null;
        }

        //The tool has been changed !
        private void ToolChange()
        {
            //Is it a CubeResource ?
            if (_tool is CubeResource)
            {
                _renderingType = ToolRenderingType.Cube;
                _cubeRenderer.PrepareCubeRendering((CubeResource)_tool);
            }
            else if (_tool is IVoxelEntity) //A voxel Entity ?
            {
                logger.Info("Voxel Entity tool equipped : {0}", _tool.Name);

                var voxelEntity = _tool as IVoxelEntity;
                _renderingType = ToolRenderingType.Voxel;
                _toolVoxelModel = _voxelModelManager.GetModel(voxelEntity.ModelName);

                if (_toolVoxelModel != null)
                {
                    if (!_toolVoxelModel.Initialized)
                    {
                        _toolVoxelModel.BuildMesh();
                    }

                    _toolVoxelInstance = _toolVoxelModel.VoxelModel.CreateInstance();
                    _toolVoxelInstance.SetState(_toolVoxelModel.VoxelModel.GetMainState());
                }
                else
                {
                    logger.Info("Unable to display the voxel model");
                }
            }
        }



        private void DrawingTool(DeviceContext context)
        {
            if (_renderingType == ToolRenderingType.Voxel && (_toolVoxelInstance == null || _toolVoxelModel == null || !_toolVoxelModel.Initialized))
                return;

            context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            float scale;
            if (_renderingType == ToolRenderingType.Cube)
            {
                scale = 0.75f;
            }
            else
            {
                var voxelBB = _toolVoxelInstance.State.BoundingBox.GetSize();
                scale = MathHelper.Min(1.0f, 16 / MathHelper.Max(MathHelper.Max(voxelBB.X, voxelBB.Y), voxelBB.Z));
                scale *= 1.20f;
            }

            var screenPosition = Matrix.RotationQuaternion(_animationRotation) * Matrix.RotationX(MathHelper.Pi / 8) * Matrix.Scaling(scale) *
                     Matrix.Translation(new Vector3(1.0f, -1, -0.2f) + _animationOffset) *
                     Matrix.Invert(_camManager.ActiveCamera.View_focused) *
                     Matrix.Translation(_camManager.ActiveCamera.LookAt.ValueInterp * 1.8f);

            if (_renderingType == ToolRenderingType.Cube)
            {
                _cubeRenderer.Render(context, 
                    Matrix.Translation(0.3f, 0.3f, 0.3f) * screenPosition,
                    _camManager.ActiveCamera.ViewProjection3D_focused, 
                    _lightColor.ValueInterp
                    );
            }
            if (_renderingType == ToolRenderingType.Voxel && _toolVoxelModel != null)
            {
                _voxelModelEffect.Begin(context);
                _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
                _voxelModelEffect.CBPerFrame.IsDirty = true;
                _toolVoxelInstance.World = Matrix.Scaling(1f / 16) * screenPosition;
                _toolVoxelInstance.LightColor = _lightColor.ValueInterp;

                _toolVoxelModel.Draw(context, _voxelModelEffect, _toolVoxelInstance);
            }
        }

        private void DrawingArm(DeviceContext context)
        {
            //Prepare DirectX rendering pipeline
            context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            //Compute a "world matrix" for displaying the Arm
            var screenPosition = 
                Matrix.RotationX(MathHelper.Pi * 1.3f) * Matrix.RotationY(MathHelper.Pi * 0.01f) *  //Some rotations
                Matrix.Scaling(1.7f) * //Adjusting scale
                Matrix.Translation(0.1f, -1, 0) * //Translation
                Matrix.Invert(_camManager.ActiveCamera.View_focused) * //Keep the Arm On screen
                Matrix.Translation(_camManager.ActiveCamera.LookAt.ValueInterp * 2.5f); //Project the arm in the lookat Direction = Inside the screen

            //Prepare Effect
            _voxelModelEffect.Begin(context);
            _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _voxelModelEffect.CBPerFrame.IsDirty = true;

            //Assign model variables values
            _voxelModelEffect.CBPerModel.Values.World = Matrix.Transpose(Matrix.Scaling(1f / 16) * screenPosition);
            _voxelModelEffect.CBPerModel.Values.LightColor = _lightColor.ValueInterp;
            _voxelModelEffect.CBPerModel.IsDirty = true;

            //Don't use the GetTransform of the Part because its linked to the entity body, here we have only the arm to display.
            _voxelModelEffect.CBPerPart.Values.Transform = Matrix.Transpose(Matrix.Identity);
            _voxelModelEffect.CBPerPart.IsDirty = true;

            //Assign color mappings
            var colorMapping = _player.ModelInstance.VoxelModel.Frames[_handState.ActiveFrame].ColorMapping;
            if (colorMapping != null)
            {
                _voxelModelEffect.CBPerModel.Values.ColorMapping = colorMapping.BlockColors;
            }
            else
            {
                _voxelModelEffect.CBPerModel.Values.ColorMapping = _player.ModelInstance.VoxelModel.ColorMapping.BlockColors;
            }

            _voxelModelEffect.CBPerModel.IsDirty = true;

            //Assign buffers
            var vb = _handVisualVoxelModel.VertexBuffer;
            var ib = _handVisualVoxelModel.IndexBuffer;

            vb.SetToDevice(context, 0);
            ib.SetToDevice(context, 0);

            //Push Effect
            _voxelModelEffect.Apply(context);

            //Draw !
            context.DrawIndexed(ib.IndicesCount, 0, 0);
        }

        #endregion
    }
}
