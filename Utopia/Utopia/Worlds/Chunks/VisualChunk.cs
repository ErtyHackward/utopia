using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Planets.Terran.Chunk;
using S33M3Engines.Threading;
using Amib.Threading;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.Buffers;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser, IThreadStatus, IDisposable
    {
        #region Private variables
        private WorldChunks _world;
        private Range<int> _cubeRange;
        #endregion

        #region Public properties/Variable
        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        public Dictionary<string, int> CubeVerticeDico; // Dictionnary used in the mesh creation, to avoid to recreate a vertex that has already been used create for another cube.
        public List<VertexCubeSolid> SolidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        public List<ushort> SolidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        public List<VertexCubeLiquid> LiquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        public List<ushort> LiquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time

        //Graphical chunk components Exposed VB and IB ==> Called a lot, so direct acces without property bounding
        public VertexBuffer<VertexCubeSolid> SolidCubeVB;   //Solid cube vertex Buffer
        public IndexBuffer<ushort> SolidCubeIB;             //Solid cube index buffer
        public VertexBuffer<VertexCubeLiquid> LiquidCubeVB; //Liquid cube vertex Buffer
        public IndexBuffer<ushort> LiquidCubeIB;            //Liquid cube index Buffer

        public IntVector2 ChunkPosition { get; private set; } // Gets or sets current chunk position
        public ChunkState State { get; set; }                 // Chunk State
        public ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public WorkItemPriority ThreadPriority { get; set; }  // Thread Priority value
        public int UserChangeOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool BorderChunk { get; set; }                 // Set to true if the chunk is located at the border of the visible world !
        public bool Ready2Draw { get; set; }                  // Whenever the chunk mesh are ready to be rendered to screen
        public bool isFrustumCulled { get; set; }             // Chunk Frustum culled

        public Matrix World;                                  // The chunk World matrix ==> Not a property, to be sure it will be direct variables acces !!
        public BoundingBox ChunkWorldBoundingBox;             // The chunk World BoundingBox ==> Not a property, to be sure it will be direct variables acces !!

        public object Lock_DrawChunksSolidFaces = new object();       //Multithread Locker
        public object Lock_DrawChunksSeeThrough1Faces = new object(); //Multithread Locker

        public Range<int> CubeRange
        {
            get { return _cubeRange; }
            set
            {
                _cubeRange = value;
                RangeChanged();
            }
        }
        #endregion

        public VisualChunk(WorldChunks world, ref Range<int> cubeRange, SingleArrayChunkContainer singleArrayContainer)
            : base(new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor

            _world = world;
            CubeRange = cubeRange;
            State = ChunkState.Empty;
            Ready2Draw = false;
        }

        #region Public methods
        /// <summary>
        /// Is my chunk at the edge of the visible world ?
        /// </summary>
        /// <param name="X">Chunk world X position</param>
        /// <param name="Z">Chunk world Z position</param>
        /// <returns>True if the chunk is at border</returns>
        public bool isBorderChunk(int X, int Z)
        {
            if (X == _world.WorldRange.Min.X ||
               Z == _world.WorldRange.Min.Z ||
               X == _world.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               Z == _world.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }


        //Graphical Part
        public void InitializeChunkBuffers()
        {
            CubeVerticeDico = new Dictionary<string, int>();
            SolidCubeVertices = new List<VertexCubeSolid>();
            SolidCubeIndices = new List<ushort>();
            LiquidCubeVertices = new List<VertexCubeLiquid>();
            LiquidCubeIndices = new List<ushort>();
        }

        #endregion

        #region Privates Methods

        private void RefreshWorldMatrix()
        {
            Matrix.Translation(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z, out World); //Create a matrix for world translation

            //Refresh the bounding Box to make it in world coord.
            ChunkWorldBoundingBox.Minimum = new Vector3(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z);
            ChunkWorldBoundingBox.Maximum = new Vector3(_cubeRange.Max.X, _cubeRange.Max.Y, _cubeRange.Max.Z);
        }

        private void RangeChanged() // Start it also if the World offset Change !!!
        {
            ChunkPosition = new IntVector2() { X = _cubeRange.Min.X, Y = _cubeRange.Min.Z };
            BorderChunk = _world.isBorderChunk(ChunkPosition);

            RefreshWorldMatrix();
        }

        #endregion

        protected override void BlockBufferChanged(object sender, ChunkDataProviderBufferChangedEventArgs e)
        {
            State = ChunkState.LandscapeCreated;
            base.BlockBufferChanged(sender, e);
        }

        protected override void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            State = ChunkState.LandscapeCreated;
            base.BlockDataChanged(sender, e);
        }

        public void Dispose()
        {
        }
    }
}
