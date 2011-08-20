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
        public IntVector2 ChunkPosition { get; private set; } // Gets or sets current chunk position
        public ChunkState State { get; set; }                 // Chunk State
        public ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public WorkItemPriority ThreadPriority { get; set; }  // Thread Priority value
        public int UserChangeOrder { get; set; }              // Variable for sync drawing at rebuild time.

        public Range<int> CubeRange
        {
            get { return _cubeRange; }
            set
            {
                _cubeRange = value;
                ChunkPosition = new IntVector2() { X = _cubeRange.Min.X, Y = _cubeRange.Min.Z };
            }
        }
        #endregion

        public VisualChunk(WorldChunks world, ref Range<int> cubeRange)
            : base(new SingleArrayDataProvider())
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
            if (X == _world.WorldBorder.Min.X ||
               Z == _world.WorldBorder.Min.Z ||
               X == _world.WorldBorder.Max.X - AbstractChunk.ChunkSize.X ||
               Z == _world.WorldBorder.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
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
