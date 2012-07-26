using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using S33M3_DXEngine.Main;
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

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Will render a specific equiped tool by an entity, this tool can be a texture block or a IVoxelEntity.
    /// It will give the possibility to "render" the tool as First person view, or Third person view.
    /// </summary>
    public class ToolRenderer : BaseComponent, IDrawable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private enum ToolRenderingType
        {
            Cube,
            Voxel
        }

        #region Private Variables
        private D3DEngine _d3DEngine;
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
        private Matrix _orthoProjection;
        private IDynamicEntity _dynamicEntity;
        private bool _isPlayerCharacterOwner;
        #endregion

        #region Public Properties
        public ITool Tool
        {
            get { return _tool; }
            set { _tool = value; ToolChange(); }
        }
        #endregion

        public ToolRenderer(D3DEngine d3DEngine,
                            CameraManager<ICameraFocused> camManager,
                            IDynamicEntity dynamicEntity)
        {
            _d3DEngine = d3DEngine;
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
            _camManager = camManager;
            _dynamicEntity = dynamicEntity;
            _isPlayerCharacterOwner = _dynamicEntity is PlayerCharacter;
            Initialize();
        }

        #region Public Methods
        public void Update(S33M3DXEngine.Main.GameTime timeSpend)
        {

        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {

        }

        public void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_tool == null) return; //No Tool equiped, render nothing !

            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

            if (_isPlayerCharacterOwner && _camManager.ActiveCamera.CameraType == CameraType.FirstPerson)
            {
                //Render First person view of the tool, only if the tool is used by the current playing person !
                _cubeToolEffect.Begin(context);
                _cubeToolEffect.CBPerDraw.Values.Projection = Matrix.Transpose(_orthoProjection);
                _cubeToolEffect.CBPerDraw.Values.Screen = Matrix.Transpose(Matrix.Scaling(130.0f) * Matrix.RotationY(MathHelper.Pi * 0.75f) * Matrix.RotationX(MathHelper.Pi * 0.75f) * Matrix.Translation(390.0f, -250.0f, 0.0f));// * Matrix.Translation(10.0f, (float)_camManager.ActiveCamera.WorldPosition.Value.Y, 10.0f));
                _cubeToolEffect.CBPerDraw.IsDirty = true;

                _cubeToolEffect.Apply(context);
                //Set the buffer to the device
                _cubeVb.SetToDevice(context, 0);
                _cubeIb.SetToDevice(context, 0);

                //Draw things here.
                context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
            }
            else 
            {
                //Render 3th person mode of the tool == attached to the IDynamicEntity "Hand" position
            }
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            //Prepare Textured Block rendering when equiped ==============================================================
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, _d3DEngine.ImmediateContext, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTextureView);
            ToDispose(_cubeTextureView);
            //Create Vertex/Index Buffer to store the loaded cube mesh.
            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(_d3DEngine.Device, _cubeMeshBluePrint.Vertices.Length, VertexMesh.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(_d3DEngine.Device, _cubeMeshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB"));

            _cubeToolEffect = ToDispose(new HLSLCubeTool(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeToolEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeToolEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
            _orthoProjection = Matrix.OrthoLH(_camManager.ActiveCamera.Viewport.Width, _camManager.ActiveCamera.Viewport.Height, _camManager.ActiveCamera.NearPlane, _camManager.ActiveCamera.FarPlane);

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
                _renderingType = ToolRenderingType.Voxel;
                logger.Info("Voxel Entity tool equiped : {0}", _tool.DisplayName);
            }
        }

        private void PrepareCubeRendering(CubeResource cube)
        {
            //Get the cube profile.
            CubeProfile cubeProfile = GameSystemSettings.Current.Settings.CubesProfile[cube.CubeId];

            //Prapare to creation a new mesh with the correct texture mapping ID
            Dictionary<int, int> MaterialChangeMapping = new Dictionary<int, int>();
            MaterialChangeMapping[0] = cubeProfile.Tex_Back;    //Change the Back Texture Id
            MaterialChangeMapping[1] = cubeProfile.Tex_Front;   //Change the Front Texture Id
            MaterialChangeMapping[2] = cubeProfile.Tex_Bottom;  //Change the Bottom Texture Id
            MaterialChangeMapping[3] = cubeProfile.Tex_Top;     //Change the Top Texture Id
            MaterialChangeMapping[4] = cubeProfile.Tex_Left;    //Change the Left Texture Id
            MaterialChangeMapping[5] = cubeProfile.Tex_Right;   //Change the Right Texture Id

            //Create the cube Mesh from the blue Print one
            _cubeMesh = _cubeMeshBluePrint.Clone(MaterialChangeMapping);

            //Refresh the mesh data inside the buffers
            _cubeVb.SetData(_d3DEngine.ImmediateContext, _cubeMesh.Vertices);
            _cubeIb.SetData(_d3DEngine.ImmediateContext, _cubeMesh.Indices);
        }
        #endregion
    }
}
