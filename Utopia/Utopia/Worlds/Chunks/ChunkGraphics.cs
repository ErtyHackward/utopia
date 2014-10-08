using System;
using System.Collections.Generic;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Resources.VertexFormats;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Contains chunk visual information. Vertices, indices and other graphics stuff
    /// </summary>
    public class ChunkGraphics : IDisposable
    {
        private readonly VisualChunkBase _parentChunk;
        private readonly D3DEngine _d3dEngine;
        private bool _disposed;

        private bool _ready2Draw;

        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        public List<VertexCubeSolid> SolidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        public List<ushort> SolidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        public List<VertexCubeLiquid> LiquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        public List<ushort> LiquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time

        //Graphical chunk components Exposed VB and IB ==> Called a lot, so direct acces without property bounding
        public VertexBuffer<VertexCubeSolid> SolidCubeVB;   //Solid cube vertex Buffer
        public IndexBuffer<ushort> SolidCubeIB;             //Solid cube index buffer
        public VertexBuffer<VertexCubeLiquid> LiquidCubeVB; //Liquid cube vertex Buffer
        public IndexBuffer<ushort> LiquidCubeIB;            //Liquid cube index Buffer

        /// <summary>
        /// Desired slice of the mesh
        /// </summary>
        public int SliceValue { get; set; }

        /// <summary>
        /// Actual slice value of the chunk mesh
        /// </summary>
        public int SliceOfMesh { get; set; }

        /// <summary>
        /// Whenever the chunk mesh are ready to be rendered to screen
        /// </summary>
        public bool IsExistingMesh4Drawing
        {
            get { return _ready2Draw; }
            internal set
            {
                if (_ready2Draw != value)
                {
                    _ready2Draw = value;
                    if (_ready2Draw) 
                        _parentChunk.OnReadyToDraw(); //Event raised when the chunk is full ready to be rendered
                }
            }
        }

        /// <summary>
        /// Indicates if the chunk is not visible from the camera and therefore should not be rendred
        /// </summary>
        public bool IsFrustumCulled { get; set; }

        /// <summary>
        /// Gets value indicating if the chunk can and should be rendered
        /// </summary>
        public bool NeedToRender { get { return IsExistingMesh4Drawing && !IsFrustumCulled; } }

        public ChunkGraphics(VisualChunkBase parentChunk, D3DEngine d3DEngine)
        {
            SliceValue = -1;
            _parentChunk = parentChunk;
            _d3dEngine = d3DEngine;
        }

        public void InitializeChunkBuffers()
        {
            SolidCubeVertices = new List<VertexCubeSolid>();
            SolidCubeIndices = new List<ushort>();
            LiquidCubeVertices = new List<VertexCubeLiquid>();
            LiquidCubeIndices = new List<ushort>();
        }

        public void SendCubeMeshesToBuffers()
        {
            if (_disposed) return;

            SendSolidCubeMeshToGraphicCard();       //Solid Cubes
            SendLiquidCubeMeshToGraphicCard();      //See Through Cubes

            _parentChunk.State = ChunkState.DisplayInSyncWithMeshes;
            IsExistingMesh4Drawing = true;
            SliceOfMesh = SliceValue;
        }

        //Solid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendSolidCubeMeshToGraphicCard()
        {
            if (SolidCubeVertices.Count == 0)
            {
                if (SolidCubeVB != null) SolidCubeVB.Dispose();
                SolidCubeVB = null;
                return;
            }

            if (SolidCubeVB == null)
            {
                SolidCubeVB = new VertexBuffer<VertexCubeSolid>(_d3dEngine.Device, SolidCubeVertices.Count, PrimitiveTopology.TriangleList, "SolidCubeVB", ResourceUsage.Default, 10);
            }
            SolidCubeVB.SetData(_d3dEngine.ImmediateContext, SolidCubeVertices.ToArray());
            SolidCubeVertices.Clear();

            if (SolidCubeIB == null)
            {
                SolidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, SolidCubeIndices.Count, "SolidCubeIB");
            }
            SolidCubeIB.SetData(_d3dEngine.ImmediateContext, SolidCubeIndices.ToArray());
            SolidCubeIndices.Clear();
        }

        //Liquid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendLiquidCubeMeshToGraphicCard()
        {
            if (LiquidCubeVertices.Count == 0)
            {
                if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
                LiquidCubeVB = null;
                return;
            }

            if (LiquidCubeVB == null)
            {
                LiquidCubeVB = new VertexBuffer<VertexCubeLiquid>(_d3dEngine.Device, LiquidCubeVertices.Count, PrimitiveTopology.TriangleList, "LiquidCubeVB", ResourceUsage.Default, 10);
            }
            LiquidCubeVB.SetData(_d3dEngine.ImmediateContext, LiquidCubeVertices.ToArray());
            LiquidCubeVertices.Clear();

            if (LiquidCubeIB == null)
            {
                LiquidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, LiquidCubeIndices.Count, "LiquidCubeIB");
            }
            LiquidCubeIB.SetData(_d3dEngine.ImmediateContext, LiquidCubeIndices.ToArray());
            LiquidCubeIndices.Clear();
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawSolidFaces(DeviceContext context)
        {
            if (_disposed) return;

            if (SolidCubeVB != null)
            {
                SolidCubeVB.SetToDevice(context, 0);
                SolidCubeIB.SetToDevice(context, 0);
                context.DrawIndexed(SolidCubeIB.IndicesCount, 0, 0);
            }
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawLiquidFaces(DeviceContext context)
        {
            if (_disposed) return;

            if (LiquidCubeVB != null)
            {
                LiquidCubeVB.SetToDevice(context, 0);
                LiquidCubeIB.SetToDevice(context, 0);
                context.DrawIndexed(LiquidCubeIB.IndicesCount, 0, 0);
            }
        }

        public void Dispose()
        {
            if (SolidCubeVB != null) SolidCubeVB.Dispose();
            if (SolidCubeIB != null) SolidCubeIB.Dispose();
            if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
            if (LiquidCubeIB != null) LiquidCubeIB.Dispose();
            _disposed = true;
        }
    }
}
