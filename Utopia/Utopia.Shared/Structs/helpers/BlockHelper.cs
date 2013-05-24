using System;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Structs.Helpers
{
    public static class BlockHelper
    {
        public static Vector2I BlockToChunkPosition(Vector3I blockPosition)
        {
            var vec2 = new Vector2I(blockPosition.X / AbstractChunk.ChunkSize.X, blockPosition.Z / AbstractChunk.ChunkSize.Z);
            if (blockPosition.X < 0 && blockPosition.X % AbstractChunk.ChunkSize.X != 0) vec2.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % AbstractChunk.ChunkSize.Z != 0) vec2.Y--;
            return vec2;
        }

        public static Vector2I EntityToChunkPosition(Vector3D blockPosition)
        {
            var vec2 = new Vector2I((int)(blockPosition.X / AbstractChunk.ChunkSize.X), (int)(blockPosition.Z / AbstractChunk.ChunkSize.Z));
            if (blockPosition.X < 0 && blockPosition.X % AbstractChunk.ChunkSize.X != 0) vec2.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % AbstractChunk.ChunkSize.Z != 0) vec2.Y--;
            return vec2;
        }

        public static Vector3I GlobalToInternalChunkPosition(Vector3I globalPosition)
        {
            var vec3 = new Vector3I(globalPosition.X % AbstractChunk.ChunkSize.X, globalPosition.Y % AbstractChunk.ChunkSize.Y, globalPosition.Z % AbstractChunk.ChunkSize.Z);

            if (vec3.X < 0)
                vec3.X = AbstractChunk.ChunkSize.X + vec3.X;
            if (vec3.Y < 0)
                vec3.Y = AbstractChunk.ChunkSize.Y + vec3.Y;
            if (vec3.Z < 0)
                vec3.Z = AbstractChunk.ChunkSize.Z + vec3.Z;

            return vec3;
        }

        public static void GlobalToLocalAndChunkPos(Vector3I globalPos, out Vector3I internalPos, out Vector2I chunkPos)
        {
            internalPos = new Vector3I(globalPos.X % AbstractChunk.ChunkSize.X, globalPos.Y, globalPos.Z % AbstractChunk.ChunkSize.Z);

            if (internalPos.X < 0)
                internalPos.X = AbstractChunk.ChunkSize.X + internalPos.X;
            if (internalPos.Z < 0)
                internalPos.Z = AbstractChunk.ChunkSize.Z + internalPos.Z;

            chunkPos = new Vector2I((int)Math.Floor((double)globalPos.X / AbstractChunk.ChunkSize.X), (int)Math.Floor((double)globalPos.Z / AbstractChunk.ChunkSize.Z));
        }

        /// <summary>
        /// Converts set of local positions to global coordinates
        /// </summary>
        /// <param name="chunkPosition"></param>
        /// <param name="blocksPositions"></param>
        public static void ConvertToGlobal(Vector2I chunkPosition, Vector3I[] blocksPositions)
        {
            var dx = chunkPosition.X * AbstractChunk.ChunkSize.X;
            var dz = chunkPosition.Y * AbstractChunk.ChunkSize.Z;
            for (int i = 0; i < blocksPositions.Length; i++)
            {
                blocksPositions[i].X += dx;
                blocksPositions[i].Z += dz;
            }
        }

        public static Vector3I ConvertToGlobal(Vector2I chunkPosition, Vector3I internalPosition)
        {
            var dx = chunkPosition.X * AbstractChunk.ChunkSize.X;
            var dz = chunkPosition.Y * AbstractChunk.ChunkSize.Z;

            internalPosition.X += dx;
            internalPosition.Z += dz;

            return internalPosition;
        }
    }
}
