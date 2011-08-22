using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Worlds.Chunks.ChunkWrapper
{
    public enum ChunkWrapType
    {
        X_Plus1,
        X_Minus1,
        Z_Plus1,
        Z_Minus1
    }

    public enum ChunkWrapperStatus
    {
        Idle,
        ProcessingNewWrap,
        WrapPostWrapWork
    }

    public class WorldChunksWrapper
    {
    }
}
