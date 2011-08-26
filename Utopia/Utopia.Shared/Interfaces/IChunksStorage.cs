using Utopia.Shared.Structs;

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
        byte[] LoadChunkData(IntVector2 pos);

        /// <summary>
        /// Saves multiple chunks in one transaction
        /// </summary>
        /// <param name="positions">Array of chunks positions</param>
        /// <param name="chunksData">corresponding array of chunks data</param>
        void SaveChunksData(IntVector2[] positions, byte[][] chunksData);
    }
}