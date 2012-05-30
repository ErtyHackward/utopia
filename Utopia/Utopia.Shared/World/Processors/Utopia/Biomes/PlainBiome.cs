using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class PlainBiome : Biome
    {
        #region Private Variables
        private RangeI _treePerChunk = new RangeI(0, 1);
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
