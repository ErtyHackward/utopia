using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class GrasslandBiome : Biome
    {
        #region Private Variables
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 30, ChanceOfSpawning = 0.7 };
        private BiomeEntity _flower1Entities = new BiomeEntity() { EntityId = EntityClassId.Plant, EntityPerChunk = 30, ChanceOfSpawning = 0.8 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return RealmConfiguration.CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return RealmConfiguration.CubeId.Dirt; } }
        public override byte GroundCube { get { return RealmConfiguration.CubeId.Stone; } }

        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        protected override BiomeEntity Flower1Entities { get { return _flower1Entities; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
