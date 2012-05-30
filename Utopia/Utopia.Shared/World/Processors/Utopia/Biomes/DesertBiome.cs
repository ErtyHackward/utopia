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
        private RangeI _cactusPerChunk = new RangeI(0, 4);
        private RangeI _treeTypeRange = new RangeI((int)TreeTemplates.TreeType.Cactus, (int)TreeTemplates.TreeType.Cactus);
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return CubeId.Sand; } }
        public override byte UnderSurfaceCube { get { return CubeId.Sand; } }
        public override RangeI UnderSurfaceLayers { get { return _underSurfaceLayers; } }
        public override byte GroundCube { get { return CubeId.Stone; } }

        protected override RangeI TreePerChunk { get { return _cactusPerChunk; } }
        protected override RangeI TreeTypeRange { get { return _treeTypeRange; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


    }
}
