using System.ComponentModel;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Base class for world generation settings, inherit it to send additional data to world processors
    /// </summary>
    public class WorldParameters : IBinaryStorable
    {
        /// <summary>
        /// The World Name
        /// </summary>
        public string WorldName { get; set; }
        public string SeedName { get; set; }
        public RealmConfiguration Configuration { get; set; }

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

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(WorldName);
            writer.Write(SeedName);
            Configuration.Save(writer);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            WorldName = reader.ReadString();
            SeedName = reader.ReadString();
            Configuration = new RealmConfiguration();
            Configuration.Load(reader);
        }
    }
}
