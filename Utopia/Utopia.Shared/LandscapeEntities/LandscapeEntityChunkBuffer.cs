using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeEntityChunkBuffer
    {
        public enum LandscapeEntityChunkBufferState
        {
            NotProcessed,
            Processing,
            Processed
        }

        public Vector2I ChunkLocation;
        public List<BlockWithPosition> Blocks;
        public LandscapeEntityChunkBufferState ProcessingState = LandscapeEntityChunkBufferState.NotProcessed;
        public bool isReady = false;
    }
}
