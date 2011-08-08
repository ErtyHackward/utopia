using Utopia.Shared.Structs;

namespace Utopia.Server.Utils
{
    public static class BlockHelper
    {
        public static Location3<int> ChunkSize = new Location3<int>(16, 128, 16);

        public static IntVector2 BlockToChunkPosition(Location3<int> blockPosition)
        {
            var vec2 = new IntVector2(blockPosition.X / ChunkSize.X, blockPosition.Z / ChunkSize.Z);
            if (blockPosition.X < 0 && blockPosition.X % ChunkSize.X != 0) vec2.X--;
            if (blockPosition.Y < 0 && blockPosition.Y % ChunkSize.Z != 0) vec2.Y--;
            return vec2;
        }

        public static Location3<int> GlobalToInternalChunkPosition(Location3<int> globalPosition)
        {
            var vec3 = new Location3<int>(globalPosition.X % ChunkSize.X, globalPosition.Y % ChunkSize.Y, globalPosition.Z % ChunkSize.Z);

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
