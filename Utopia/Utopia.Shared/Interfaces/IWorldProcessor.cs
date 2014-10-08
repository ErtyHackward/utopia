using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using System;
using S33M3Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Represents object that can do world generation related operations
    /// </summary>
    public interface IWorldProcessor : IDisposable
    {
        /// <summary>
        /// Gets overall operation progress [0; 100]
        /// </summary>
        int PercentCompleted { get; }

        /// <summary>
        /// Gets current processor name
        /// </summary>
        string ProcessorName { get; }

        /// <summary>
        /// Gets current processor description
        /// </summary>
        string ProcessorDescription { get; }

        /// <summary>
        /// Starts generation process.
        /// </summary>
        void Generate(Range3I generationRange, GeneratedChunk[,,] chunks);
    }
}
