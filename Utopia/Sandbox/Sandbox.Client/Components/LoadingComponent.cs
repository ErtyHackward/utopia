using System;
using System.Drawing;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using S33M3CoreComponents.Meshes.Factories;
using S33M3CoreComponents.Meshes;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using UtopiaContent.Effects.Entities;
using Utopia.Settings;
using S33M3DXEngine.RenderStates;
using Utopia.GameDXStates;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Settings;
using S33M3DXEngine.Textures;
using System.Collections.Generic;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Component to show something while game loading
    /// </summary>
    public class LoadingComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private ImageControl _loadingLabel;
        private SpriteTexture _stLoading;

        private VertexBuffer<VertexMesh> _rotatingBlockVB;
        private IndexBuffer<ushort> _rotatingBlockIB;
        private HLSLIcons _shader;
        private Mesh _meshBluePrint;
        private Matrix _view;
        private Matrix _world;
        private CubeProfile _profile;
        private ShaderResourceView _cubeTexture;

        public LoadingComponent(D3DEngine engine, MainScreen screen) : base(engine, screen)
        {
            _engine = engine;
            _screen = screen;

            _stLoading = ToDispose(LoadTexture(engine, "Images\\loading.png"));

            this.DrawOrders.UpdateIndex(0, int.MaxValue);
        }

        public override void Initialize()
        {
            _loadingLabel = new ImageControl();
            _loadingLabel.Image = _stLoading;

            //Create the Block mesh
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();

            meshfactory.LoadMesh(@"\Meshes\block.txt", out _meshBluePrint, 0);
            //Create Vertex/Index Buffer to store the loaded mesh.
            _rotatingBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                                       _meshBluePrint.Vertices.Length,
                                                                       VertexMesh.VertexDeclaration,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "Block VB"));
            _rotatingBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, _meshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB"));

            _shader = ToDispose(new HLSLIcons(_engine.Device,
                                   ClientSettings.EffectPack + @"Entities/Icons.hlsl",
                                   VertexMesh.VertexDeclaration,
                                   new S33M3DXEngine.Effects.HLSLFramework.EntryPoints() { VertexShader_EntryPoint = "VS", PixelShader_EntryPoint="PS2D" }));

            Random rnd = new Random();
            _profile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];

            Dictionary<int, int> MaterialChangeMapping = new Dictionary<int, int>();
            MaterialChangeMapping.Add(0, 0); //Change the Back Texture Id
            MaterialChangeMapping.Add(1, 0); //Change the Front Texture Id
            MaterialChangeMapping.Add(2, 0); //Change the Bottom Texture Id
            MaterialChangeMapping.Add(3, 0); //Change the Top Texture Id
            MaterialChangeMapping.Add(4, 0); //Change the Left Texture Id
            MaterialChangeMapping.Add(5, 0); //Change the Right Texture Id

            //Here the key parameter is the ID name given to the texture inside the file model.
            //In our case the model loaded has these Materials/texture Ids :
            // 0 = Back
            // 1 = Front
            // 2 = Bottom
            // 3 = Top
            // 4 = Left
            // 5 = Right
            //The value attached to it is simply the TextureID from the texture array to use.
            MaterialChangeMapping[0] = _profile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _profile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _profile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _profile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _profile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _profile.Tex_Right; //Change the Right Texture Id

            _meshBluePrint.ChangeMaterialMapping(MaterialChangeMapping);
        }

        public override void LoadContent(DeviceContext context)
        {
            //Set data inside VBuffer & IBuffer
            _rotatingBlockVB.SetData(context, _meshBluePrint.Vertices);
            _rotatingBlockIB.SetData(context, _meshBluePrint.Indices);

            ArrayTexture.CreateTexture2DFromFiles(_engine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTexture);
            ToDispose(_cubeTexture);

            _shader.DiffuseTexture.Value = _cubeTexture;

            _view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            YRotation.Value = MathHelper.PiOver4;
            XRotation.Value = MathHelper.Pi / 5.0f;

            base.LoadContent(context);
        }

        public override void EnableComponent()
        {
            _screen.Desktop.Children.Add(_loadingLabel);
            Resize(_engine.ViewPort);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_loadingLabel);
            base.DisableComponent();
        }

        protected override void EngineViewPortUpdated(Viewport viewport, Texture2DDescription newBackBuffer)
        {
            base.EngineViewPortUpdated(viewport, newBackBuffer);

            Resize(viewport);
        }

        private void Resize(Viewport viewport)
        {
            _loadingLabel.Bounds = new UniRectangle((viewport.Width - 148) / 2 + 50, (viewport.Height - _headerHeight - 58) / 2 + _headerHeight, 148, 58);
        }

        FTSValue<float> YRotation = new FTSValue<float>();
        FTSValue<float> XRotation = new FTSValue<float>();
        public override void Update(S33M3DXEngine.Main.GameTime timeSpent)
        {
            YRotation.BackUpValue();
            XRotation.BackUpValue();

            YRotation.Value += 0.1f;
            XRotation.Value += 0.05f;

            base.Update(timeSpent);
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            YRotation.ValueInterp = MathHelper.Lerp(YRotation.ValuePrev, YRotation.Value, interpolationLd);
            XRotation.ValueInterp = MathHelper.Lerp(XRotation.ValuePrev, XRotation.Value, interpolationLd);
            //Compute projection + View matrix

            _world = Matrix.RotationY(YRotation.ValueInterp) * Matrix.RotationX(XRotation.ValueInterp);

            if (_profile.YBlockOffset > 0)
            {
                _world *= Matrix.Scaling(1, 1.0f - _profile.YBlockOffset, 1);
            }
            else
            {
                _world *= Matrix.Scaling(70);
            }

            _world = _world * Matrix.Translation(_engine.ViewPort.Width / 2, _engine.ViewPort.Height / 2, 0);

            base.Interpolation(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(DeviceContext context, int index)
        {

            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

            _shader.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);

            _shader.Begin(context);
            _shader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(-0.8f, -0.9f, 1.5f);
            _shader.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _shader.CBPerFrame.Values.Projection = Matrix.Transpose(_engine.Projection2D);
            _shader.CBPerFrame.IsDirty = true;

            _shader.CBPerDraw.Values.World = Matrix.Transpose(_world);
            _shader.CBPerDraw.IsDirty = true;

            _shader.Apply(context);
            //Set the buffer to the device
            _rotatingBlockVB.SetToDevice(context, 0);
            _rotatingBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_rotatingBlockIB.IndicesCount, 0, 0);

            base.Draw(context, index);
        }

    }
}
