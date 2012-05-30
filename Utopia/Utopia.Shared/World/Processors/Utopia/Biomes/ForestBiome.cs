using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using S33M3_Resources.Structs;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class ForestBiome : Biome
    {
        #region Private Variables
        private RangeI _treePerChunk = new RangeI(8, 15);
        private RangeI _treeTypeRange = new RangeI(0, 2);
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return CubeId.Dirt; } }
        public override RangeI UnderSurfaceLayers { get { return _underSurfaceLayers; } }
        public override byte GroundCube { get { return CubeId.Stone; } }

        protected override RangeI TreePerChunk { get { return _treePerChunk; } }
        protected override RangeI TreeTypeRange { get { return _treeTypeRange; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
