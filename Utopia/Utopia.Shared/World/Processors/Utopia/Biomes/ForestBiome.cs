using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class ForestBiome : Biome
    {
        #region Private Variables
        private RangeI _treePerChunk = new RangeI(8, 15);
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 15, ChanceOfSpawning = 0.6 };
        private BiomeEntity _flower1Entities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 15, ChanceOfSpawning = 0.6 };
        private BiomeEntity _mushroomEntities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 15, ChanceOfSpawning = 0.6 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return RealmConfiguration.CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return RealmConfiguration.CubeId.Dirt; } }
        public override byte GroundCube { get { return RealmConfiguration.CubeId.Stone; } }

        protected override RangeI TreePerChunk { get { return _treePerChunk; } }
        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        protected override BiomeEntity Flower1Entities { get { return _flower1Entities; } }
        protected override BiomeEntity MushroomEntities { get { return _mushroomEntities; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
