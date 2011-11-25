using System;
using LostIsland.Shared.Tools;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Textures;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Settings;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3Engines.Meshes.Factories;
using S33M3Engines.Meshes;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Shared.Math;
using UtopiaContent.Effects.Entities;
using Utopia.Shared.Cubes;
using Utopia.Worlds.Cubes;
using System.Collections.Generic;
using S33M3Engines.StatesManager;
using S33M3Engines.Sprites;
using SharpDX.DXGI;
using Utopia.Shared.Settings;

namespace Utopia.Entities
{
    /// <summary>
    /// Provides cached access to items icons
    /// </summary>
    public class IconFactory : GameComponent
    {
        #region private variables
        private readonly D3DEngine _d3DEngine;
        public const int IconSize = 64;
        private ShaderResourceView _iconsTextureArray;

        private SpriteTexture _iconTextureArray;

        private int _nbrCubeIcon;

        #endregion

        #region public Variables/properties
        #endregion

        public IconFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public override void LoadContent()
        {
            List<Texture2D> icons;
            ShaderResourceView cubeTextureView;
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out cubeTextureView);
            icons = Create3DBlockIcons(cubeTextureView);

            _nbrCubeIcon = icons.Count;

            Texture2D[] spriteTextures;
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, ClientSettings.TexturePack + @"Sprites/", @"*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out spriteTextures, 1);

            icons.AddRange(spriteTextures);
            CreateTextureArray(icons);

            cubeTextureView.Dispose();
            foreach (Texture2D tex in spriteTextures) tex.Dispose();
        }

        public override void Dispose()
        {
            _iconsTextureArray.Dispose();
            _iconTextureArray.Dispose();
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
            else if (item is SpriteItem)
            {
                texture = _iconTextureArray;
                var spriteItem = (SpriteItem)item;
                var entityProfile = GameSystemSettings.Current.Settings.EntityProfile[item.ClassId];

                textureArrayIndex = _nbrCubeIcon;

                if (entityProfile.NbrGrowSprites > 0)
                {
                    textureArrayIndex += entityProfile.SpriteID + ((IGrowEntity)item).GrowPhase;
                }
                else
                {
                    textureArrayIndex += entityProfile.SpriteID;
                }
                return;
            }
            else if (item is VoxelItem)
            {
                VoxelItem voxelItem = item as VoxelItem;

                //2 options : 
                // option 1 :  draw voxelModel in a render target texture (reuse/pool while unchanged)
                // option 2 :  cpu projection of voxels into a dynamic Texture (making a for loop on blocks, creating a sort of heigtmap in a bitmap)
            }
            return;
        }
        #endregion

        #region Private methods

        private void CreateTextureArray(List<Texture2D> textureArray)
        {
            //Create the Icon texture Array
            ArrayTexture.CreateTexture2D(_d3DEngine.Device, textureArray.ToArray(), FilterFlags.Linear, "Icon's ArrayTexture", out _iconsTextureArray);

            // indexes into array corresponds textures with modifier +1 (Air)
            _iconTextureArray = new SpriteTexture(IconSize, IconSize, _iconsTextureArray, new Vector2());
        }

        private List<Texture2D> Create3DBlockIcons(ShaderResourceView _cubesTexture)
        {
            List<Texture2D> createdIconsTexture = new List<Texture2D>();

            SpriteRenderer spriteRenderer = new SpriteRenderer();
            spriteRenderer.Initialize(_d3DEngine);
            //Get the "Block" mesh that will be used to draw the various blocks.
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            Mesh meshBluePrint;

            int textureSize = IconSize;
            
            meshfactory.LoadMesh(@"\Meshes\block.txt", out meshBluePrint, 0);
            //Create Vertex/Index Buffer to store the loaded mesh.
            VertexBuffer<VertexMesh> vb = new VertexBuffer<VertexMesh>(_d3DEngine,
                                                                       meshBluePrint.Vertices.Length,
                                                                       VertexMesh.VertexDeclaration,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "Block VB");
            IndexBuffer<ushort> ib = new IndexBuffer<ushort>(_d3DEngine, meshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB");

            //Create the render texture
            RenderedTexture2D texture = new RenderedTexture2D(_d3DEngine, textureSize, textureSize, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 255, 255, 255)
            };

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
            Texture2D SpriteTexture = new Texture2D(_d3DEngine.Device, SpriteTextureDesc);
            DataStream dataStream;
            _d3DEngine.Context.MapSubresource(SpriteTexture, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            dataStream.Position = 0;
            dataStream.Write<Vector4>(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); //Ecrire dans la texture
            dataStream.Position = 0;
            _d3DEngine.Context.UnmapSubresource(SpriteTexture, 0);

            SpriteTexture spriteTexture = new SpriteTexture(_d3DEngine.Device, SpriteTexture, new Vector2(0, 0));
            spriteTexture.ScreenPosition = Matrix.Scaling(textureSize) * spriteTexture.ScreenPosition;
            SpriteTexture.Dispose();

            //Create the Shadder used to render on the texture.
            HLSLIcons shader = new HLSLIcons(_d3DEngine,
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
                vb.SetData(mesh.Vertices);
                ib.SetData(mesh.Indices);

                //Begin Drawing
                texture.Begin();

                StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

                shader.Begin();

                shader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(-0.8f, -0.9f, 1.5f) * -1;
                shader.CBPerFrame.Values.View = Matrix.Transpose(view);
                shader.CBPerFrame.Values.Projection = Matrix.Transpose(projection);
                shader.CBPerFrame.IsDirty = true;

                if (profile.YBlockOffset > 0)
                {
                    WorldScale = Matrix.Scaling(1, 1.0f - profile.YBlockOffset, 1);
                }
                else
                {
                    WorldScale = Matrix.Identity;
                }

                shader.CBPerDraw.Values.World = Matrix.Transpose(WorldScale * Matrix.RotationY(MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi / 5));
                shader.CBPerDraw.IsDirty = true;

                shader.DiffuseTexture.Value = _cubesTexture;
                shader.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);  

                shader.Apply();
                //Set the buffer to the device
                vb.SetToDevice(0);
                ib.SetToDevice(0);

                //Draw things here.
                _d3DEngine.Context.DrawIndexed(ib.IndicesCount, 0, 0);

                //Draw a sprite for lighting block
                if (profile.IsEmissiveColorLightSource)
                {
                    spriteRenderer.Begin(false, SpriteRenderer.FilterMode.Point);
                    spriteRenderer.Draw(spriteTexture, ref spriteTexture.ScreenPosition, new Color4(0.5f, profile.EmissiveColor.R /255, profile.EmissiveColor.G/255, profile.EmissiveColor.B/255), new RectangleF(0,0,textureSize,textureSize));
                    spriteRenderer.End();
                }

                //End Drawing
                texture.End(false);
                
                //Texture2D.ToFile<Texture2D>(_d3DEngine.Context, texture.RenderTargetTexture, ImageFileFormat.Png, @"E:\text\Block" + profile.Name + ".png");

                //Must be staging as these needs to have CPU read/write access (to create the Texture2d array from them)
                createdIconsTexture.Add(texture.CloneTexture(ResourceUsage.Staging));
            }

            //Reset device Default render target
            _d3DEngine.ResetDefaultRenderTargetsAndViewPort();

            //Dispose temp resource.
            texture.Dispose();
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