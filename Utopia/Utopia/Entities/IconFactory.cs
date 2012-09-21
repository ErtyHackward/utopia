using System;
using S33M3Resources.Effects.Sprites;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using UtopiaContent.Effects.Entities;
using Utopia.Shared.Cubes;
using System.Collections.Generic;
using SharpDX.DXGI;
using Utopia.Shared.Settings;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine.Textures;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Textures;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3Resources.Structs;
using Utopia.Shared.GameDXStates;
using Resource = SharpDX.Direct3D11.Resource;

namespace Utopia.Entities
{
    /// <summary>
    /// Provides cached access to item icons
    /// </summary>
    public class IconFactory : GameComponent
    {
        #region private variables
        private readonly D3DEngine _d3DEngine;
        private readonly VoxelModelManager _modelManager;
        public const int IconSize = 64;
        private ShaderResourceView _iconsTextureArray;
        private SpriteTexture _iconTextureArray;

        // holds cached textures of the voxel models
        private readonly Dictionary<string, SpriteTexture> _voxelIcons = new Dictionary<string, SpriteTexture>();

        private HLSLVoxelModel _voxelEffect;

        private HLSLBlur _blurEffect;
        private VertexBuffer<VertexPosition2Texture> _blurVertexBuffer;
        private IndexBuffer<short> _iBuffer;

        private int _nbrCubeIcon;

        #endregion

        #region public Variables/properties
        #endregion

        public IconFactory(D3DEngine d3DEngine, VoxelModelManager modelManager)
        {
            _d3DEngine = d3DEngine;
            _modelManager = modelManager;
        }

        public override void LoadContent(DeviceContext context)
        {
            List<Texture2D> icons;
            ShaderResourceView cubeTextureView;
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out cubeTextureView);
            icons = Create3DBlockIcons(context, cubeTextureView);
            
            _nbrCubeIcon = icons.Count;

            Texture2D[] spriteTextures;
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, ClientSettings.TexturePack + @"Sprites/", @"*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out spriteTextures, 1);

            icons.AddRange(spriteTextures);
            CreateTextureArray(context, icons);

            //Array created i can dispose the various icons
            foreach (var icon in icons)
            {
                icon.Dispose();
            }

            cubeTextureView.Dispose();
            foreach (var tex in spriteTextures) tex.Dispose();

            _iBuffer = ToDispose(new IndexBuffer<short>(_d3DEngine.Device, 6, SharpDX.DXGI.Format.R16_UInt, "Blur_iBuffer"));
            _blurVertexBuffer = ToDispose(new VertexBuffer<VertexPosition2Texture>(_d3DEngine.Device, 4, VertexPosition2Texture.VertexDeclaration, PrimitiveTopology.TriangleList, "Blur_vBuffer", ResourceUsage.Immutable));

            //Load data into the VB  => NOT Thread safe, MUST be done in the loadcontent
            VertexPosition2Texture[] vertices =
                {
                    new VertexPosition2Texture(new Vector2(-1.00f, -1.00f), new Vector2(-1.00f, -1.00f)),
                    new VertexPosition2Texture(new Vector2(1.00f, -1.00f), new Vector2(1.00f, -1.00f)),
                    new VertexPosition2Texture(new Vector2(1.00f, 1.00f), new Vector2(1.00f, 1.00f)),
                    new VertexPosition2Texture(new Vector2(-1.00f, 1.00f), new Vector2(-1.00f, 1.00f))
                };
            _blurVertexBuffer.SetData(context, vertices);

            //Load data into the IB => NOT Thread safe, MUST be done in the loadcontent
            short[] indices = { 3, 0, 2, 0, 1, 2 };
            _iBuffer.SetData(context, indices);

