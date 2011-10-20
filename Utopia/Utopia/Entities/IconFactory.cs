using System;
using LostIsland.Shared.Tools;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Textures;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Settings;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3Engines.Meshes.Factories;
using S33M3Engines.Meshes;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities
{
    public class IconFactory : GameComponent
    {
        #region private variables
        private readonly D3DEngine _d3DEngine;
        public const int IconSize = 32; 
        #endregion

        #region public Variables/properties
        public ShaderResourceView CubesTexture { get; private set; }
        #endregion

        public IconFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public override void LoadContent()
        {
            ShaderResourceView cubeTextureView;
            //TODO this code is at multiple places, could be only handled here, texturefactory instead of IconFactory ? 
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out cubeTextureView);
            CubesTexture = cubeTextureView;

            Create3DBlockIcons();
        }

        public override void Dispose()
        {
            CubesTexture.Dispose();

        }

        #region Public methods
        public SpriteTexture Lookup(IItem item)
        {
            //TODO pooling in  a dictionary<entityId,Texture>, but don't forget to unpool entities that become unused !
            if (item is CubeResource)
            {
                CubeResource blockAdder = item as CubeResource;
                SpriteTexture texture = new SpriteTexture(IconSize, IconSize, CubesTexture, Vector2.Zero);
                texture.Index = blockAdder.CubeId;
                return texture;
            }
            else if (item is SpriteItem)
            {
                //TODO spriteItem icon (shouldnt be difficult ;)
            }
            else if (item is VoxelItem)
            {
                VoxelItem voxelItem = item as VoxelItem;

                //2 options : 
                // option 1 : draw voxelModel in a render target texture (reuse/pool while unchanged)
                // option 2 :  cpu projection of voxels into a dynamic Texture (making a for loop on blocks, creating a sort of heigtmap in a bitmap)
            }
            return null;
        }
        #endregion

        #region Private methods
        private void Create3DBlockIcons()
        {
            //Get the "Block" mesh that will be used to draw the various blocks.
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            Mesh mesh;
            meshfactory.LoadMesh(@"\Meshes\Block.txt", out mesh, 0);
            //Create Vertex/Index Buffer to store the loaded mesh.
            VertexBuffer<VertexMesh> vb = new VertexBuffer<VertexMesh>(_d3DEngine,
                                                                       mesh.Vertices.Length,
                                                                       VertexMesh.VertexDeclaration,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "Block VB");
            IndexBuffer<ushort> ib = new IndexBuffer<ushort>(_d3DEngine, mesh.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB");

            //Create the render texture
            RenderedTexture2D texture = new RenderedTexture2D(_d3DEngine, IconSize, IconSize, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(255, 0, 150, 150)
            };

            //Create the Shadder used to render on the texture.
            HLSLVertexPositionNormalTexture shader = new HLSLVertexPositionNormalTexture(_d3DEngine,
                                                                                         @"D3D/Effects/Basics/VertexPositionNormalTexture.hlsl",
                                                                                         VertexMesh.VertexDeclaration);

            //Init Textures
            ShaderResourceView innerTexture = ShaderResourceView.FromFile(_d3DEngine.Device, ClientSettings.TexturePack + @"Terran/" + @"ct01.png");

            //Stored the mesh data inside teh buffers
            vb.SetData(mesh.Vertices);
            ib.SetData(mesh.Indices);

            //Compute projection + View matrix
            float aspectRatio = IconSize / IconSize;
            Matrix projection;
            Matrix.PerspectiveFovRH((float)Math.PI / 3, aspectRatio, 0.5f, 100f, out projection);
            Matrix view = Matrix.LookAtRH(new Vector3(0, 0, -1.8f), Vector3.Zero, Vector3.UnitY);

            //Begin Drawing
            texture.Begin();

            shader.Begin();

            shader.CBPerFrame.Values.Alpha = 1;
            shader.CBPerFrame.Values.View = Matrix.Transpose(view);
            shader.CBPerFrame.Values.Projection = Matrix.Transpose(projection);
            shader.CBPerFrame.IsDirty = true;

            shader.CBPerDraw.Values.World = Matrix.Transpose( Matrix.RotationY(MathHelper.PiOver4) * Matrix.Translation(new Vector3(0, 0, 0)));
            shader.CBPerDraw.IsDirty = true;

            shader.DiffuseTexture.Value = innerTexture;

            shader.Apply();
            //Set the buffer to the device
            vb.SetToDevice(0);
            ib.SetToDevice(0);

            //Draw things here.
            _d3DEngine.Context.DrawIndexed(ib.IndicesCount, 0, 0);

            //End Drawing
            texture.End(false);

            //Texture2D.ToFile<Texture2D>(_d3DEngine.Context, texture.RenderTargetTexture, ImageFileFormat.Png, @"e:\test.png");

            //Reset device Default render target
            _d3DEngine.ResetDefaultRenderTargetsAndViewPort();

            //Dispose temp resource.
            shader.Dispose();
            vb.Dispose();
            ib.Dispose();
            innerTexture.Dispose();
        }
        #endregion
        
    }
}