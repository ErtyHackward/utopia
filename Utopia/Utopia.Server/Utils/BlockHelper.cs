using Utopia.Shared.Structs;

namespace Utopia.Server.Utils
{
    public static class BlockHelper
    {
        public static Vector3I ChunkSize = new Vector3I(16, 128, 16);

        public static Vector2I BlockToChunkPosition(Vector3I blockPosition)
        {
            var vec2 = new Vector2I(blockPosition.X / ChunkSize.X, blockPosition.Z / ChunkSize.Z);
            if (blockPosition.X < 0 && blockPosition.X % ChunkSize.X != 0) vec2.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % ChunkSize.Z != 0) vec2.Y--;
            return vec2;
        }

        public static Vector3I GlobalToInternalChunkPosition(Vector3I globalPosition)
        {
            var vec3 = new Vector3I(globalPosition.X % ChunkSize.X, globalPosition.Y % ChunkSize.Y, globalPosition.Z % ChunkSize.Z);

            if (vec3.X < 0)
                vec3.X = ChunkSize.X + vec3.X;
            if (vec3.Y < 0)
                vec3.Y = ChunkSize.Y + vec3.Y;
            if (vec3.Z < 0)
                vec3.Z = ChunkSize.Z + vec3.Z;

            return vec3;
        }
    }
}