            _blurEffect = ToDispose(new HLSLBlur(_d3DEngine.Device));
            _voxelEffect = ToDispose(new HLSLVoxelModel(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
            CreateVoxelIcons(context);
        }

        public override void BeforeDispose()
        {
            if (_iconsTextureArray != null) _iconsTextureArray.Dispose();
            if (_iconTextureArray != null) _iconTextureArray.Dispose();
        }

        //Too costy to recreate, better keep it
        public override void UnloadContent()
        {
            //this.DisableComponent(); //Disable to component

            //if (_iconsTextureArray != null) _iconsTextureArray.Dispose();
            //if (_iconTextureArray != null) _iconTextureArray.Dispose();

            //_nbrCubeIcon = 0;

            //this.IsInitialized = false;
        }

        #region Public methods
        public void Lookup(IItem item, out SpriteTexture texture, out int textureArrayIndex)
        {
            texture = null;
            textureArrayIndex = 0;

            if (item is CubeResource)
            {
                texture = _iconTextureArray;
                var cubeId = ((CubeResource)item).CubeId;
                textureArrayIndex = cubeId - 1;
                return;
            }
            else if (item is Item)
            {
                var voxelItem = item as Item;

                _voxelIcons.TryGetValue(voxelItem.ModelName, out texture);

                //2 options : 
                // option 1 :  draw voxelModel in a render target texture (reuse/pool while unchanged)
                // option 2 :  cpu projection of voxels into a dynamic Texture (making a for loop on blocks, creating a sort of heigtmap in a bitmap)
            }
          
            return;
        }
        #endregion

        #region Private methods

        private void CreateTextureArray(DeviceContext context, List<Texture2D> textureArray)
        {
            //Create the Icon texture Array
            ArrayTexture.CreateTexture2D(_d3DEngine.Device, context, textureArray.ToArray(), FilterFlags.Linear, "Icon's ArrayTexture", out _iconsTextureArray);
            
            // indexes into array corresponds textures with modifier +1 (Air)
            _iconTextureArray = new SpriteTexture(IconSize, IconSize, _iconsTextureArray, new Vector2());
        }

        private void CreateVoxelIcons(DeviceContext context)
        {
            //Create the render texture
            var texture = ToDispose(new RenderedTexture2D(_d3DEngine, IconSize, IconSize, Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 0, 0, 0)
            });

            var blurredTex = ToDispose(new RenderedTexture2D(_d3DEngine, IconSize, IconSize, Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 0, 0, 0)
            });

            float aspectRatio = IconSize / IconSize;
            Matrix projection;
            Matrix.PerspectiveFovLH((float)Math.PI / 3.6f, aspectRatio, 0.5f, 100f, out projection);
            Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            foreach (var visualVoxelModel in _modelManager.Enumerate())
            {
                texture.Begin();

                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthEnabled);

                _voxelEffect.Begin(context);

                _voxelEffect.CBPerFrame.Values.LightDirection = Vector3.Zero;
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
                _voxelEffect.CBPerFrame.IsDirty = true;

                var instance = visualVoxelModel.VoxelModel.CreateInstance();

                var state = visualVoxelModel.VoxelModel.GetMainState();

                instance.SetState(state);
                var size = state.BoundingBox.GetSize();

                var offset = - size / 2 - state.BoundingBox.Minimum;

                const float scaleFactor = 1.6f; // the bigger factor the bigger items

                var scale = Math.Min(scaleFactor / size.X, Math.Min(scaleFactor / size.Y, scaleFactor / size.Z));

                instance.World = Matrix.Translation(offset) * Matrix.Scaling(scale) * Matrix.RotationY(MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi / 5);

                visualVoxelModel.Draw(context, _voxelEffect, instance);

                texture.End(false);


                var tex2D = texture.CloneTexture(ResourceUsage.Default);



                blurredTex.Begin();

                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

                _blurVertexBuffer.SetToDevice(context, 0);
                _iBuffer.SetToDevice(context, 0);

                _blurEffect.Begin(context);
                _blurEffect.SpriteTexture.Value = new SpriteTexture(tex2D).Texture;
                _blurEffect.Apply(context);

