﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.Textures;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs.Helpers;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Responsible to draw cracks on the blocks
    /// </summary>
    public class CracksRenderer : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly IWorldChunks _worldChunks;
        private ShaderResourceView _cracksArray;

        private HLSLCracks _cubeEffect;
        private IMeshFactory _milkShapeMeshfactory;
        private VertexBuffer<VertexMesh> _cubeVb;
        private IndexBuffer<ushort> _cubeIb;
        private Mesh _cubeMeshBluePrint;

        public CracksRenderer(D3DEngine engine,
                              CameraManager<ICameraFocused> cameraManager,
                              IWorldChunks worldChunks)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (worldChunks == null) throw new ArgumentNullException("worldChunks");

            _engine = engine;
            _cameraManager = cameraManager;
            _worldChunks = worldChunks;

            this.DrawOrders.UpdateIndex(0, 1010);
        }

        public override void Initialize()
        {
            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
            //Prepare Textured Block rendering when equiped ==============================================================
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);
            
            base.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(_engine.Device, context, ClientSettings.TexturePack + @"Cracks/", @"ct*.png", TexturePackConfig.Current.Settings.enuSamplingFilter, "ArrayTexture_Cracks", out _cracksArray);
            ToDispose(_cracksArray);

            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(context.Device, _cubeMeshBluePrint.Vertices.Length, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Cracks Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(context.Device, _cubeMeshBluePrint.Indices.Length, "Cracks Block IB"));

            _cubeVb.SetData(context, _cubeMeshBluePrint.Vertices);
            _cubeIb.SetData(context, _cubeMeshBluePrint.Indices);

            _cubeEffect = ToDispose(new HLSLCracks(context.Device, ClientSettings.EffectPack + @"Terran/Cracks.hlsl", VertexMesh.VertexDeclaration));
            _cubeEffect.DiffuseTexture.Value = _cracksArray;
            _cubeEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);

            base.LoadContent(context);
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled);
            
            foreach (var chunk in _worldChunks.ChunksToDraw())
            {
                foreach (var pair in chunk.BlockData.GetTags().Where(p => p.Value is DamageTag && _worldChunks.IsEntityVisible(p.Key)))
                {
                    var tag = (DamageTag)pair.Value;

                    var pos = BlockHelper.ConvertToGlobal(chunk.Position, pair.Key);

                    _cubeEffect.Begin(context);
                    _cubeEffect.CBPerDraw.Values.TextureIndex = 5 - 5 * tag.Strength / tag.TotalStrength;
                    _cubeEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(1.01f) * Matrix.Translation(pos + new Vector3(0.5f)));
                    _cubeEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);                   
                    _cubeEffect.CBPerDraw.Values.LightColor = Color3.White;
                    _cubeEffect.CBPerDraw.IsDirty = true;
                    _cubeEffect.Apply(context);

                    _cubeVb.SetToDevice(context, 0);
                    _cubeIb.SetToDevice(context, 0);
                    
                    context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
                }
            }
            
            base.Draw(context, index);
        }

        public override void BeforeDispose()
        {
            _cracksArray.Dispose();
            base.BeforeDispose();
        }
    }
}
