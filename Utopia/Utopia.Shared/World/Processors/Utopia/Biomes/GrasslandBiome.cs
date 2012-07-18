using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;
using Utopia.Shared.Entities;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class GrasslandBiome : Biome
    {
        #region Private Variables
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Grass, EntityPerChunk = 50, ChanceOfSpawning = 0.7 };
        private BiomeEntity _flower1Entities = new BiomeEntity() { EntityId = EntityClassId.Flower1, EntityPerChunk = 40, ChanceOfSpawning = 0.8 };
        private BiomeEntity _flower2Entities = new BiomeEntity() { EntityId = EntityClassId.Flower2, EntityPerChunk = 5, ChanceOfSpawning = 0.4 };
        private BiomeEntity _flower3Entities = new BiomeEntity() { EntityId = EntityClassId.Flower3, EntityPerChunk = 5, ChanceOfSpawning = 0.4 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube { get { return CubeId.Grass; } }
        public override byte UnderSurfaceCube { get { return CubeId.Dirt; } }
        public override RangeI UnderSurfaceLayers { get { return _underSurfaceLayers; } }
        public override byte GroundCube { get { return CubeId.Stone; } }

        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        protected override BiomeEntity Flower1Entities { get { return _flower1Entities; } }
        protected override BiomeEntity Flower2Entities { get { return _flower2Entities; } }
        protected override BiomeEntity Flower3Entities { get { return _flower3Entities; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
