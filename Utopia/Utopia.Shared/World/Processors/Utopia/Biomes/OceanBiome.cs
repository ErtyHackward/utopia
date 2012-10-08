using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class OceanBiome : Biome
    {
        #region Private Variables
        private Cavern _moonStoneCavern = new Cavern() { CubeId = RealmConfiguration.CubeId.MoonStone, CavernHeightSize = new RangeB(3, 5), CavernPerChunk = 0, SpawningHeight = new RangeB(20, 60), ChanceOfSpawning = 0.00 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube
        {
            get { return RealmConfiguration.CubeId.Sand; }
        }

        public override byte UnderSurfaceCube
        {
            get { return RealmConfiguration.CubeId.Sand; }
        }

        public override byte GroundCube
        {
            get { return RealmConfiguration.CubeId.Stone; }
        }

        protected override Cavern MoonStoneCavern
        {
            get
            {
                return _moonStoneCavern;
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
