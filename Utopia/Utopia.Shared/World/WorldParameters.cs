﻿using System.ComponentModel;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Base class for world generation settings, inherit it to send additional data to world processors
    /// </summary>
    public class WorldParameters
    {
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

        public string SeedName { get; set; }

        public RealmConfiguration Configuration { get; set; }

        /// <summary>
        /// The World Name
        /// </summary>
        public string WorldName { get; set; }

        public void Clear()
        {
            WorldName = null;
            SeedName = null;
        }
    }
}
