using S33M3Engines.Shared.Math;
using Utopia.Shared.Structs;

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
        /// <param name="position">chunk position in World coordinate</param>
        /// <returns></returns>
        IChunkLayout2D GetChunk(Vector2I chunkPosition);

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