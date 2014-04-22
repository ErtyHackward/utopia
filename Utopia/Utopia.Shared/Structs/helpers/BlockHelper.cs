using System;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Structs.Helpers
{
    public static class BlockHelper
    {
        public static Vector3I BlockToChunkPosition(Vector3I blockPosition)
        {
            var vec3 = new Vector3I(blockPosition.X / AbstractChunk.ChunkSize.X, blockPosition.Y / AbstractChunk.ChunkSize.Y, blockPosition.Z / AbstractChunk.ChunkSize.Z);
            if (blockPosition.X < 0 && blockPosition.X % AbstractChunk.ChunkSize.X != 0) vec3.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % AbstractChunk.ChunkSize.Y != 0) vec3.Y--;
            if (blockPosition.Z < 0 && blockPosition.Z % AbstractChunk.ChunkSize.Z != 0) vec3.Z--;
            return vec3;
        }

        public static Vector3I EntityToChunkPosition(Vector3D blockPosition)
        {
            var vec3 = new Vector3I((int)(blockPosition.X / AbstractChunk.ChunkSize.X), (int)(blockPosition.Y / AbstractChunk.ChunkSize.Y),  (int)(blockPosition.Z / AbstractChunk.ChunkSize.Z));
            if (blockPosition.X < 0 && blockPosition.X % AbstractChunk.ChunkSize.X != 0) vec3.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % AbstractChunk.ChunkSize.Y != 0) vec3.Y--;
            if (blockPosition.Z < 0 && blockPosition.Z % AbstractChunk.ChunkSize.Z != 0) vec3.Z--;
            return vec3;
        }

        public static Vector3I GlobalToInternalChunkPosition(Vector3D globalPosition)
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

        public static void GlobalToLocalAndChunkPos(Vector3I globalPos, out Vector3I internalPos, out Vector3I chunkPos)
        {
            internalPos = new Vector3I(globalPos.X % AbstractChunk.ChunkSize.X, globalPos.Y % AbstractChunk.ChunkSize.Y, globalPos.Z % AbstractChunk.ChunkSize.Z);

            if (internalPos.X < 0)
                internalPos.X = AbstractChunk.ChunkSize.X + internalPos.X;
            if (internalPos.Y < 0)
                internalPos.Y = AbstractChunk.ChunkSize.Y + internalPos.Y;
            if (internalPos.Z < 0)
                internalPos.Z = AbstractChunk.ChunkSize.Z + internalPos.Z;

            chunkPos = new Vector3I((int)Math.Floor((double)globalPos.X / AbstractChunk.ChunkSize.X), 
                                    (int)Math.Floor((double)globalPos.Y / AbstractChunk.ChunkSize.Y), 
                                    (int)Math.Floor((double)globalPos.Z / AbstractChunk.ChunkSize.Z));
        }

        /// <summary>
        /// Converts set of local positions to global coordinates
        /// </summary>
        /// <param name="chunkPosition"></param>
        /// <param name="blocksPositions"></param>
        public static void ConvertToGlobal(Vector3I chunkPosition, Vector3I[] blocksPositions)
        {
            var dx = chunkPosition.X * AbstractChunk.ChunkSize.X;
            var dy = chunkPosition.Y * AbstractChunk.ChunkSize.Y;
            var dz = chunkPosition.Z * AbstractChunk.ChunkSize.Z;
            for (int i = 0; i < blocksPositions.Length; i++)
            {
                blocksPositions[i].X += dx;
                blocksPositions[i].Y += dy;
                blocksPositions[i].Z += dz;
            }
        }

        public static Vector3I ConvertToGlobal(Vector3I chunkPosition, Vector3I internalPosition)
        {
            var dx = chunkPosition.X * AbstractChunk.ChunkSize.X;
            var dy = chunkPosition.Y * AbstractChunk.ChunkSize.Y;
            var dz = chunkPosition.Z * AbstractChunk.ChunkSize.Z;

            internalPosition.X += dx;
            internalPosition.Y += dy;
            internalPosition.Z += dz;

            return internalPosition;
        }

        public static Vector3I EntityToBlock(Vector3D position)
        {
            return (Vector3I)position;
        }
    }
}
