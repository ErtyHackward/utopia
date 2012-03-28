using System;
using System.Drawing;
using System.Drawing.Text;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Settings;
using SharpDX.Direct3D11;
using UtopiaContent.Effects.Entities;
using S33M3CoreComponents.Meshes;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Meshes.Factories;
using Utopia.Settings;
using System.Collections.Generic;
using S33M3DXEngine.Textures;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.RenderStates;
using Utopia.GameDXStates;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Base component for utopia menu (display logo and background)
    /// </summary>
    public abstract class SandboxMenuComponent : DrawableGameComponent
    {
        private enum PixelShaderEntry
        {
            PSWhiteCube,
            PSTexturedCube
        }

        // common resources
        public static SpriteTexture StShadow;
        public static SpriteTexture StLogo;
        public static SpriteTexture StGameName;
        public static SpriteTexture StCubesPattern;
        public static SpriteTexture StLinenPattern;

        public static SpriteFont FontBebasNeue35;
        public static SpriteFont FontBebasNeue25;
        protected static PrivateFontCollection fontCollection;
        public static bool WithTexturedCubes = false;
        protected static CubeProfile _staticCube1Profile, _staticCube2Profile, _staticCube3Profile, _staticCube4Profile;


        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        protected int _headerHeight;

        protected ImageControl _linen;
        protected ImageControl _cubes;
        protected ImageControl _shadow;
        protected ImageControl _logo;
        protected ImageControl _version;

        //Resources used to display the Static cubes in the menu
        protected VertexBuffer<VertexMesh> _staticBlockVB;
        protected IndexBuffer<ushort> _staticBlockIB;
        protected HLSLIcons _cubeShader;
        protected Mesh _meshBluePrint;
        protected Matrix _view;
        private Matrix _worldC1, _worldC2, _worldC3, _worldC4;
        protected ShaderResourceView _cubeTexture;
        protected Dictionary<int, int> MaterialChangeMapping;
        private Mesh _staticCube1Mesh, _staticCube2Mesh, _staticCube3Mesh, _staticCube4Mesh;

        public static SpriteTexture LoadTexture(D3DEngine engine, string filePath)
        {
            return new SpriteTexture(engine.Device, filePath, new Vector2I());
        }

        public static void LoadCommonImages(D3DEngine engine)
        {
            if (StShadow != null)
                throw new InvalidOperationException("Common images already loaded");

            StShadow        = LoadTexture(engine, "Images\\shadow.png");
            StLogo          = LoadTexture(engine, "Images\\logo.png");
            StGameName      = LoadTexture(engine, "Images\\version.png");
            StCubesPattern  = LoadTexture(engine, "Images\\cubes.png");
            StLinenPattern  = LoadTexture(engine, "Images\\black-linen.png");

            fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile("Images\\BebasNeue.ttf");

            FontBebasNeue35 = new SpriteFont();
            FontBebasNeue35.Initialize(fontCollection.Families[0], 35, FontStyle.Regular, true, engine.Device);

            FontBebasNeue25 = new SpriteFont();
            FontBebasNeue25.Initialize(fontCollection.Families[0], 25, FontStyle.Regular, true, engine.Device);
        }

        protected SandboxMenuComponent(D3DEngine engine, MainScreen screen)
        {
            _engine = engine;
            _screen = screen;
            _engine.ViewPort_Updated += EngineViewPortUpdated;

            _linen = new ImageControl { Image = StLinenPattern };
            _cubes = new ImageControl { Image = StCubesPattern };
            _shadow = new ImageControl { Image = StShadow };
            _logo = new ImageControl { Image = StLogo };
            _version = new ImageControl { Image = StGameName };

            Resize(_engine.ViewPort);

            this.DrawOrders.UpdateIndex(0, int.MaxValue - 1);

        }

        public override void Initialize()
        {
            //Create the Block mesh
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            meshfactory.LoadMesh(@"\Meshes\block.txt", out _meshBluePrint, 0);

            _staticBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                           _meshBluePrint.Vertices.Length,
                                                           VertexMesh.VertexDeclaration,
                                                           SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                           "rotatingBlockVB"));
            _staticBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, _meshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "rotatingBlockIB"));

            PixelShaderEntry psEntry = WithTexturedCubes ? PixelShaderEntry.PSTexturedCube : PixelShaderEntry.PSWhiteCube;

            _cubeShader = ToDispose(new HLSLIcons(_engine.Device,
                       ClientSettings.EffectPack + @"Entities/LoadingCube.hlsl",
                       VertexMesh.VertexDeclaration,
                       new S33M3DXEngine.Effects.HLSLFramework.EntryPoints() { VertexShader_EntryPoint = "VS", PixelShader_EntryPoint = psEntry.ToString() }));

            Random rnd = new Random();

            if (_staticCube1Profile == null)
            {
                _staticCube1Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                while (_staticCube1Profile.IsEmissiveColorLightSource == true)
                {
                    _staticCube1Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                }

                _staticCube2Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                while (_staticCube2Profile.IsEmissiveColorLightSource == true || _staticCube2Profile.Id == _staticCube1Profile.Id)
                {
                    _staticCube2Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                }

                _staticCube3Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                while (_staticCube3Profile.IsEmissiveColorLightSource == true || _staticCube3Profile.Id == _staticCube1Profile.Id || _staticCube3Profile.Id == _staticCube2Profile.Id)
                {
                    _staticCube3Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                }

                _staticCube4Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                while (_staticCube4Profile.IsEmissiveColorLightSource == true || _staticCube4Profile.Id == _staticCube1Profile.Id || _staticCube4Profile.Id == _staticCube2Profile.Id || _staticCube4Profile.Id == _staticCube3Profile.Id)
                {
                    _staticCube4Profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];
                }
            }

            MaterialChangeMapping = new Dictionary<int, int>();
            MaterialChangeMapping.Add(0, 0); //Change the Back Texture Id
            MaterialChangeMapping.Add(1, 0); //Change the Front Texture Id
            MaterialChangeMapping.Add(2, 0); //Change the Bottom Texture Id
            MaterialChangeMapping.Add(3, 0); //Change the Top Texture Id
            MaterialChangeMapping.Add(4, 0); //Change the Left Texture Id
            MaterialChangeMapping.Add(5, 0); //Change the Right Texture Id

            MaterialChangeMapping[0] = _staticCube1Profile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _staticCube1Profile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _staticCube1Profile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _staticCube1Profile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _staticCube1Profile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _staticCube1Profile.Tex_Right; //Change the Right Texture Id
            _staticCube1Mesh = _meshBluePrint.Clone(MaterialChangeMapping);

            MaterialChangeMapping[0] = _staticCube2Profile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _staticCube2Profile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _staticCube2Profile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _staticCube2Profile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _staticCube2Profile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _staticCube2Profile.Tex_Right; //Change the Right Texture Id
            _staticCube2Mesh = _meshBluePrint.Clone(MaterialChangeMapping);

            MaterialChangeMapping[0] = _staticCube3Profile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _staticCube3Profile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _staticCube3Profile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _staticCube3Profile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _staticCube3Profile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _staticCube3Profile.Tex_Right; //Change the Right Texture Id
            _staticCube3Mesh = _meshBluePrint.Clone(MaterialChangeMapping);

            MaterialChangeMapping[0] = _staticCube4Profile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _staticCube4Profile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _staticCube4Profile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _staticCube4Profile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _staticCube4Profile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _staticCube4Profile.Tex_Right; //Change the Right Texture Id
            _staticCube4Mesh = _meshBluePrint.Clone(MaterialChangeMapping);
        }

        public override void LoadContent(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(_engine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTexture);
            ToDispose(_cubeTexture);

            _view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            _cubeShader.DiffuseTexture.Value = _cubeTexture;
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

            _cubeShader.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(-0.8f, -0.9f, 1.5f);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_engine.Projection2D);
            _cubeShader.CBPerFrame.IsDirty = true;

            //Cube 1 ==============
            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_worldC1);
            _cubeShader.CBPerDraw.IsDirty = true;

            _cubeShader.Apply(context);
            //Set the buffer to the device
            _staticBlockVB.SetData(context, _staticCube1Mesh.Vertices);
            _staticBlockIB.SetData(context, _staticCube1Mesh.Indices);
            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);

            //Cube 2 ==============
            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_worldC2);
            _cubeShader.CBPerDraw.IsDirty = true;

            _cubeShader.Apply(context);
            //Set the buffer to the device
            _staticBlockVB.SetData(context, _staticCube2Mesh.Vertices);
            _staticBlockIB.SetData(context, _staticCube2Mesh.Indices);
            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);

            //Cube 3 ==============
            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_worldC3);
            _cubeShader.CBPerDraw.IsDirty = true;

            _cubeShader.Apply(context);
            //Set the buffer to the device
            _staticBlockVB.SetData(context, _staticCube3Mesh.Vertices);
            _staticBlockIB.SetData(context, _staticCube3Mesh.Indices);
            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);

            //Cube 4 ==============
            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_worldC4);
            _cubeShader.CBPerDraw.IsDirty = true;

            _cubeShader.Apply(context);
            //Set the buffer to the device
            _staticBlockVB.SetData(context, _staticCube4Mesh.Vertices);
            _staticBlockIB.SetData(context, _staticCube4Mesh.Indices);
            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
        }

        public override void Dispose()
        {
            fontCollection.Dispose();
            _engine.ViewPort_Updated -= EngineViewPortUpdated;
            base.Dispose();
        }

        protected virtual void EngineViewPortUpdated(SharpDX.Direct3D11.Viewport viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {
            Resize(viewport);
        }

        public override void EnableComponent()
        {
            _screen.Desktop.Children.Add(_logo);
            _screen.Desktop.Children.Add(_version);
            _screen.Desktop.Children.Add(_shadow);
            _screen.Desktop.Children.Add(_cubes);
            _screen.Desktop.Children.Add(_linen);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_linen);
            _screen.Desktop.Children.Remove(_cubes);
            _screen.Desktop.Children.Remove(_shadow);
            _screen.Desktop.Children.Remove(_logo);
            _screen.Desktop.Children.Remove(_version);
            base.DisableComponent();
        }

        private void Resize(SharpDX.Direct3D11.Viewport viewport)
        {
            if (viewport.Height >= 620)
                _headerHeight = (int)(viewport.Height * 0.3f);
            else
                _headerHeight = Math.Abs((int)viewport.Height - 434);

            _cubes.Bounds = new UniRectangle(0, 0, viewport.Width, _headerHeight);
            _linen.Bounds = new UniRectangle(0, _headerHeight, viewport.Width, viewport.Height - _headerHeight);
            _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, viewport.Width, 287);
            _logo.Bounds = new UniRectangle((viewport.Width - 562) / 2, _headerHeight - 44, 562, 113);
            _version.Bounds = new UniRectangle((viewport.Width - 562) / 2 + 360, _headerHeight + 49, 196, 31);

            //Static Cube 1 Screen position
            _worldC1 = Matrix.RotationY(MathHelper.PiOver4) * Matrix.RotationX(-MathHelper.Pi * 6 / 5);
            _worldC1 *= Matrix.Scaling(_headerHeight * 0.4f);
            _worldC1 *= Matrix.Translation((_engine.ViewPort.Width) * 0.23f, (_headerHeight) / 2, 0);

            //Static Cube 2 Screen position
            _worldC2 = Matrix.RotationY(MathHelper.PiOver2 * 0.125f) * Matrix.RotationX(-MathHelper.PiOver2 * 6 * 1.2f) * Matrix.RotationZ(MathHelper.Pi * 1.23f);
            _worldC2 *= Matrix.Scaling(60);
            _worldC2 *= Matrix.Translation(60 + 15, (_headerHeight), 0);

            //Static Cube 3 Screen position
            _worldC3 = Matrix.RotationY(MathHelper.PiOver2 * 0.6f) * Matrix.RotationX(-MathHelper.PiOver2 * 1.9f) * Matrix.RotationZ(MathHelper.Pi * 1.3f);
            _worldC3 *= Matrix.Scaling(30);
            _worldC3 *= Matrix.Translation(30 + 15, (_engine.ViewPort.Height - _headerHeight) / 4 + _headerHeight, 0);

            //Static Cube 4 Screen position
            _worldC4 = Matrix.RotationY(MathHelper.PiOver2 * 0.9f) * Matrix.RotationX(-MathHelper.PiOver2 * 1.6f) * Matrix.RotationZ(MathHelper.Pi * 1.9f);
            _worldC4 *= Matrix.Scaling(40);
            _worldC4 *= Matrix.Translation((_engine.ViewPort.Width) / 7, 15, 0);
        }
    }
}
