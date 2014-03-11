using System;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Meshes;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Meshes.Factories;
using System.Collections.Generic;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Resources.Effects.Entities;

namespace Realms.Client.Components.GUI
{
    /// <summary>
    /// Base component for utopia menu (display logo and background)
    /// </summary>
    public abstract class SandboxMenuComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        protected readonly SandboxCommonResources _commonResources;

        protected int _headerHeight;

        protected ImageControl _linen;
        protected ImageControl _cubes;
        protected ImageControl _shadow;
        protected ImageControl _logo;
        protected ImageControl _version;

        //Resources used to display the Static cubes in the menu
        private List<RotationCube> _rotatingCubes = new List<RotationCube>();

        protected VertexBuffer<VertexMesh> _staticBlockVB;
        protected IndexBuffer<ushort> _staticBlockIB;
        private HLSLLoadingCube _cubeShader;
        protected Mesh _meshBluePrint;
        protected Matrix _view;
        protected int _borderOffset = 0;
        
        protected SandboxMenuComponent(D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
        {
            _engine = engine;
            _screen = screen;
            _commonResources = commonResources;
            _engine.ScreenSize_Updated += EngineViewPortUpdated;

            _linen = new ImageControl { Image = _commonResources.StLinenPattern };
            _cubes = new ImageControl { Image = _commonResources.StCubesPattern };
            _shadow = new ImageControl { Image = _commonResources.StShadow };
            _logo = new ImageControl { Image = _commonResources.StLogo };
            _version = new ImageControl { Image = _commonResources.StGameName };


            DrawOrders.UpdateIndex(0, int.MaxValue - 1);
        }

        public override void Initialize()
        {
            //Create the Block mesh
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            meshfactory.LoadMesh(@"\Meshes\block.txt", out _meshBluePrint, 0);

            _staticBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                           _meshBluePrint.Vertices.Length,
                                                           SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                           "rotatingBlockVB"));
            _staticBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, _meshBluePrint.Indices.Length, "rotatingBlockIB"));

            _cubeShader = ToDispose(new HLSLLoadingCube(_engine.Device,
                                        ClientSettings.EffectPack + @"Entities/LoadingCube.hlsl",
                                        VertexMesh.VertexDeclaration));

            RotationCube cube;
            //Create Cubes 1
            cube = new RotationCube
            {
                ID = 0,
                Rotation  = new Vector3(-MathHelper.Pi * 6 / 5, MathHelper.PiOver4, 0),
                SpinningRotation = new Vector3(0.01f,0.02f,0.0001f)
            };
            _rotatingCubes.Add(cube);

            cube = new RotationCube
            {
                ID = 1,
                Rotation = new Vector3(-MathHelper.PiOver2 * 6 * 1.2f, MathHelper.PiOver2 * 0.125f, MathHelper.Pi * 1.23f),
                SpinningRotation = new Vector3(0.01f, 0.01f, 0.001f)
            };
            _rotatingCubes.Add(cube);

            cube = new RotationCube
            {
                ID = 2,
                Rotation = new Vector3(-MathHelper.PiOver2 * 1.9f, MathHelper.PiOver2 * 0.6f, MathHelper.Pi * 1.3f),
                SpinningRotation = new Vector3(0.001f, 0.05f, 0.002f)
            };
            _rotatingCubes.Add(cube);

            cube = new RotationCube
            {
                ID = 3,
                Rotation = new Vector3(-MathHelper.PiOver2 * 1.6f, MathHelper.PiOver2 * 0.9f, MathHelper.Pi * 1.9f),
                SpinningRotation = new Vector3(0.001f, 0.0004f, 0.03f)
            };
            _rotatingCubes.Add(cube);

            Resize(_engine.ViewPort);
        }

        public override void LoadContent(DeviceContext context)
        {
            _view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

            _staticBlockVB.SetData(context, _meshBluePrint.Vertices);
            _staticBlockIB.SetData(context, _meshBluePrint.Indices);
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            foreach (RotationCube cube in _rotatingCubes)
            {
                cube.Update();
            }
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            foreach (RotationCube cube in _rotatingCubes)
            {
                cube.Interpolation(interpolationLd);
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_engine.Projection2D);
            _cubeShader.CBPerFrame.IsDirty = true;

            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);

            foreach (RotationCube cube in _rotatingCubes)
            {
                //Draw Shadow Cube
                _cubeShader.CBPerDraw.Values.Color = cube.CubeShadowColor;
                _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(cube.WorldShadow);
                _cubeShader.CBPerDraw.IsDirty = true;
                _cubeShader.Apply(context);
                context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
                //Draw Cube
                _cubeShader.CBPerDraw.Values.Color = cube.CubeColor;
                _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(cube.World);
                _cubeShader.CBPerDraw.IsDirty = true;
                _cubeShader.Apply(context);
                context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
            }
        }

        public override void BeforeDispose()
        {
            _engine.ScreenSize_Updated -= EngineViewPortUpdated;
        }

        protected virtual void EngineViewPortUpdated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            Resize(viewport);
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

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

        private void Resize(SharpDX.ViewportF viewport)
        {
            if (viewport.Height >= 620)
                _headerHeight = (int)(viewport.Height * 0.3f);
            else
                _headerHeight = Math.Abs((int)viewport.Height - 434);

            _cubes.Bounds = new UniRectangle(0 + _borderOffset, 0 + _borderOffset, viewport.Width - (_borderOffset * 2), _headerHeight);
            _linen.Bounds = new UniRectangle(0 + _borderOffset, _headerHeight, viewport.Width - (_borderOffset * 2), (viewport.Height - _headerHeight) - (_borderOffset * 2));
            _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, viewport.Width, 287);
            _logo.Bounds = new UniRectangle((viewport.Width - 562) / 2, _headerHeight - 44, 562, 113);
            _version.Bounds = new UniRectangle((viewport.Width - 490) / 2 + 360, _headerHeight + 49, 89, 31);

            foreach (RotationCube cube in _rotatingCubes)
            {
                switch (cube.ID)
                {
                    case 0:
                        cube.Scale = _headerHeight * 0.4f;
                        cube.ScreenPosition = new Vector3((_engine.ViewPort.Width) * 0.23f, (_headerHeight) / 2, 0);
                        break;
                    case 1:
                        cube.Scale = 60;
                        cube.ScreenPosition = new Vector3(cube.Scale + 15, (_headerHeight), 0);
                        break;
                    case 2:
                        cube.Scale = 30;
                        cube.ScreenPosition = new Vector3(cube.Scale + 15, (_engine.ViewPort.Height - _headerHeight) / 4 + _headerHeight, 0);
                        break;
                    case 3:
                        cube.Scale = 40;
                        cube.ScreenPosition = new Vector3((_engine.ViewPort.Width) / 7, 15, 0);
                        break;
                }
            }
        }

        public class RotationCube
        {
            public int ID;
            private FTSValue<Vector3> _rotation = new FTSValue<Vector3>();
            public Vector3 Rotation
            {
                set
                {
                    _rotation.Value = value;
                }
            }
            public float Scale;
            public Vector3 ScreenPosition;
            public Vector3 SpinningRotation;

            public Color4 CubeColor = Color.White;
            public Color4 CubeShadowColor = new Color4(0.05f, 0.05f, 0.05f, 0.02f);

            public Matrix World
            {
                get 
                {
                    Matrix w = Matrix.RotationY(_rotation.ValueInterp.Y) * Matrix.RotationX(_rotation.ValueInterp.X) * Matrix.RotationZ(_rotation.ValueInterp.Z);
                    w *= Matrix.Scaling(Scale);
                    w *= Matrix.Translation(ScreenPosition);
                    return w;
                }
            }

            public Matrix WorldShadow
            {
                get
                {
                    Matrix w = Matrix.RotationY(_rotation.ValueInterp.Y) * Matrix.RotationX(_rotation.ValueInterp.X) * Matrix.RotationZ(_rotation.ValueInterp.Z);
                    w *= Matrix.Scaling(Scale * 1.10f);
                    w *= Matrix.Translation(ScreenPosition);
                    return w;
                }
            }

            public void Update()
            {
                _rotation.BackUpValue();

                _rotation.Value += SpinningRotation;
            }

            public void Interpolation(float interpValue)
            {
                Vector3.Lerp(ref _rotation.ValuePrev, ref _rotation.Value, interpValue, out _rotation.ValueInterp);
            }

        }
    }
}
