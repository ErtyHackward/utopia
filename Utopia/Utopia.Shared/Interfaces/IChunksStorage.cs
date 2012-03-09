using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Describes chunks storage. Allows to save and load the chunks
    /// </summary>
    public interface IChunksStorage
    {
        /// <summary>
        /// Fetch chunk data from the storage
        /// </summary>
        /// <param name="pos">Position of the block</param>
        /// <returns></returns>
        byte[] LoadChunkData(Vector2I pos);

        /// <summary>
        /// Saves multiple chunks in one transaction
        /// </summary>
        /// <param name="positions">Array of chunks positions</param>
        /// <param name="chunksData">corresponding array of chunks data</param>
        void SaveChunksData(Vector2I[] positions, byte[][] chunksData);
    }
}