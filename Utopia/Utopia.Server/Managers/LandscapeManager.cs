using System;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides functions to work with landscape
    /// </summary>
    public class LandscapeManager
    {
        private readonly Server _parentServer;

        public LandscapeManager(Server parentServer)
        {
            if (parentServer == null) throw new ArgumentNullException("parentServer");
            _parentServer = parentServer;
        }

        /// <summary>
        /// Provides chunk and internal chunk position of global Vector3
        /// </summary>
        /// <param name="position">Global position</param>
        /// <param name="chunk">a chunk containing this position</param>
        /// <param name="cubePosition">a cube position inside the chunk</param>
        public void GetBlockAndChunk(DVector3 position, out ServerChunk chunk, out Location3<int> cubePosition)
        {
            cubePosition.X = (int)Math.Floor(position.X);
            cubePosition.Y = (int)Math.Floor(position.Y);
            cubePosition.Z = (int)Math.Floor(position.Z);

            var chunkPosition = cubePosition;

            chunkPosition.X /= AbstractChunk.ChunkSize.X;
            chunkPosition.Z /= AbstractChunk.ChunkSize.Z;

            chunk = _parentServer.GetChunk(new IntVector2(cubePosition.X / AbstractChunk.ChunkSize.X, cubePosition.Z / AbstractChunk.ChunkSize.Z));

            cubePosition.X = cubePosition.X % AbstractChunk.ChunkSize.X;
            cubePosition.Z = cubePosition.Z % AbstractChunk.ChunkSize.Z;
        }

    }
}
