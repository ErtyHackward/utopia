using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Base class for world generation settings, inherit it to send additional data to world processors
    /// </summary>
    public class WorldParameters
    {
        ///// <summary>
        ///// World size in chunk unit (width and length)
        ///// </summary>
        //public Vector2I WorldChunkSize { get; set; }

        /// <summary>
        /// Base seed to use in random initializers
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Sea height Level (blocks)
        /// </summary>
        public int SeaLevel { get; set; }

        public WorldParameters()
        {
        }
    }
}
