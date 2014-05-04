using System;
using System.IO;
using System.Linq;
using S33M3Resources.Effects.Sprites;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using System.Collections.Generic;
using SharpDX.DXGI;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Settings;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Sprites2D;
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
using Utopia.Shared.Configuration;
using Utopia.Shared.World;
using Utopia.Resources.Effects.Entities;

namespace Utopia.Entities
{
    /// <summary>
    /// Provides cached access to item icons
    /// </summary>
    public class IconFactory : GameComponent
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region private variables
        private readonly D3DEngine _d3DEngine;
        private readonly VoxelModelManager _modelManager;
        public const int IconSize = 64;
        private ShaderResourceView _iconsTextureArray;
        private SpriteTexture _iconTextureArray;
        private VisualWorldParameters _visualWorldParameters;
        private Dictionary<byte, byte> _cubeIconIndexes;

        // holds cached textures of the voxel models
        private readonly Dictionary<string, SpriteTexture> _voxelIcons = new Dictionary<string, SpriteTexture>();

        private HLSLVoxelModel _voxelEffect;

        private HLSLBlur _blurHorisontalEffect;
        private HLSLBlur _blurVerticalEffect;
        private VertexBuffer<VertexPosition2Texture> _blurVertexBuffer;
        private IndexBuffer<ushort> _iBuffer;
        private HLSLColorOverlay _overlayEffect;

        private int _nbrCubeIcon;

        #endregion

        public VisualWorldParameters VisualWorldParameters
        {
            get { return _visualWorldParameters; }
            set { _visualWorldParameters = value; }
        }

        public WorldConfiguration Configuration
        {
            get { return _visualWorldParameters.WorldParameters.Configuration; }
            set { _visualWorldParameters.WorldParameters.Configuration = value; }
        }

        public VoxelModelManager VoxelModelManager
        {
            get { return _modelManager; }
        }

        public IconFactory(D3DEngine d3DEngine, VoxelModelManager modelManager, VisualWorldParameters visualWorldParameters)
        {
            _d3DEngine = d3DEngine;
            _modelManager = modelManager;
            _visualWorldParameters = visualWorldParameters;
            //if(Utopia.UtopiaRender.LCDefferedModeLvl !=1) this.IsDefferedLoadContent = true;
        }