                context.DrawIndexed(6, 0, 0);
                
                blurredTex.End(false);

                var tex2DBlurred = blurredTex.CloneTexture(ResourceUsage.Default);

                Resource.ToFile(context, tex2DBlurred, ImageFileFormat.Png, visualVoxelModel.VoxelModel.Name + "-blur.png");

                _voxelIcons.Add(visualVoxelModel.VoxelModel.Name, new SpriteTexture(tex2D));

            }

            //Reset device Default render target
            _d3DEngine.SetRenderTargetsAndViewPort();
        }

        private List<Texture2D> Create3DBlockIcons(DeviceContext context, ShaderResourceView cubesTexture)
        {
            List<Texture2D> createdIconsTexture = new List<Texture2D>();

            SpriteRenderer spriteRenderer = new SpriteRenderer(_d3DEngine);
            //Get the "Block" mesh that will be used to draw the various blocks.
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            Mesh meshBluePrint;

            int textureSize = IconSize;
            
            meshfactory.LoadMesh(@"\Meshes\block.txt", out meshBluePrint, 0);
            //Create Vertex/Index Buffer to store the loaded mesh.
            VertexBuffer<VertexMesh> vb = new VertexBuffer<VertexMesh>(_d3DEngine.Device,
                                                                       meshBluePrint.Vertices.Length,
                                                                       VertexMesh.VertexDeclaration,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "Block VB");
            IndexBuffer<ushort> ib = new IndexBuffer<ushort>(_d3DEngine.Device, meshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB");

            //Create the render texture
            RenderedTexture2D texture = ToDispose(new RenderedTexture2D(_d3DEngine, textureSize, textureSize, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 0, 0, 0)
            });

            Texture2DDescription SpriteTextureDesc = new Texture2DDescription()
            {
                Width = 1,
                Height = 1,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32G32B32A32_Float,
                SampleDescription = new SampleDescription() { Count = 1 },
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write
            };
            Texture2D sTexture = new Texture2D(_d3DEngine.Device, SpriteTextureDesc);

            DataStream dataStream;
            DataBox data = context.MapSubresource(sTexture, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            dataStream.Position = 0;
            dataStream.Write<Vector4>(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); //Ecrire dans la texture
            dataStream.Position = 0;
            context.UnmapSubresource(sTexture, 0);
            dataStream.Dispose();

            SpriteTexture spriteTexture = new SpriteTexture(sTexture);
            spriteTexture.ScreenPosition = new Rectangle(spriteTexture.ScreenPosition.X, spriteTexture.ScreenPosition.Y, spriteTexture.ScreenPosition.X + textureSize, spriteTexture.ScreenPosition.Y + textureSize) ;
            sTexture.Dispose();

            //Create the Shadder used to render on the texture.
            HLSLIcons shader = new HLSLIcons(_d3DEngine.Device,
                                             ClientSettings.EffectPack + @"Entities/Icons.hlsl",
                                             VertexMesh.VertexDeclaration);

            //Compute projection + View matrix
            float aspectRatio = textureSize / textureSize;
            Matrix projection;
            Matrix.PerspectiveFovLH((float)Math.PI / 3.6f, aspectRatio, 0.5f, 100f, out projection);
            Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);
            Matrix WorldScale;

            Dictionary<int, int> MaterialChangeMapping = new Dictionary<int, int>();
            MaterialChangeMapping.Add(0, 0); //Change the Back Texture Id
            MaterialChangeMapping.Add(1, 0); //Change the Front Texture Id
            MaterialChangeMapping.Add(2, 0); //Change the Bottom Texture Id
            MaterialChangeMapping.Add(3, 0); //Change the Top Texture Id
            MaterialChangeMapping.Add(4, 0); //Change the Left Texture Id
            MaterialChangeMapping.Add(5, 0); //Change the Right Texture Id

            //Create a texture for each cubes existing !
            foreach (byte cubeId in CubeId.All())
            {
                //Don't create "Air" cube
                if (cubeId == 0) continue;
                //Create the new Material MeshMapping
                var profile = GameSystemSettings.Current.Settings.CubesProfile[cubeId];
                
                //Here the key parameter is the ID name given to the texture inside the file model.
                //In our case the model loaded has these Materials/texture Ids :
                // 0 = Back
                // 1 = Front
                // 2 = Bottom
                // 3 = Top
                // 4 = Left
                // 5 = Right
                //The value attached to it is simply the TextureID from the texture array to use.
                MaterialChangeMapping[0] = profile.Tex_Back; //Change the Back Texture Id
                MaterialChangeMapping[1] = profile.Tex_Front; //Change the Front Texture Id
                MaterialChangeMapping[2] = profile.Tex_Bottom; //Change the Bottom Texture Id
                MaterialChangeMapping[3] = profile.Tex_Top; //Change the Top Texture Id
                MaterialChangeMapping[4] = profile.Tex_Left; //Change the Left Texture Id
                MaterialChangeMapping[5] = profile.Tex_Right; //Change the Right Texture Id

                Mesh mesh = meshBluePrint.Clone(MaterialChangeMapping);
                //Stored the mesh data inside the buffers
                vb.SetData(context, mesh.Vertices);
                ib.SetData(context, mesh.Indices);

                //Begin Drawing
                texture.Begin();

                RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

                //Set sampler
                shader.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);

                shader.Begin(context);

                shader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(-0.8f, -0.9f, 1.5f) * -1;
                shader.CBPerFrame.Values.View = Matrix.Transpose(view);
                shader.CBPerFrame.Values.Projection = Matrix.Transpose(projection);
                shader.CBPerFrame.IsDirty = true;

                if (profile.YBlockOffset > 0)
                {
                    WorldScale = Matrix.Scaling(1, (float)(1.0f - profile.YBlockOffset), 1);
                }
                else
                {
                    WorldScale = Matrix.Identity;
                }

                shader.CBPerDraw.Values.World = Matrix.Transpose(WorldScale * Matrix.RotationY(MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi / 5));
                shader.CBPerDraw.IsDirty = true;

                shader.DiffuseTexture.Value = cubesTexture;

                shader.Apply(context);
                //Set the buffer to the device
                vb.SetToDevice(context, 0);
                ib.SetToDevice(context, 0);

                //Draw things here.
                context.DrawIndexed(ib.IndicesCount, 0, 0);

                //Draw a sprite for lighting block
                if (profile.IsEmissiveColorLightSource)
                {
                    spriteRenderer.Begin(true);
                    ByteColor color = new ByteColor(profile.EmissiveColor.R, profile.EmissiveColor.G, profile.EmissiveColor.B, (byte)127);
                    spriteRenderer.Draw(spriteTexture, ref spriteTexture.ScreenPosition, ref color);
                    spriteRenderer.EndWithCustomProjection(context, ref texture.Projection2D);
                }

                //End Drawing
                texture.End(false);

                //Texture2D.ToFile<Texture2D>(_d3DEngine.ImmediateContext, texture.RenderTargetTexture, ImageFileFormat.Png, @"E:\text\Block" + profile.Name + ".png");

                //Must be staging as these needs to have CPU read/write access (to create the Texture2d array from them)
                createdIconsTexture.Add(texture.CloneTexture(ResourceUsage.Staging));
            }

            //Reset device Default render target
            _d3DEngine.SetRenderTargetsAndViewPort();

            //Dispose temp resource.
            spriteTexture.Dispose();
            shader.Dispose();
            vb.Dispose();
            ib.Dispose();
            spriteRenderer.Dispose();

            return createdIconsTexture;            
        }

        private List<Texture2D> CreateSpritesIcons(ShaderResourceView spriteTextureView)
        {
            return new List<Texture2D>();
        }
        #endregion
        
    }
}