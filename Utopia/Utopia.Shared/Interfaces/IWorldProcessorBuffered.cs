using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Interfaces
{
    public interface IWorldProcessorBuffered
    {
        /// <summary>
        /// Flush a buffered chunk from Buffer
        /// </summary>
        /// <param name="chunkPosition"></param>
        void FlushBufferedChunks(Vector3I chunkPosition);

        /// <summary>
        /// Flush a list of buffered chunk from Buffer
        /// </summary>
        /// <param name="chunkPosition"></param>
        void FlushBufferedChunks(Vector3I[] chunkPosition);
    }
}
