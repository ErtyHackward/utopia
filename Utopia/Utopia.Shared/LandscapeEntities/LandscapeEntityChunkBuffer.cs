using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeChunkBuffer
    {
        public enum LandscapeChunkBufferState
        {
            NotProcessed,
            Processing,
            Processed
        }

        public Vector2I ChunkLocation;
        public List<LandscapeEntity> Entities;
        public LandscapeChunkBufferState ProcessingState = LandscapeChunkBufferState.NotProcessed;

        //Buffered landscape data, in order to avoid to recreate it if not necessary !
        public ChunkColumnInfo[] ColumnsInfoBuffer;
        public byte[] chunkBytesBuffer;

        public bool isReady = false;
    }
}
