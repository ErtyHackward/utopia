﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using S33M3_Resources.Structs;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class DesertBiome : Biome
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public override byte SurfaceCube
        {
            get { return CubeId.Sand; }
        }

        public override byte UnderSurfaceCube
        {
            get { return CubeId.Sand; }
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