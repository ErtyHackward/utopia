using System;
using System.Drawing;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using SharpDX;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Settings;
using Color = SharpDX.Color;
using Utopia.Resources.Effects.Entities;

namespace Realms.Client.Components.GUI
{
    /// <summary>
    /// Component to show something while game loading
    /// </summary>
    public class LoadingComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private LabelControl _loadingLabel;
        private LabelControl _pleaseWaitLabel;
        

        private VertexBuffer<VertexMesh> _rotatingBlockVB;
        private IndexBuffer<ushort> _rotatingBlockIB;
        private RotationCube _loadingCube;
        private HLSLLoadingCube _cubeShader;

        private DateTime _dotsUpdate;
        private int _dots;

        public LoadingComponent(D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
            : base(engine, screen, commonResources)
        {
            _engine = engine;
            _screen = screen;
        }

        public override void Initialize()
        {
            base.Initialize();

            _loadingLabel = new LabelControl 
            {
                Text = "LOADING...",
                Color = Color.White,
                CustomFont = _commonResources.FontBebasNeue50
            };
            
            _pleaseWaitLabel = new LabelControl 
            {
                Text = "PLEASE WAIT",
                Color = new Color4(new Color(198, 0, 75, 255).ToRgba()),
                CustomFont = _commonResources.FontBebasNeue25
            };
            
            _cubeShader = ToDispose(new HLSLLoadingCube(_engine.Device,
                            ClientSettings.EffectPack + @"Entities/LoadingCube.hlsl",
                            VertexMesh.VertexDeclaration,
                            new S33M3DXEngine.Effects.HLSLFramework.EntryPoints() { VertexShader_EntryPoint = "VS", PixelShader_EntryPoint = "PSColoredCubeWithLight" }
                            ));

            //Create Vertex/Index Buffer to store the rotating Cube
            _rotatingBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                                       _meshBluePrint.Vertices.Length,
                                                                       SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                       "rotatingBlockVB"));
            _rotatingBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, _meshBluePrint.Indices.Length, "rotatingBlockIB"));

            _loadingCube = new RotationCube
            {
                Scale = 70,
                ScreenPosition = new Vector3((_engine.ViewPort.Width - 168) / 2, (_engine.ViewPort.Height - _headerHeight) / 2 + _headerHeight, 0),
                SpinningRotation = new Vector3(0.05f, 0.1f, 0.0f),
            };
        }

        public override void LoadContent(DeviceContext context)
        {
            base.LoadContent(context);

            //Set data inside VBuffer & IBuffer
            _rotatingBlockVB.SetData(context, _meshBluePrint.Vertices);
            _rotatingBlockIB.SetData(context, _meshBluePrint.Indices);

            YRotation.Value = MathHelper.PiOver4;
            XRotation.Value = MathHelper.Pi / 5.0f;
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            _dotsUpdate = DateTime.Now;
            _screen.Desktop.Children.Add(_loadingLabel);
            _screen.Desktop.Children.Add(_pleaseWaitLabel);
            Resize(_engine.ViewPort);
            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_loadingLabel);
            _screen.Desktop.Children.Remove(_pleaseWaitLabel);
            base.DisableComponent();
        }

        protected override void EngineViewPortUpdated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            base.EngineViewPortUpdated(viewport, newBackBuffer);

            Resize(viewport);
        }

        private void Resize(ViewportF viewport)
        {
            _loadingLabel.Bounds        = new UniRectangle((viewport.Width - 168) / 2 + 70, (viewport.Height - _headerHeight - 58) / 2 + _headerHeight - 10, 148, 58);
            _pleaseWaitLabel.Bounds     = new UniRectangle((viewport.Width - 168) / 2 + 70, (viewport.Height - _headerHeight - 58) / 2 + _headerHeight + 40, 148, 58);
            _loadingCube.ScreenPosition = new Vector3((_engine.ViewPort.Width - 168) / 2, (_engine.ViewPort.Height - _headerHeight) / 2 + _headerHeight, 0);
        }

        FTSValue<float> YRotation = new FTSValue<float>();
        FTSValue<float> XRotation = new FTSValue<float>();
        public override void FTSUpdate(S33M3DXEngine.Main.GameTime timeSpent)
        {
            _loadingCube.Update();

            if ((DateTime.Now - _dotsUpdate).TotalSeconds > 0.5f)
            {
                if (_dots++ == 3) _dots = 0;

                _loadingLabel.Text = "LOADING" + new string('.', _dots);
                _dotsUpdate = DateTime.Now;
            }
            
            base.FTSUpdate(timeSpent);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _loadingCube.Interpolation(interpolationLd);

            base.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(DeviceContext context, int index)
        {
            base.Draw(context, index);

            _rotatingBlockVB.SetToDevice(context, 0);
            _rotatingBlockIB.SetToDevice(context, 0);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_engine.Projection2D);
            _cubeShader.CBPerFrame.Values.DiffuseLightDirection = new Vector3(0.8f, 0.9f, 0.9f);
            _cubeShader.CBPerFrame.IsDirty = true;

            _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(_loadingCube.World);
            _cubeShader.CBPerDraw.Values.Color = _loadingCube.CubeColor;
            _cubeShader.CBPerDraw.IsDirty = true;
            _cubeShader.Apply(context);
            context.DrawIndexed(_rotatingBlockIB.IndicesCount, 0, 0);
        }

    }
}
