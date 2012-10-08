using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class MontainBiome : Biome
    {
        #region Private Variables
        private RangeI _montainUnderSurfaceLayers = new RangeI(1, 2);
        #endregion

        #region Public Properties
        public override byte SurfaceCube
        {
            get { return RealmConfiguration.CubeId.Grass; }
        }

        public override byte UnderSurfaceCube
        {
            get { return RealmConfiguration.CubeId.Dirt; }
        }

        public override byte GroundCube
        {
            get { return RealmConfiguration.CubeId.Stone; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
