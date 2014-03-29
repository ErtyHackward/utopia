using System.Collections.Generic;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.Textures;
using S33M3Resources.Structs.Vertex;
using SharpDX.Direct3D11;
using Utopia.Shared.Configuration;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using System.Linq;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.World;

namespace Utopia.Entities
{
    /// <summary>
    /// Holds all terra cubes models and the effect to draw them
    /// </summary>
    public class TerraCubes : GameComponent
    {
        private struct CubePack
        {
            public Mesh Mesh;
            public VertexBuffer<VertexMesh> Vb;
            public IndexBuffer<ushort> Ib;
        }

        private VisualWorldParameters _config;
        private readonly D3DEngine _d3DEngine;
        private HLSLCubeTool _cubeEffect;
        private IMeshFactory _milkShapeMeshfactory;
        private Mesh _cubeMeshBluePrint;

        private readonly Dictionary<byte, CubePack> _cache = new Dictionary<byte, CubePack>();

        private ShaderResourceView _cubeTextureView;

        /// <summary>
        /// Common effect to draw the cubes
        /// </summary>
        public HLSLCubeTool Effect
        {
            get { return _cubeEffect; }
        }

        public TerraCubes(D3DEngine d3DEngine, VisualWorldParameters config)
        {
            _d3DEngine = d3DEngine;
            _config = config;
        }

        public override void Initialize()
        {
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();

            
        }

        public override void LoadContent(DeviceContext context)
        {
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);

            _cubeTextureView = _config.CubeTextureManager.CubeArrayTexture;
            //ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, _d3DEngine.ImmediateContext, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTextureView);

            foreach (var cube in _config.WorldParameters.Configuration.GetAllCubesProfiles())
            {
                CubePack pack;

                //Prapare to creation of a new mesh with the correct texture mapping ID
                var materialChangeMapping = new Dictionary<int, int>();
                materialChangeMapping[0] = cube.Tex_Back.TextureArrayId;    //Change the Back Texture Id
                materialChangeMapping[1] = cube.Tex_Front.TextureArrayId;   //Change the Front Texture Id
                materialChangeMapping[2] = cube.Tex_Bottom.TextureArrayId;  //Change the Bottom Texture Id
                materialChangeMapping[3] = cube.Tex_Top.TextureArrayId;     //Change the Top Texture Id
                materialChangeMapping[4] = cube.Tex_Left.TextureArrayId;    //Change the Left Texture Id
                materialChangeMapping[5] = cube.Tex_Right.TextureArrayId;   //Change the Right Texture Id

                pack.Mesh = _cubeMeshBluePrint.Clone(materialChangeMapping);
                
                pack.Vb = ToDispose(new VertexBuffer<VertexMesh>(_d3DEngine.Device, _cubeMeshBluePrint.Vertices.Length, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
                pack.Ib = ToDispose(new IndexBuffer<ushort>(_d3DEngine.Device, _cubeMeshBluePrint.Indices.Length, "Block IB"));

                pack.Vb.SetData(_d3DEngine.ImmediateContext, pack.Mesh.Vertices);
                pack.Ib.SetData(_d3DEngine.ImmediateContext, pack.Mesh.Indices);

                _cache.Add(cube.Id, pack);
            }
            
            _cubeEffect = ToDispose(new HLSLCubeTool(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
        }

        public void GetMesh(byte cubeProfile, out Mesh cubeMesh, out VertexBuffer<VertexMesh> vb, out IndexBuffer<ushort> ib)
        {
            cubeMesh = null;
            vb = null;
            ib = null;

            CubePack pack;
            if (!_cache.TryGetValue(cubeProfile, out pack))
                return;

            cubeMesh = pack.Mesh;
            vb = pack.Vb;
            ib = pack.Ib;
        }

    }
}
