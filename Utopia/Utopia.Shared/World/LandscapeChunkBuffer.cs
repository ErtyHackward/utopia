﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.World
{
    [ProtoContract]
    public class LandscapeChunkBuffer
    {
        public enum LandscapeChunkBufferState
        {
            NotProcessed,
            Processing,
            Processed
        }

        [ProtoMember(1)]
        public Vector2I ChunkLocation { get; set; }
        [ProtoMember(2, OverwriteList = true)]
        public List<LandscapeEntity> Entities { get; set; }
        [ProtoMember(3)]
        public LandscapeChunkBufferState ProcessingState { get; set; }
        [ProtoMember(4)]
        public ChunkColumnInfo[] ColumnsInfoBuffer { get; set; }
        [ProtoMember(5)]
        public byte[] chunkBytesBuffer { get; set; }
        [ProtoMember(6)]
        public bool isReady = false;

        public bool isLocked { get; set; }

        public LandscapeChunkBuffer()
        {
            ProcessingState = LandscapeChunkBufferState.NotProcessed;
        }
    }
}
