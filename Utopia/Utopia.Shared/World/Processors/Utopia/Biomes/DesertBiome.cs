using System;
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
        private Cavern _moonStoneCavern = new Cavern() { CubeId = CubeId.MoonStone, CavernHeightSize = new RangeB(5, 10), CavernPerChunk = 3, SpawningHeight = new RangeB(20, 60), ChanceOfSpawning = 0.1 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return CubeId.Sand; } }
        public override byte UnderSurfaceCube { get { return CubeId.Sand; } }
        public override RangeI UnderSurfaceLayers { get { return _underSurfaceLayers; } }
        public override byte GroundCube { get { return CubeId.Stone; } }
        
        protected override RangeI TreePerChunk { get { return _cactusPerChunk; } }
        protected override Cavern MoonStoneCavern { get { return _moonStoneCavern; } }
        #endregion

        public DesertBiome()
        : base()
        {
            CreateTreeDistribution();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateTreeDistribution()
        {
            //Default tree distribution
            for (int i = 0; i < 100; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Cactus;
        }
        #endregion


    }
}