        public override void LoadContent(DeviceContext context)
        {
            _iBuffer = ToDispose(new IndexBuffer<ushort>(context.Device, 6, "Blur_iBuffer"));
            _blurVertexBuffer = ToDispose(new VertexBuffer<VertexPosition2Texture>(context.Device, 4, PrimitiveTopology.TriangleList, "Blur_vBuffer", ResourceUsage.Immutable));

            //Create Vertices for Icon Plane.
            VertexPosition2Texture[] vertices =
                {
                    new VertexPosition2Texture(new Vector2(-1.00f, -1.00f), new Vector2(0.00f, 1.00f)),
                    new VertexPosition2Texture(new Vector2( 1.00f, -1.00f), new Vector2(1.00f, 1.00f)),
                    new VertexPosition2Texture(new Vector2( 1.00f,  1.00f), new Vector2(1.00f, 0.00f)),
                    new VertexPosition2Texture(new Vector2(-1.00f,  1.00f), new Vector2(0.00f, 0.00f))
                };
            _blurVertexBuffer.SetData(context, vertices); //Send the vertices inside the VBuffer

            //Create the Indices for icon plane
            ushort[] indices = { 0, 3, 1, 1, 3, 2 };
            _iBuffer.SetData(context, indices); //Send the vertices inside the IBuffer

            //Create the various Effect for rendering the icons
            _blurHorisontalEffect = ToDispose(new HLSLBlur(_d3DEngine.Device, HLSLBlur.BlurPass.Horizontal, Path.Combine(ClientSettings.PathRoot, @"Effects\Sprites\Blur.hlsl")));
            _blurVerticalEffect = ToDispose(new HLSLBlur(_d3DEngine.Device, HLSLBlur.BlurPass.Vertical, Path.Combine(ClientSettings.PathRoot, @"Effects\Sprites\Blur.hlsl")));
            _voxelEffect = ToDispose(new HLSLVoxelModel(_d3DEngine.Device, Path.Combine(ClientSettings.EffectPack, @"Entities\VoxelModel.hlsl"), VertexVoxel.VertexDeclaration));
            _overlayEffect = ToDispose(new HLSLColorOverlay(_d3DEngine.Device, Path.Combine(ClientSettings.PathRoot, @"Effects\Sprites\ColorOverlay.hlsl")));

            if (_visualWorldParameters.WorldParameters != null && _visualWorldParameters.WorldParameters.Configuration != null)
            {
                List<Texture2D> icons = new List<Texture2D>();

                ShaderResourceView cubeTextureView = _visualWorldParameters.CubeTextureManager.CubeArrayTexture;

                //ShaderResourceView cubeTextureView;
                //ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, context,
                //                                      Path.Combine(ClientSettings.TexturePack, @"Terran\"), @"ct*.png",
                //                                      FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer",
                //                                      out cubeTextureView);

                icons = Create3DBlockIcons(context, cubeTextureView, IconSize);

                _nbrCubeIcon = icons.Count;
                if (_nbrCubeIcon == 0) return;
                //Texture2D[] spriteTextures;
                //ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device,
                //                                      Path.Combine(ClientSettings.TexturePack, @"Sprites\"), @"*.png",
                //                                      FilterFlags.Point, "ArrayTexture_WorldChunk", out spriteTextures,
                //                                      1);
                //icons.AddRange(spriteTextures);
                CreateTextureArray(context, icons);

                //Array created i can dispose the various icons
                foreach (var icon in icons)
                {
                    icon.Dispose();
                }
                //cubeTextureView.Dispose();
                //foreach (var tex in spriteTextures) tex.Dispose();


                CreateVoxelIcons(context);
            }
            else
            {
                logger.Warn("Could not create voxel models and blocks icons, no configuration is set");
            }
        }
        
