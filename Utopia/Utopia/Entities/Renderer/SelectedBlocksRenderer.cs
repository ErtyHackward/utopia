using System;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders selection by the god entity
    /// </summary>
    public class SelectedBlocksRenderer : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly ICameraManager _cameraManager;
        private readonly IPlayerManager _playerManager;

        protected VertexBuffer<VertexMesh> _staticBlockVB;
        protected IndexBuffer<ushort> _staticBlockIB;
        private HLSLLoadingCube _cubeShader;
        protected Mesh _meshBluePrint;

        public SelectedBlocksRenderer(D3DEngine engine,
                                      ICameraManager cameraManager,
                                      IPlayerManager playerManager)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (playerManager == null) throw new ArgumentNullException("playerManager");

            _engine = engine;
            _cameraManager = cameraManager;
            _playerManager = playerManager;
        }

        public override void Initialize()
        {
            IMeshFactory meshfactory = new MilkShape3DMeshFactory();
            meshfactory.LoadMesh(@"\Meshes\block.txt", out _meshBluePrint, 0);

            _staticBlockVB = ToDispose(new VertexBuffer<VertexMesh>(_engine.Device,
                                                                    _meshBluePrint.Vertices.Length,
                                                                    SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                                                                    "rotatingBlockVB"));

            _staticBlockIB = ToDispose(new IndexBuffer<ushort>(_engine.Device, 
                                                               _meshBluePrint.Indices.Length, 
                                                               "rotatingBlockIB"));

            _cubeShader = ToDispose(new HLSLLoadingCube(_engine.Device,
                                                        ClientSettings.EffectPack + @"Entities/LoadingCube.hlsl",
                                                        VertexMesh.VertexDeclaration));

            base.Initialize();
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _staticBlockVB.SetData(context, _meshBluePrint.Vertices);
            _staticBlockIB.SetData(context, _meshBluePrint.Indices);

            base.LoadContent(context);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            var playerManager = _playerManager as GodEntityManager;

            if (playerManager == null)
                throw new InvalidOperationException("Use this component only with GodEntityManager or disable it");

            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthDisabled);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_cameraManager.ActiveBaseCamera.View);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_cameraManager.ActiveBaseCamera.Projection);
            _cubeShader.CBPerFrame.IsDirty = true;

            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);

            _cubeShader.CBPerDraw.Values.Color = new Color4(0, 0, 150, 60);

            foreach (var selectedBlock in playerManager.FocusEntity.SelectedBlocks)
            {
                _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Translation(selectedBlock) * Matrix.Scaling(1.01f));
                _cubeShader.CBPerDraw.IsDirty = true;
                _cubeShader.Apply(context);
                context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
            }

            base.Draw(context, index);
        }
    }
}
