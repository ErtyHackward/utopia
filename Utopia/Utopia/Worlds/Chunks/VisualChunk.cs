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

        Dictionary<string, int> _solidCubeVerticeDico; // Dictionnary used in the mesh creation, to avoid to recreate a vertex that has already been used create for another cube.

        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        List<VertexCubeSolid> _solidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        List<ushort> _solidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        List<VertexCubeLiquid> _liquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        List<ushort> _liquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time
        #endregion

        #region Public properties/Variable
        public IntVector2 ChunkPosition { get; private set; } // Gets or sets current chunk position
        public ChunkState State { get; set; }                 // Chunk State
        public ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public WorkItemPriority ThreadPriority { get; set; }  // Thread Priority value
        public int UserChangeOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool BorderChunk { get; set; }                 // Set to true if the chunk is located at the border of the visible world !

        public Range<int> CubeRange
        {
            get { return _cubeRange; }
            set
            {
                _cubeRange = value;
                ChunkPosition = new IntVector2() { X = _cubeRange.Min.X, Y = _cubeRange.Min.Z };
                BorderChunk = _world.isBorderChunk(ChunkPosition);
            }
        }
        #endregion

        public VisualChunk(WorldChunks world, ref Range<int> cubeRange, SingleArrayChunkContainer singleArrayContainer)
            : base(new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor

            CubeRange = cubeRange;
            State = ChunkState.Empty;
            _world = world;
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
            _solidCubeVerticeDico = new Dictionary<string, int>();
            _solidCubeVertices = new List<VertexCubeSolid>();
            _solidCubeIndices = new List<ushort>();
            _liquidCubeVertices = new List<VertexCubeLiquid>();
            _liquidCubeIndices = new List<ushort>();
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
