using Utopia.Shared.Structs;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Base class for world generation settings, inherit it to send additional data to world processors
    /// </summary>
    public class WorldParameters
    {
        /// <summary>
        /// World size in chunk unit (width and length)
        /// </summary>
        public Location2<int> WorldSize { get; set; }

        /// <summary>
        /// Indicates if world is infinite of final
        /// </summary>
        public bool IsInfinite { get; set; }

        /// <summary>
        /// Base seed to use in random initializers
        /// </summary>
        public int Seed { get; set; }
    }
}
