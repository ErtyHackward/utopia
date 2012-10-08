using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class PlainBiome : Biome
    {
        #region Private Variables
        private RangeI _treePerChunk = new RangeI(0, 1);
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 20, ChanceOfSpawning = 0.4 };
        private BiomeEntity _flower2Entities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 3, ChanceOfSpawning = 0.4 };
        private BiomeEntity _flower3Entities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 20, ChanceOfSpawning = 0.4 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return RealmConfiguration.CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return RealmConfiguration.CubeId.Dirt; } }
        public override byte GroundCube { get { return RealmConfiguration.CubeId.Stone; } }
        protected override RangeI TreePerChunk { get { return _treePerChunk; } }

        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        protected override BiomeEntity Flower2Entities { get { return _flower2Entities; } }
        protected override BiomeEntity Flower3Entities { get { return _flower3Entities; } }

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
