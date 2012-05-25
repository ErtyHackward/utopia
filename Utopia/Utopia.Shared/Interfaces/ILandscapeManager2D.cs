using S33M3Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Represents landscape manager that have 2d chunk layout
    /// </summary>
    public interface ILandscapeManager2D
    {
        /// <summary>
        /// Gets the chunk at position specified
        /// </summary>
        /// <param name="chunkPosition">chunk position in World coordinate</param>
        /// <returns></returns>
        IChunkLayout2D GetChunk(Vector2I chunkPosition);

        /// <summary>
        /// Gets the chunk at block position specified
        /// </summary>
        /// <param name="blockPosition">block position in World coordinate</param>
        /// <returns></returns>
        IChunkLayout2D GetChunk(Vector3I blockPosition);

        /// <summary>
        /// Returns block cursor
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        ILandscapeCursor GetCursor(Vector3I blockPosition);

        /// <summary>
        /// Returns block cursor
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        ILandscapeCursor GetCursor(Vector3D entityPosition);
    }
}