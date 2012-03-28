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
        private Mesh _rotatingCubeMesh;
        private Matrix _world;
        private CubeProfile _rotatingCubeProfile;

        public LoadingComponent(D3DEngine engine, MainScreen screen)
            : base(engine, screen)
        {
            _engine = engine;
            _screen = screen;

            _stLoading = ToDispose(LoadTexture(engine, "Images\\loading.png"));

        }

        public override void Initialize()
        {
            base.Initialize();

            _loadingLabel = new ImageControl();
            _loadingLabel.Image = _stLoading;
           
            //Create Vertex/Index Buffer to store the rotating Cube
            _rotatingBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                                       _meshBluePrint.Vertices.Length,
                                                                       VertexMesh.VertexDeclaration,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "rotatingBlockVB"));
            _rotatingBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, _meshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "rotatingBlockIB"));

            Random rnd = new Random();
            _rotatingCubeProfile = GameSystemSettings.Current.Settings.CubesProfile[rnd.Next(GameSystemSettings.Current.Settings.CubesProfile.Length)];

            //Here the key parameter is the ID name given to the texture inside the file model.
            //In our case the model loaded has these Materials/texture Ids :
            // 0 = Back
            // 1 = Front
            // 2 = Bottom
            // 3 = Top
            // 4 = Left
            // 5 = Right
            //The value attached to it is simply the TextureID from the texture array to use.
            MaterialChangeMapping[0] = _rotatingCubeProfile.Tex_Back; //Change the Back Texture Id
            MaterialChangeMapping[1] = _rotatingCubeProfile.Tex_Front; //Change the Front Texture Id
            MaterialChangeMapping[2] = _rotatingCubeProfile.Tex_Bottom; //Change the Bottom Texture Id
            MaterialChangeMapping[3] = _rotatingCubeProfile.Tex_Top; //Change the Top Texture Id
            MaterialChangeMapping[4] = _rotatingCubeProfile.Tex_Left; //Change the Left Texture Id
            MaterialChangeMapping[5] = _rotatingCubeProfile.Tex_Right; //Change the Right Texture Id

            _rotatingCubeMesh = _meshBluePrint.Clone(MaterialChangeMapping);
        }

        public override void LoadContent(DeviceContext context)
        {
            base.LoadContent(context);

            //Set data inside VBuffer & IBuffer
            _rotatingBlockVB.SetData(context, _rotatingCubeMesh.Vertices);
            _rotatingBlockIB.SetData(context, _rotatingCubeMesh.Indices);

            YRotation.Value = MathHelper.PiOver4;
            XRotation.Value = MathHelper.Pi / 5.0f;
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
            _loadingLabel.Bounds = new UniRectangle((viewport.Width - 168) / 2 + 70, (viewport.Height - _headerHeight - 58) / 2 + _headerHeight, 148, 58);
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
            _world *= Matrix.Scaling(70);
            _world = _world * Matrix.Translation((_engine.ViewPort.Width - 168) / 2, (_engine.ViewPort.Height - _headerHeight) / 2 + _headerHeight, 0);

            base.Interpolation(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(DeviceContext context, int index)
        {
            base.Draw(context, index);

            _cubeShader.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(-0.8f, -0.9f, 1.5f);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_engine.Projection2D);
            _cubeShader.CBPerFrame.IsDirty = true;

            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_world);
            _cubeShader.CBPerDraw.IsDirty = true;

            _cubeShader.Apply(context);
            //Set the buffer to the device
            _rotatingBlockVB.SetToDevice(context, 0);
            _rotatingBlockIB.SetToDevice(context, 0);
            context.DrawIndexed(_rotatingBlockIB.IndicesCount, 0, 0);

        }

    }
}
