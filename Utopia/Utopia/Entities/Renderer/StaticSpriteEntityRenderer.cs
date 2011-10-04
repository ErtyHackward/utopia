using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Renderer.Interfaces;
using S33M3Engines.Cameras;
using S33M3Engines;
using Utopia.Worlds.SkyDomes;
using S33M3Engines.WorldFocus;
using Utopia.Shared.World;
using Utopia.Entities.Voxel;
using SharpDX.Direct3D11;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Textures;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using SharpDX;
using SharpDX.Direct3D;
using S33M3Engines.Maths;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Renderer
{
    public class StaticSpriteEntityRenderer : IStaticSpriteEntityRenderer
    {
        #region Private variables
        private HLSLStaticEntitySpriteInstanced _entitySpriteInstanced;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private ShaderResourceView _spriteTexture_View;
        private ISkyDome _skydome;
        private VisualWorldParameters _visualWorldParameters;
        private List<VertexPositionColorTextureInstanced> _vertices; //Default to support a maximum of 50.000 elements !

        private InstancedVertexBuffer<VertexPositionTexture, VertexPositionColorTextureInstanced> _staticSpriteBuffer;
        private IndexBuffer<short> _staticSpriteIndices;
        #endregion

        #region Public variables/properties
        #endregion

        public StaticSpriteEntityRenderer(D3DEngine d3dEngine,
                                    CameraManager camManager,
                                    WorldFocusManager worldFocusManager,
                                    ISkyDome skydome,
                                    VisualWorldParameters visualWorldParameters)
        {
            _d3dEngine = d3dEngine;
            _camManager = camManager;
            _skydome = skydome;
            _visualWorldParameters = visualWorldParameters;
            _worldFocusManager = worldFocusManager;
            Initialize();   
        }

        #region Private Methods
        private void Initialize()
        {
            _entitySpriteInstanced = new HLSLStaticEntitySpriteInstanced(_d3dEngine, @"Effects/Entities/StaticEntitySpriteInstanced.hlsl", VertexPositionColorTextureInstanced.VertexDeclaration);
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Sprites/", @"sp*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _spriteTexture_View);
            _entitySpriteInstanced.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
            _entitySpriteInstanced.DiffuseTexture.Value = _spriteTexture_View;

            _vertices = new List<VertexPositionColorTextureInstanced>(20000);
            CreateStaticSpriteBuffer();
        }

        private void CreateStaticSpriteBuffer()
        {
            //Create the base mesh that will be instanced.
            //Create the vertex buffer
            VertexPositionTexture[] vertices = { 
                                          //First Quad
                                          new VertexPositionTexture(new Vector3(-0.5f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)),
                                          new VertexPositionTexture(new Vector3(-0.5f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)),
                                          new VertexPositionTexture(new Vector3(0.5f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f)),
                                          new VertexPositionTexture(new Vector3(0.5f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                                          //Add more quad here
                                      };
            //Create a new InstancendeVertexBuffer
            _staticSpriteBuffer = new InstancedVertexBuffer<VertexPositionTexture, VertexPositionColorTextureInstanced>(_d3dEngine, VertexPositionColorTextureInstanced.VertexDeclaration, PrimitiveTopology.TriangleList);
            _staticSpriteBuffer.SetFixedData(vertices);

            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _staticSpriteIndices = new IndexBuffer<short>(_d3dEngine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "StaticEntityManagerInd");
            _staticSpriteIndices.SetData(indices);
             
        }

        #endregion

        #region Public Methods
        public void Draw(int Index)
        {
            if (_vertices.Count == 0) return;

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _entitySpriteInstanced.Begin();

            _entitySpriteInstanced.CBPerFrame.Values.WorldFocus = Matrix.Transpose(_worldFocusManager.CenterOnFocus(ref MMatrix.Identity));
            _entitySpriteInstanced.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _entitySpriteInstanced.CBPerFrame.Values.WindPower = 1; //TextureAnimationOffset.ValueInterp;
            _entitySpriteInstanced.CBPerFrame.Values.SunColor = _skydome.SunColor;
            _entitySpriteInstanced.CBPerFrame.Values.fogdist = ((_visualWorldParameters.WorldVisibleSize.X) / 2) - 48; ;
            _entitySpriteInstanced.CBPerFrame.IsDirty = true;

            _entitySpriteInstanced.Apply();

            _staticSpriteIndices.SetToDevice(0);
            _staticSpriteBuffer.SetToDevice(0);
            _d3dEngine.Context.DrawIndexedInstanced(_staticSpriteIndices.IndicesCount, _vertices.Count, 0, 0, 0);
        }

        public void BeginSpriteCollectionRefresh()
        {
            _vertices.Clear();
        }

        public void AddPointSpriteVertex(VisualSpriteEntity spriteVertex)
        {
            //_vertices.Add(spriteVertex.Vertex);
        }

        public void EndSpriteCollectionRefresh()
        {
            //Udpate the Dynamic instanced Vertex Buffer
            if (_vertices.Count == 0) return;
            _staticSpriteBuffer.SetInstancedData(_vertices.ToArray());
        }

        public void Update(ref GameTime timeSpent)
        {
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public void Dispose()
        {
            _vertices = null;
            _spriteTexture_View.Dispose();
            _entitySpriteInstanced.Dispose();

            _staticSpriteBuffer.Dispose();
            _staticSpriteIndices.Dispose();
            _entitySpriteInstanced.Dispose();
        }
        #endregion
    }
}
