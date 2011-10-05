using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    public interface ILandscapeManager2D
    {
        /// <summary>
        /// Gets the chunk at position specified
        /// </summary>
        /// <param name="position">chunk position</param>
        /// <returns></returns>
        IChunkLayout2D GetChunk(Vector2I position);
    }
}