using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Planets.Terran.Chunk;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser
    {
        #region Private variables
        private World _world;
        #endregion

        #region Public properties/Variable
        public IntVector2 ChunkPosition { get; set; } // Gets or sets current chunk position
        public ChunkState State { get; set; }         // Chunk State

        #endregion

        public VisualChunk(World world)
            : base(new SingleArrayDataProvider())
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor
            
            State = ChunkState.Empty;
            _world = world;
        }

        #region Public methods
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
    }
}
