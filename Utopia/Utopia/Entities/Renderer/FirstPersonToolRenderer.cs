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
        //Use to draw textured cubes
        private HLSLCubeTool _cubeToolEffect;
        private IMeshFactory _milkShapeMeshfactory;
        private Mesh _cubeMeshBluePrint;
        private Mesh _cubeMesh;
        private VertexBuffer<VertexMesh> _cubeVb;
        private IndexBuffer<ushort> _cubeIb;
        private ShaderResourceView _cubeTextureView;
        private readonly VoxelModelManager _voxelModelManager;
        private VisualVoxelModel _toolVoxelModel;
        private VisualVoxelModel _playerModel;
        private VoxelModelInstance _toolVoxelInstance;
        private HLSLVoxelModel _voxelModelEffect;
        private PlayerCharacter _player;
        private SingleArrayChunkContainer _chunkContainer;
        private VisualWorldParameters _visualWorldParameters;

        private VoxelModelPart _handVoxelModel;
        private VoxelModelPartState _handState;
        private VisualVoxelFrame _handVisualVoxelModel;

        private FTSValue<Color3> _lightColor = new FTSValue<Color3>();
        private ISkyDome _skyDome;

        private bool _animation = false;
        private bool _animationStated;
        private Quaternion _animationRotation;
        private Vector3 _animationOffset;
        

        #endregion

        #region Public Properties
        public IItem Tool
        {
            get { return _tool; }
            set { _tool = value; ToolChange(); }
        }
        #endregion

        public FirstPersonToolRenderer(D3DEngine d3DEngine,
                            CameraManager<ICameraFocused> camManager,
                            PlayerCharacter player,
                            VoxelModelManager voxelModelManager, 
                            VisualWorldParameters visualWorldParameters,
                            SingleArrayChunkContainer chunkContainer,
                            ISkyDome skyDome)
        {
            _d3dEngine = d3DEngine;
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
            _camManager = camManager;
            _player = player;
            _voxelModelManager = voxelModelManager;
            _visualWorldParameters = visualWorldParameters;
            _chunkContainer = chunkContainer;
            _skyDome = skyDome;

            _player.Equipment.ItemEquipped += EquipmentItemEquipped;
            _player.Use += _player_Use;

            _animationRotation = Quaternion.Identity;

            DrawOrders.UpdateIndex(0, 5000);

            this.IsDefferedLoadContent = true;
        }

        void _player_Use(object sender, Shared.Entities.Events.EntityUseEventArgs e)
        {
            if (e.Tool != null)
            {
                _animation = true;
                _animationStated = true;
            }
        }


        public override void BeforeDispose()
        {
            _player.Equipment.ItemEquipped -= EquipmentItemEquipped;
        }


        #region Public Methods
        public override void FTSUpdate(GameTime timeSpend)
        {
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            // update model color, get the cube where model is
            var block = _chunkContainer.GetCube(_player.Position);
            if (block.Id == 0)
            {
                // we take the max color
                var sunPart = (float)block.EmissiveColor.A / 255;
                var sunColor = _skyDome.SunColor * sunPart;
                var resultColor = Color3.Max(block.EmissiveColor.ToColor3(), sunColor);

                _lightColor.Value = resultColor;

                if (_lightColor.ValueInterp != _lightColor.Value)
                {
                    Color3.Lerp(ref _lightColor.ValueInterp, ref _lightColor.Value, elapsedTime / 100f, out _lightColor.ValueInterp);
                }
            }

            // play animation
            if (_animation)
            {
                const float speed = 0.021f;
                if (elapsedTime == 0) elapsedTime = 1;
                Quaternion finalRotation = Quaternion.RotationYawPitchRoll(0, MathHelper.PiOver2, 0);
                Vector3 finalOffset = new Vector3(0,0,2);

                if (_animationStated)
                {
                    Quaternion.Slerp(ref _animationRotation, ref finalRotation, elapsedTime * speed, out _animationRotation);
                    Vector3.Lerp(ref _animationOffset, ref finalOffset, elapsedTime * speed, out _animationOffset);

                    if (_animationRotation.EqualsEpsilon(finalRotation, 0.1f))
                        _animationStated = false;
                }
                else
                {
                    var identity = Quaternion.Identity;
                    var nullOffset = new Vector3();
                    Quaternion.Slerp(ref _animationRotation, ref identity, elapsedTime * speed, out _animationRotation);
                    Vector3.Lerp(ref _animationOffset, ref nullOffset, elapsedTime * speed, out _animationOffset);
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
            //Prepare Textured Block rendering when equiped ==============================================================
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);
            ArrayTexture.CreateTexture2DFromFiles(context.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTextureView);
            ToDispose(_cubeTextureView);
            //Create Vertex/Index Buffer to store the loaded cube mesh.
            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(context.Device, _cubeMeshBluePrint.Vertices.Length, VertexMesh.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(context.Device, _cubeMeshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB"));

            _cubeToolEffect = ToDispose(new HLSLCubeTool(context.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeToolEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeToolEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);

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
                PrepareCubeRendering((CubeResource)_tool);
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

        private void PrepareCubeRendering(CubeResource cube)
        {
            //Get the cube profile.
            var cubeProfile = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[cube.CubeId];

            //Prapare to creation a new mesh with the correct texture mapping ID
            var materialChangeMapping = new Dictionary<int, int>();
            materialChangeMapping[0] = cubeProfile.Tex_Back;    //Change the Back Texture Id
            materialChangeMapping[1] = cubeProfile.Tex_Front;   //Change the Front Texture Id
            materialChangeMapping[2] = cubeProfile.Tex_Bottom;  //Change the Bottom Texture Id
            materialChangeMapping[3] = cubeProfile.Tex_Top;     //Change the Top Texture Id
            materialChangeMapping[4] = cubeProfile.Tex_Left;    //Change the Left Texture Id
            materialChangeMapping[5] = cubeProfile.Tex_Right;   //Change the Right Texture Id

            //Create the cube Mesh from the blue Print one
            _cubeMesh = _cubeMeshBluePrint.Clone(materialChangeMapping);

            //Refresh the mesh data inside the buffers
            _cubeVb.SetData(_d3dEngine.ImmediateContext, _cubeMesh.Vertices);
            _cubeIb.SetData(_d3dEngine.ImmediateContext, _cubeMesh.Indices);
        }

        private void DrawingTool(DeviceContext context)
        {
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
                //Render First person view of the tool, only if the tool is used by the current playing person !
                _cubeToolEffect.Begin(context);
                _cubeToolEffect.CBPerDraw.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
                _cubeToolEffect.CBPerDraw.Values.Screen = Matrix.Transpose(Matrix.Translation(0.3f, 0.3f, 0.3f) *  screenPosition);
                _cubeToolEffect.CBPerDraw.Values.LightColor = _lightColor.ValueInterp;
                _cubeToolEffect.CBPerDraw.IsDirty = true;

                _cubeToolEffect.Apply(context);
                //Set the buffer to the device
                _cubeVb.SetToDevice(context, 0);
                _cubeIb.SetToDevice(context, 0);

                //Draw things here.
                context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
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
