using ProtoBuf;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Base class for world generation settings, inherit it to send additional data to world processors
    /// </summary>
    [ProtoContract]
    public class WorldParameters
    {
        /// <summary>
        /// The World Name
        /// </summary>
        [ProtoMember(1)]
        public string WorldName { get; set; }

        [ProtoMember(2)]
        public string SeedName { get; set; }

        [ProtoMember(3)]
        public WorldConfiguration Configuration { get; set; }

        /// <summary>
        /// Base seed to use in random initializers
        /// </summary>
        public int Seed
        {
            get
            {
                return SeedName.GetHashCode();
            }
        }

        public void Clear()
        {
            WorldName = null;
            SeedName = null;
        }
    }
}
