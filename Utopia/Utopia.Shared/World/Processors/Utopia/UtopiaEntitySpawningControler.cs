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
                case ChunkSpawningPlace.FloorInsideCave:
                case ChunkSpawningPlace.CeilingInsideCave:
                    return CaveSpawnLogic(x, z, entity, chunk, cursor, rnd, out entitySpawnLocation);
                case ChunkSpawningPlace.AirAboveSurface:
                    return AirSpawnLogic(x, z, entity, chunk, cursor, rnd, out entitySpawnLocation);
                case ChunkSpawningPlace.Surface:
                default:
                    return SurfaceSpawnLogic(x, z, entity, chunk, cursor, rnd, out entitySpawnLocation);
            }
        }
        #endregion

        #region Private methods
        private bool SurfaceSpawnLogic(int x, int z, ChunkSpawnableEntity entity, AbstractChunk chunk, ByteChunkCursor cursor, FastRandom rnd, out Vector3D entitySpawnLocation)
        {
            entitySpawnLocation = default(Vector3D);

            int y;
            int columnInfoIndex = x * AbstractChunk.ChunkSize.Z + z;
            //Y base = Original world generated ground height (Before any player modification)
            y = chunk.BlockData.ColumnsInfo[columnInfoIndex].MaxGroundHeight;
            cursor.SetInternalPosition(x, y, z);

            // verify that we can spawn here
            var canSpawn = cursor.Read() != WorldConfiguration.CubeId.Air && cursor.Move(CursorRelativeMovement.Up) &&
                           cursor.Read() == WorldConfiguration.CubeId.Air && cursor.Move(CursorRelativeMovement.Up) &&
                           cursor.Read() == WorldConfiguration.CubeId.Air;

            // return cursor to the spawn point
            cursor.Move(CursorRelativeMovement.Down);

            if (!canSpawn)
                return false;
            
            // Check that the block is well "Solid to entity"
            BlockProfile blockSpawnProfile = _config.BlockProfiles[cursor.Peek(CursorRelativeMovement.Down)];
            if (!blockSpawnProfile.IsSolidToEntity) return false;
            if (entity.IsWildChunkNeeded)
            {
                //Get Chunk master biome
                byte masterBiomeId = chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType;
                //Get biome surface block layer
                byte surfaceBiomeCube = _config.ProcessorParam.Biomes[masterBiomeId].SurfaceCube;
                //If the entity need a Wild chunk, then it can only spawn on a cube surface equal to the default biome surface cube !
                if (surfaceBiomeCube != blockSpawnProfile.Id) return false;
            }

            // Hurray it can spawn ! :D
            //Add some randomnes on the cube where it will spawn
            double XOffset, ZOffset;
            XOffset = rnd.NextDouble(0.2, 0.8);
            ZOffset = rnd.NextDouble(0.2, 0.8);

            entitySpawnLocation = new Vector3D(chunk.BlockPosition.X + x + XOffset, cursor.InternalPosition.Y, chunk.BlockPosition.Z + z + ZOffset);

            return true;
        }

        private bool CaveSpawnLogic(int x, int z, ChunkSpawnableEntity entity, AbstractChunk chunk, ByteChunkCursor cursor, FastRandom rnd, out Vector3D entitySpawnLocation)
        {
            entitySpawnLocation = default(Vector3D);
            int y;
            int columnInfoIndex = x * AbstractChunk.ChunkSize.Z + z;
            
            cursor.SetInternalPosition(x, 1, z);
            //Move up until Air Block
            while (cursor.Read() != WorldConfiguration.CubeId.Air)
            {
                //Move up, if top chunk height exit
                if (cursor.Move(CursorRelativeMovement.Up) == false) return false;
            }

            int YFloorSpawning = cursor.InternalPosition.Y;

            int MaximumSpawningHeight = chunk.BlockData.ColumnsInfo[columnInfoIndex].MaxGroundHeight - 10;
            if (MaximumSpawningHeight <= 0) MaximumSpawningHeight = 1;
            //Move up until "solid" Block
            while (cursor.Read() == WorldConfiguration.CubeId.Air && cursor.InternalPosition.Y <= MaximumSpawningHeight)
            {
                //Move up, if top chunk height exit
                if (cursor.Move(CursorRelativeMovement.Up) == false) return false;
            }

            if (cursor.InternalPosition.Y > MaximumSpawningHeight) return false;

            // Hurray it can spawn ! :D
            //Add some randomnes on the cube where it will spawn
            double XOffset, ZOffset;
            XOffset = rnd.NextDouble(0.2, 0.8);
            ZOffset = rnd.NextDouble(0.2, 0.8);

            if (entity.SpawningPlace == ChunkSpawningPlace.FloorInsideCave)
            {
                entitySpawnLocation = new Vector3D(chunk.BlockPosition.X + x + XOffset, YFloorSpawning, chunk.BlockPosition.Z + z + ZOffset);
            }
            else
            {
                entitySpawnLocation = new Vector3D(chunk.BlockPosition.X + x + XOffset, cursor.InternalPosition.Y - 1, chunk.BlockPosition.Z + z + ZOffset);
            }

            return true;
        }
        
        private bool AirSpawnLogic(int x, int z, ChunkSpawnableEntity entity, AbstractChunk chunk, ByteChunkCursor cursor, FastRandom rnd, out Vector3D entitySpawnLocation)
        {
            entitySpawnLocation = default(Vector3D);
            int y;
            int columnInfoIndex = x * AbstractChunk.ChunkSize.Z + z;
            //Y base = Original world generated ground height (Before any player modification)
            y = rnd.Next(chunk.BlockData.ColumnsInfo[columnInfoIndex].MaxHeight + 5, AbstractChunk.ChunkSize.Y);

            if (y <= 0 || y >= AbstractChunk.ChunkSize.Y) return false;

            cursor.SetInternalPosition(x, y, z);

            if (cursor.Read() != WorldConfiguration.CubeId.Air) return false;

            // Hurray it can spawn ! :D
            //Add some randomnes on the cube where it will spawn
            double XOffset, ZOffset;
            XOffset = rnd.NextDouble(0.2, 0.8);
            ZOffset = rnd.NextDouble(0.2, 0.8);

            entitySpawnLocation = new Vector3D(chunk.BlockPosition.X + x + XOffset, cursor.InternalPosition.Y, chunk.BlockPosition.Z + z + ZOffset);
            return true;
        }
        #endregion
    }
}
