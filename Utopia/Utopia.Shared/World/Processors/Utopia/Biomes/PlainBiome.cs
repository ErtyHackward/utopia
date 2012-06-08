using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class PlainBiome : Biome
    {
        #region Private Variables
        private RangeI _treePerChunk = new RangeI(0, 1);
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Grass, EntityPerChunk = 10, ChanceOfSpawning = 0.6 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return CubeId.Dirt; } }
        public override RangeI UnderSurfaceLayers { get { return _underSurfaceLayers; } }
        public override byte GroundCube { get { return CubeId.Stone; } }
        protected override RangeI TreePerChunk { get { return _treePerChunk; } }
        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        #endregion

        public PlainBiome()
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
            for (int i = 0; i < 50; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Small;
            for (int i = 50; i < 100; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Medium;
        }
        #endregion

    }
}
