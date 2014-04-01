using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class UtopiaEntitySpawningControler : IEntitySpawningControler
    {
        #region Private variable
        private UtopiaWorldConfiguration _config;
        #endregion

        #region Public properties
        #endregion

        public UtopiaEntitySpawningControler(UtopiaWorldConfiguration config)
        {
            _config = config;
        }

        #region Public methods
        /// <summary>
        /// Function that will try to get a place where the entity can spawn, it will apply spawning restrictions.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="chunk">The chunk where the entity must be placed</param>
        /// <param name="cursor">A cursor on the chunk block data</param>
        /// <param name="rnd">Randomnes generator</param>
        /// <param name="entitySpawnLocation">Returned spawn location, will be [0;0;0] if entity cannot be placed</param>
        /// <returns>False if the entity cannot be placed</returns>
        public bool TryGetSpawnLocation(ChunkSpawnableEntity entity, AbstractChunk chunk, ByteChunkCursor cursor, FastRandom rnd, out Vector3D entitySpawnLocation)
        {
            entitySpawnLocation = default(Vector3D);

            //Test spawning chance
            if (rnd.NextDouble() <= entity.SpawningChance) return false;
            //Get Rnd chunk Location X/Z
            int x = rnd.Next(0, 16);
            int z = rnd.Next(0, 16);
            //Column Info index
            int columnInfoIndex = x * AbstractChunk.ChunkSize.Z + z;
            
            int y;
            switch (entity.SpawningPlace)
            {
                case ChunkSpawningPlace.Surface:
                    //Y base = Original world generated ground height (Before any player modification)
                    y = chunk.BlockData.ColumnsInfo[columnInfoIndex].MaxGroundHeight;
                    cursor.SetInternalPosition(x, y, z);
                    //Check that the block above is "Air"
                    if (cursor.Peek(CursorRelativeMovement.Up) != WorldConfiguration.CubeId.Air) return false;
                    //Check that the block below is "Solid"
                    BlockProfile blockSpawnProfile = _config.BlockProfiles[cursor.Read()];
                    if (!blockSpawnProfile.IsSolidToEntity) return false;
                    //Get Chunk master biome
                    byte masterBiomeId = chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType;
                    //Get biome surface block layer
                    byte surfaceBiomeCube = _config.ProcessorParam.Biomes[masterBiomeId].SurfaceCube;
                    //If the entity need a Wild chunk, then it can only spawn on a cube surface equal to the default biome surface cube !
                    if (entity.isWildChunkNeeded && surfaceBiomeCube != blockSpawnProfile.Id) return false;

                    // Hurray it can spawn ! :D
                    //Add some randomnes on the cube where it will spawn
                    double XOffset = rnd.NextDouble(0.2, 0.8);
                    double ZOffset = rnd.NextDouble(0.2, 0.8);

                    entitySpawnLocation = new Vector3D(chunk.Position.X + x + XOffset, y + 1, chunk.Position.Z + z + ZOffset);
                    break;
                case ChunkSpawningPlace.FloorInsideCave:
                    break;
                case ChunkSpawningPlace.CeilingInsideCave:
                    break;
                case ChunkSpawningPlace.AirAboveSurface:
                    break;
                default:
                    break;
            }

            return true;
        }
        #endregion

        #region Private methods
        #endregion
    }
}
