using System;
using System.Collections.Generic;
using System.Linq;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders selection by the god entity
    /// </summary>
    public class SelectedBlocksRenderer : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly IPlayerManager _playerManager;
        private readonly SingleArrayChunkContainer _cubesHolder;

        protected VertexBuffer<VertexMesh> _staticBlockVB;
        protected IndexBuffer<ushort> _staticBlockIB;
        private HLSLLoadingCube _cubeShader;
        protected Mesh _meshBluePrint;

        public SelectedBlocksRenderer(D3DEngine engine,
                                      CameraManager<ICameraFocused> cameraManager,
                                      IPlayerManager playerManager,
                                      SingleArrayChunkContainer cubesHolder)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (playerManager == null) throw new ArgumentNullException("playerManager");
            if (cubesHolder == null) throw new ArgumentNullException("cubesHolder");

            _engine = engine;
            _cameraManager = cameraManager;
            _playerManager = playerManager;
            _cubesHolder = cubesHolder;

            this.DrawOrders.UpdateIndex(0, 1020);
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

            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled);

            _cubeShader.Begin(context);
            _cubeShader.CBPerFrame.Values.View = Matrix.Transpose(_cameraManager.ActiveCamera.View);
            _cubeShader.CBPerFrame.Values.Projection = Matrix.Transpose(_cameraManager.ActiveCamera.Projection);
            _cubeShader.CBPerFrame.IsDirty = true;

            _staticBlockVB.SetToDevice(context, 0);
            _staticBlockIB.SetToDevice(context, 0);

            _cubeShader.CBPerDraw.Values.Color = new Color4(0, 0, 1f, 0.2f);

            var range = playerManager.HoverRange;

            var select = playerManager.Selection;

            var hoverBlocks = range.HasValue && select ? range.Value.ToList() : null;
            
            foreach (var selectedBlock in playerManager.Faction.BlocksToRemove)
            {
                if (hoverBlocks != null && hoverBlocks.Contains(selectedBlock))
                    hoverBlocks.Remove(selectedBlock);

                if (!select && range.HasValue && range.Value.Contains(selectedBlock))
                    continue;

                var cube = _cubesHolder.GetCube(selectedBlock);

                if (cube.IsValid && cube.Cube.Id != WorldConfiguration.CubeId.Air)
                {
                    _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(1.01f) * Matrix.Translation(selectedBlock + new Vector3(0.5f)));
                    _cubeShader.CBPerDraw.IsDirty = true;
                    _cubeShader.Apply(context);
                    context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
                }
            }

            if (hoverBlocks != null)
            {
                foreach (var hoverBlock in hoverBlocks)
                {
                    var cube = _cubesHolder.GetCube(hoverBlock);

                    if (cube.IsValid && cube.Cube.Id != WorldConfiguration.CubeId.Air)
                    {
                        _cubeShader.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(1.01f) * Matrix.Translation(hoverBlock + new Vector3(0.5f)));
                        _cubeShader.CBPerDraw.IsDirty = true;
                        _cubeShader.Apply(context);
                        context.DrawIndexed(_staticBlockIB.IndicesCount, 0, 0);
                    }
                }
            }

            base.Draw(context, index);
        }
    }
}
