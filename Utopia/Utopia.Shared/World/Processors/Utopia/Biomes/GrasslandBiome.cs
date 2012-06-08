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
        private BiomeEntity _grassEntities = new BiomeEntity() { EntityId = EntityClassId.Grass, EntityPerChunk = 20, ChanceOfSpawning = 0.9 };
        #endregion

        #region Public Properties
        public override byte SurfaceCube
        {
            get { return CubeId.Grass; }
        }

        public override byte UnderSurfaceCube
        {
            get { return CubeId.Dirt; }
        }

        public override RangeI UnderSurfaceLayers
        {
            get { return _underSurfaceLayers; }
        }

        public override byte GroundCube
        {
            get { return CubeId.Stone; }
        }

        protected override BiomeEntity GrassEntities { get { return _grassEntities; } }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
