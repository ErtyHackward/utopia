using System.Collections.Generic;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.World;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Allows to draw a cube with terra texture, use where you need to draw a single cube (tool, interface etc)
    /// </summary>
    public class CubeRenderer : Component
    {
        private readonly D3DEngine _engine;
        private readonly VisualWorldParameters _visualWorldParameters;
        //Use to draw textured cubes
        private HLSLCubeTool _cubeToolEffect;
        private readonly IMeshFactory _milkShapeMeshfactory;
        private Mesh _cubeMeshBluePrint;
        private Mesh _cubeMesh;
        private VertexBuffer<VertexMesh> _cubeVb;
        private IndexBuffer<ushort> _cubeIb;
        private ShaderResourceView _cubeTextureView;

        public CubeRenderer(D3DEngine engine, VisualWorldParameters visualWorldParameters)
        {
            _engine = engine;
            _visualWorldParameters = visualWorldParameters;
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
        }

        public void LoadContent(DeviceContext context)
        {
            //Prepare Textured Block rendering when equiped ==============================================================
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);

            _cubeTextureView = _visualWorldParameters.CubeTextureManager.CubeArrayTexture;

            //Create Vertex/Index Buffer to store the loaded cube mesh.
            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(context.Device, _cubeMeshBluePrint.Vertices.Length, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(context.Device, _cubeMeshBluePrint.Indices.Length, "Block IB"));

            _cubeToolEffect = ToDispose(new HLSLCubeTool(context.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeToolEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeToolEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
        }

        public void PrepareCubeRendering(CubeResource cube)
        {
            //Get the cube profile.
            var blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.CubeId];

            //Prapare to creation a new mesh with the correct texture mapping ID
            var materialChangeMapping = new Dictionary<int, int>();
            materialChangeMapping[0] = blockProfile.Tex_Back.TextureArrayId;    //Change the Back Texture Id
            materialChangeMapping[1] = blockProfile.Tex_Front.TextureArrayId;   //Change the Front Texture Id
            materialChangeMapping[2] = blockProfile.Tex_Bottom.TextureArrayId;  //Change the Bottom Texture Id
            materialChangeMapping[3] = blockProfile.Tex_Top.TextureArrayId;     //Change the Top Texture Id
            materialChangeMapping[4] = blockProfile.Tex_Left.TextureArrayId;    //Change the Left Texture Id
            materialChangeMapping[5] = blockProfile.Tex_Right.TextureArrayId;   //Change the Right Texture Id

            //Create the cube Mesh from the blue Print one
            _cubeMesh = _cubeMeshBluePrint.Clone(materialChangeMapping);

            //Refresh the mesh data inside the buffers
            _cubeVb.SetData(_engine.ImmediateContext, _cubeMesh.Vertices);
            _cubeIb.SetData(_engine.ImmediateContext, _cubeMesh.Indices);
        }

        public void Render(DeviceContext context, Matrix screen, Matrix projection, Color3 light)
        {
            //Render First person view of the tool, only if the tool is used by the current playing person !
            _cubeToolEffect.Begin(context);
            _cubeToolEffect.CBPerDraw.Values.Projection = Matrix.Transpose(projection);
            _cubeToolEffect.CBPerDraw.Values.Screen = Matrix.Transpose(screen);
            _cubeToolEffect.CBPerDraw.Values.LightColor = light;
            _cubeToolEffect.CBPerDraw.IsDirty = true;

            _cubeToolEffect.Apply(context);
            //Set the buffer to the device
            _cubeVb.SetToDevice(context, 0);
            _cubeIb.SetToDevice(context, 0);

            //Draw things here.
            context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
        }
    }
}