        public override void BeforeDispose()
        {
            if (_iconsTextureArray != null) _iconsTextureArray.Dispose();
            if (_iconTextureArray != null) _iconTextureArray.Dispose();
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
                textureArrayIndex = _cubeIconIndexes[cubeId];
                return;
            }
            if (item is Item)
            {
                var voxelItem = item as Item;

                var id = voxelItem.ModelName;

                if (id == null)
                    return;

                if (!string.IsNullOrEmpty(voxelItem.ModelState))
                {
                    id += ":" + voxelItem.ModelState;
                }

                _voxelIcons.TryGetValue(id, out texture);
            }
        }
        #endregion

        #region Private methods

        private void CreateTextureArray(DeviceContext context, List<Texture2D> textureArray)
        {
            //Create the Icon texture Array
            ArrayTexture.CreateTexture2D(context, textureArray.ToArray(), "Icon's ArrayTexture", out _iconsTextureArray);
            
            // indexes into array corresponds textures with modifier +1 (Air)
            _iconTextureArray = new SpriteTexture(IconSize, IconSize, _iconsTextureArray, new Vector2());
        }

        public Texture2D CreateVoxelIcon(VisualVoxelModel visualVoxelModel, Size2 iconSize, VoxelModelState state = null, DeviceContext context = null, Matrix transform = default(Matrix))
        {
            if (context == null)
                context = _d3DEngine.ImmediateContext;

            //Create the render texture
            var texture = ToDispose(new RenderedTexture2D(_d3DEngine, iconSize.Width, iconSize.Height, Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 0, 0, 0)
            });

            float aspectRatio = IconSize / IconSize;
            Matrix projection;
            var fov = (float)Math.PI / 3.6f;
            Matrix.PerspectiveFovLH(fov, aspectRatio, 0.5f, 100f, out projection);
            Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            texture.Begin(context);

            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            _voxelEffect.Begin(context);

            _voxelEffect.CBPerFrame.Values.LightDirection = Vector3.Zero;
            _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
            _voxelEffect.CBPerFrame.IsDirty = true;

            var instance = visualVoxelModel.VoxelModel.CreateInstance();

            if (state == null)
            {
                var iconState = visualVoxelModel.VoxelModel.States.FirstOrDefault(s => string.Equals(s.Name, "Icon", StringComparison.CurrentCultureIgnoreCase));
                state = iconState ?? visualVoxelModel.VoxelModel.GetMainState();
            }

            instance.SetState(state);

            var sphere = BoundingSphere.FromBox(state.BoundingBox);

            var rMax = 2f * Math.Sin(fov / 2);

            var size = state.BoundingBox.GetSize();

            var offset = -size / 2 - state.BoundingBox.Minimum;

            var scale = (float)rMax / sphere.Radius; // Math.Min(scaleFactor / size.X, Math.Min(scaleFactor / size.Y, scaleFactor / size.Z));

            if (transform == default(Matrix))
                instance.World = Matrix.Translation(offset) * Matrix.Scaling(scale) * Matrix.RotationY(MathHelper.Pi + MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi / 5);
            else
            {
                instance.World = transform;
            }

            visualVoxelModel.Draw(context, _voxelEffect, instance);

            texture.End(context, false);


            var tex2D = texture.CloneTexture(context, ResourceUsage.Default);

            tex2D = DrawOuterShadow(context, texture, tex2D, iconSize.Width);

            _d3DEngine.SetRenderTargetsAndViewPort(context);

            return tex2D;
        }

        private void CreateVoxelIcons(DeviceContext context)
        {
            //Create the render texture
            var texture = ToDispose(new RenderedTexture2D(_d3DEngine, IconSize, IconSize, Format.R8G8B8A8_UNorm)
            {
                BackGroundColor = new Color4(0, 0, 0, 0)
            });

            float aspectRatio = IconSize / IconSize;
            Matrix projection;
            Matrix.PerspectiveFovLH((float)Math.PI / 3.6f, aspectRatio, 0.5f, 100f, out projection);
            Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            _voxelIcons.Clear();

            foreach (var visualVoxelModel in _modelManager.Enumerate())
            {
                foreach (var voxelModelState in visualVoxelModel.VoxelModel.States)
                {
                    System.Threading.Thread.Sleep(0);
                    texture.Begin(context);

                    RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

                    _voxelEffect.Begin(context);

                    _voxelEffect.CBPerFrame.Values.LightDirection = Vector3.Zero;
                    _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
                    _voxelEffect.CBPerFrame.IsDirty = true;

                    var instance = visualVoxelModel.VoxelModel.CreateInstance();


                    instance.SetState(voxelModelState);
                    var size = voxelModelState.BoundingBox.GetSize();
                    var offset = -size / 2 - voxelModelState.BoundingBox.Minimum;

                    const float scaleFactor = 1.6f; // the bigger factor the bigger items

                    var scale = Math.Min(scaleFactor / size.X, Math.Min(scaleFactor / size.Y, scaleFactor / size.Z));

                    instance.World = Matrix.Translation(offset) * Matrix.Scaling(scale) * Matrix.RotationY(MathHelper.Pi + MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi / 5);

                    visualVoxelModel.Draw(context, _voxelEffect, instance);

                    texture.End(context, false);


                    var tex2D = texture.CloneTexture(context, ResourceUsage.Default); //Create a copy of the currently painted icon (Need to do it since next FOR will paint on it again

                    try
                    {
                        if (voxelModelState.IsIconState)
                        {
                            _voxelIcons.Add(visualVoxelModel.VoxelModel.Name, ToDispose(new SpriteTexture(tex2D)));
                            _voxelIcons.Add(visualVoxelModel.VoxelModel.Name + ":" + voxelModelState.Name, ToDispose(new SpriteTexture(tex2D)));
                        }
                        else
                        {
                            _voxelIcons.Add(visualVoxelModel.VoxelModel.Name + ":" + voxelModelState.Name, ToDispose(new SpriteTexture(tex2D)));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error when generating icons: {0}", e.Message);
                    }
                }
            }

            //Reset device Default render target
            _d3DEngine.SetRenderTargetsAndViewPort(context);
        }

        private Texture2D DrawOuterShadow(DeviceContext context, RenderedTexture2D texture, Texture2D tex2D, int size = 0)
        {
            if (size == 0)
                size = IconSize;

            var clampSampler = ToDispose(new SamplerState(context.Device,
                            new SamplerStateDescription
                            {
                                AddressU = TextureAddressMode.Clamp,
                                AddressV = TextureAddressMode.Clamp,
                                AddressW = TextureAddressMode.Clamp,
                                Filter = Filter.MinMagMipLinear,
                                MaximumLod = float.MaxValue,
                                MinimumLod = 0
                            }));

            SpriteRenderer spriteRenderer = new SpriteRenderer(_d3DEngine, Path.Combine(ClientSettings.PathRoot, @"Effects\Sprites\Sprites2.hlsl"));
            texture.Begin(context);

            var voxelIconSpriteTexture = new SpriteTexture(tex2D);

            // draw black overlay
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

            _overlayEffect.Begin(context);
            _overlayEffect.SpriteTexture.Value = voxelIconSpriteTexture.Texture;
            _overlayEffect.SpriteSampler.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);
            _overlayEffect.CBPerDraw.Values.Color = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            _overlayEffect.CBPerDraw.IsDirty = true;
            _overlayEffect.Apply(context);

            _blurVertexBuffer.SetToDevice(context, 0);
            _iBuffer.SetToDevice(context, 0);

            context.DrawIndexed(6, 0, 0);

            texture.End(context, false);

            var tex2DOverlayed = texture.CloneTexture(context, ResourceUsage.Default);

            // blur horisontal
            texture.Begin(context);

            _blurHorisontalEffect.Begin(context);
            _blurHorisontalEffect.SpriteTexture.Value = new SpriteTexture(tex2DOverlayed).Texture;
            _blurHorisontalEffect.SpriteSampler.Value = clampSampler;
            _blurHorisontalEffect.CBPerDraw.Values.Size = size;
            _blurHorisontalEffect.CBPerDraw.IsDirty = true;
            _blurHorisontalEffect.Apply(context);

            context.DrawIndexed(6, 0, 0);

            texture.End(context, false);

            var tex2DHorisontalBlurred = texture.CloneTexture(context, ResourceUsage.Default);

            // blur vertical
            texture.Begin(context);

            _blurVerticalEffect.Begin(context);
            _blurVerticalEffect.SpriteTexture.Value = new SpriteTexture(tex2DHorisontalBlurred).Texture;
            _blurVerticalEffect.SpriteSampler.Value = clampSampler;
            _blurVerticalEffect.CBPerDraw.Values.Size = size;
            _blurVerticalEffect.CBPerDraw.IsDirty = true;
            _blurVerticalEffect.Apply(context);

            context.DrawIndexed(6, 0, 0);

            texture.End(context, false);

            var tex2DBlurred = texture.CloneTexture(context, ResourceUsage.Default);

            //Resource.ToFile(context, tex2DBlurred, ImageFileFormat.Png, visualVoxelModel.VoxelModel.Name + "-blur.png");

            texture.Begin(context);
            spriteRenderer.Begin(false, context);
            ByteColor color = new ByteColor(255, 255, 255, 255);
            var spriteTexture = new SpriteTexture(tex2DBlurred);
            spriteRenderer.Draw(spriteTexture, ref spriteTexture.ScreenPosition, ref color);
            spriteRenderer.Draw(voxelIconSpriteTexture, ref voxelIconSpriteTexture.ScreenPosition, ref color);
            spriteRenderer.EndWithCustomProjection(context, ref texture.Projection2D);
            texture.End(context, false);

            return texture.CloneTexture(context, ResourceUsage.Default);
        }

        public List<Texture2D> Get3DBlockIcons(DeviceContext context, Size2 iconSize, ShaderResourceView cubeTextureView)
        {
            return Create3DBlockIcons(context, cubeTextureView, iconSize.Width);
        }

        private List<Texture2D> Create3DBlockIcons(DeviceContext context, ShaderResourceView cubesTexture, int iconSize)
        {
            List<Texture2D> createdIconsTexture = new List<Texture2D>();

            SpriteRenderer spriteRenderer = new SpriteRenderer(_d3DEngine, Path.Combine(ClientSettings.PathRoot, @"Effects\Sprites\Sprites2.hlsl"));
            //Get the "Block" mesh that will be used to draw the various blocks.
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            Mesh meshBluePrint;

            int textureSize = iconSize;
            
            meshfactory.LoadMesh(ClientSettings.PathRoot + @"\Meshes\block.txt", out meshBluePrint, 0);
            //Create Vertex/Index Buffer to store the loaded mesh.
            VertexBuffer<VertexMesh> vb = new VertexBuffer<VertexMesh>(_d3DEngine.Device,
                                                                       meshBluePrint.Vertices.Length,                                                                       
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "Block VB");
            IndexBuffer<ushort> ib = new IndexBuffer<ushort>(_d3DEngine.Device, meshBluePrint.Indices.Length, "Block IB");

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
            spriteTexture.ScreenPosition = new Rectangle(spriteTexture.ScreenPosition.X, spriteTexture.ScreenPosition.Y, textureSize, textureSize) ;
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

            _cubeIconIndexes = new Dictionary<byte, byte>();

            //Create a texture for each cubes existing !
            foreach (BlockProfile profile in _visualWorldParameters.WorldParameters.Configuration.GetAllCubesProfiles())
            {
                //Don't create "Air" cube
                if (profile.Id == WorldConfiguration.CubeId.Air) continue;
                //Create the new Material MeshMapping
                
                //Here the key parameter is the ID name given to the texture inside the file model.
                //In our case the model loaded has these Materials/texture Ids :
                // 0 = Back
                // 1 = Front
                // 2 = Bottom
                // 3 = Top
                // 4 = Left
                // 5 = Right
                //The value attached to it is simply the TextureID from the texture array to use.
                MaterialChangeMapping[0] = profile.Textures == null ? 0 : profile.Tex_Back.TextureArrayId; //Change the Back Texture Id
                MaterialChangeMapping[1] = profile.Textures == null ? 0 : profile.Tex_Front.TextureArrayId; //Change the Front Texture Id
                MaterialChangeMapping[2] = profile.Textures == null ? 0 : profile.Tex_Bottom.TextureArrayId; //Change the Bottom Texture Id
                MaterialChangeMapping[3] = profile.Textures == null ? 0 : profile.Tex_Top.TextureArrayId; //Change the Top Texture Id
                MaterialChangeMapping[4] = profile.Textures == null ? 0 : profile.Tex_Left.TextureArrayId; //Change the Left Texture Id
                MaterialChangeMapping[5] = profile.Textures == null ? 0 : profile.Tex_Right.TextureArrayId; //Change the Right Texture Id

                Mesh mesh = meshBluePrint.Clone(MaterialChangeMapping);
                //Stored the mesh data inside the buffers
                vb.SetData(context, mesh.Vertices);
                ib.SetData(context, mesh.Indices);

                //Begin Drawing
                texture.Begin(context);

                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

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
                    spriteRenderer.Begin(true, context);
                    ByteColor color = new ByteColor(profile.EmissiveColor.R, profile.EmissiveColor.G, profile.EmissiveColor.B, (byte)127);
                    spriteRenderer.Draw(spriteTexture, ref spriteTexture.ScreenPosition, ref color);
                    spriteRenderer.EndWithCustomProjection(context, ref texture.Projection2D);
                }
                
                //End Drawing
                texture.End(context, false);

                var tex2d = texture.CloneTexture(context, ResourceUsage.Default);

                //Create Shadow around Icon object displayed
                if (!profile.IsEmissiveColorLightSource)
                {
                    tex2d = DrawOuterShadow(context, texture, tex2d);
                }

                createdIconsTexture.Add(tex2d);
                _cubeIconIndexes.Add(profile.Id, (byte)(createdIconsTexture.Count - 1));
            }

            //Reset device Default render target
            _d3DEngine.SetRenderTargetsAndViewPort(context);

            //Dispose temp resource.
            spriteTexture.Dispose();
            shader.Dispose();
            vb.Dispose();
            ib.Dispose();
            spriteRenderer.Dispose();

            return createdIconsTexture;            
        }
        #endregion
        
    }
}