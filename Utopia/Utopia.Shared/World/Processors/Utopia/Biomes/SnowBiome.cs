using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class SnowBiome : Biome
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public override byte SurfaceCube
        {
            get { return CubeId.Gravel; }
        }

        public override byte UnderSurfaceCube
        {
            get { return CubeId.Dirt; }
        }

        public override RangeI UnderSurfaceLayers
        {
            get { return _underSurfaceLayers; }
        }

        public override byte GroundCube
        {
            get { return CubeId.Stone; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
