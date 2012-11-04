using System.Collections.Generic;
using Ninject;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Models;
using Utopia.Worlds.SkyDomes;
using UtopiaContent.Effects.Entities;
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

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders a specific equipped tool by an entity, this tool can be a texture block or an IVoxelEntity.
    /// It tool is an IVoxel entity then also draws the arm of the player model
    /// Works only in first person mode
    /// </summary>
    public class ToolRenderer : DrawableGameComponent
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
        private ITool _tool;
        //Use to draw textured cubes
        private HLSLCubeTool _cubeToolEffect;
        private IMeshFactory _milkShapeMeshfactory;
        private Mesh _cubeMeshBluePrint;
        private Mesh _cubeMesh;
        private VertexBuffer<VertexMesh> _cubeVb;
        private IndexBuffer<ushort> _cubeIb;
        private ShaderResourceView _cubeTextureView;
        private IDynamicEntity _dynamicEntity;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly PlayerEntityManager _playerManager;
        private bool _isPlayerCharacterOwner;
        private VisualVoxelModel _voxelModel;
        private VoxelModelInstance _voxelInstance;
        private HLSLVoxelModel _voxelModelEffect;
        private PlayerCharacter _player;
        private SingleArrayChunkContainer _chunkContainer;

        private FTSValue<Color3> _lightColor = new FTSValue<Color3>();

        #endregion


        public ITool Tool
        {
            get { return _tool; }
            set { _tool = value; ToolChange(); }
        }

        [Inject]
        public ISkyDome SkyDome { get; set; }

        [Inject]
        public SingleArrayChunkContainer ChunkContainer
        {
            get { return _chunkContainer; }
            set { _chunkContainer = value; }
        }

        public ToolRenderer(D3DEngine d3DEngine,
                            CameraManager<ICameraFocused> camManager,
                            IDynamicEntity dynamicEntity,
                            VoxelModelManager voxelModelManager, 
                            PlayerEntityManager playerManager)
        {
            _d3dEngine = d3DEngine;
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
            _camManager = camManager;
            _dynamicEntity = dynamicEntity;
            _voxelModelManager = voxelModelManager;
            _playerManager = playerManager;
            _isPlayerCharacterOwner = _dynamicEntity is PlayerCharacter;

            DrawOrders.UpdateIndex(0, 5000);
        }

        void EquipmentItemEquipped(object sender, Shared.Entities.Inventory.CharacterEquipmentEventArgs e)
        {
            if (e.EquippedItem != null)
            {
                Tool = e.EquippedItem.Item as ITool;
            }
            else
                Tool = null;
        }

        #region Public Methods
        public override void Update(GameTime timeSpend)
        {

        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            
            
            // update model color, get the cube where model is
            var block = _chunkContainer.GetCube(_playerManager.CameraWorldPosition);
            if (block.Id == 0)
            {
                // we take the max color
                var sunPart = (float)block.EmissiveColor.A / 255;
                var sunColor = SkyDome.SunColor * sunPart;
                var resultColor = Color3.Max(block.EmissiveColor.ToColor3(), sunColor);

                _lightColor.Value = resultColor;

                if (_lightColor.ValueInterp != _lightColor.Value)
                {
                    Color3.Lerp(ref _lightColor.ValueInterp, ref _lightColor.Value, elapsedTime / 100f, out _lightColor.ValueInterp);
                }
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            //No tool equipped or not in first person mode, render nothing !
            if (_tool == null) 
                return;
            
            if (!_isPlayerCharacterOwner || _camManager.ActiveCamera.CameraType != CameraType.FirstPerson) 
                return;

            context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);


            var screenPosition = Matrix.RotationY(MathHelper.Pi * 2) * Matrix.RotationX(MathHelper.Pi * 2f) *
                                 Matrix.Translation(1, -1, 0) *
                                 Matrix.Invert(_camManager.ActiveCamera.View_focused) * Matrix.Scaling(0.5f) *
                                 Matrix.Translation(_camManager.ActiveCamera.LookAt.ValueInterp);

            if (_renderingType == ToolRenderingType.Cube)
            {
                //Render First person view of the tool, only if the tool is used by the current playing person !
                _cubeToolEffect.Begin(context);
                _cubeToolEffect.CBPerDraw.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
                _cubeToolEffect.CBPerDraw.Values.Screen = Matrix.Transpose(screenPosition);
                _cubeToolEffect.CBPerDraw.Values.LightColor = _lightColor.ValueInterp;
                _cubeToolEffect.CBPerDraw.IsDirty = true;

                _cubeToolEffect.Apply(context);
                //Set the buffer to the device
                _cubeVb.SetToDevice(context, 0);
                _cubeIb.SetToDevice(context, 0);

                //Draw things here.
                context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
            }
            if (_renderingType == ToolRenderingType.Voxel && _voxelModel != null)
            {
                _voxelModelEffect.Begin(context);
                _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
                _voxelModelEffect.CBPerFrame.IsDirty = true;
                _voxelInstance.World = Matrix.Scaling(1f/16) * screenPosition;
                _voxelInstance.LightColor = _lightColor.ValueInterp;

                _voxelModel.Draw(context, _voxelModelEffect, _voxelInstance);
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
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, _d3dEngine.ImmediateContext, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTextureView);
            ToDispose(_cubeTextureView);
            //Create Vertex/Index Buffer to store the loaded cube mesh.
            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(_d3dEngine.Device, _cubeMeshBluePrint.Vertices.Length, VertexMesh.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(_d3dEngine.Device, _cubeMeshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB"));

            _cubeToolEffect = ToDispose(new HLSLCubeTool(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeToolEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeToolEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);

            _voxelModelEffect = ToDispose(new HLSLVoxelModel(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));

            _player = _dynamicEntity as PlayerCharacter;

            if (_player != null)
            {
                _player.Equipment.ItemEquipped += EquipmentItemEquipped;
                Tool = _player.Equipment.RightTool;
            }

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
                _voxelModel = _voxelModelManager.GetModel(voxelEntity.ModelName);

                if (_voxelModel != null)
                {
                    if (!_voxelModel.Initialized)
                    {
                        _voxelModel.BuildMesh();
                    }

                    _voxelInstance = _voxelModel.VoxelModel.CreateInstance();
                    _voxelInstance.SetState(_voxelModel.VoxelModel.GetMainState());
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
            var cubeProfile = RealmConfiguration.CubeProfiles[cube.CubeId];

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
        #endregion
    }
}
