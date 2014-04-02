﻿using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

namespace Utopia.Shared.Server.Managers
{
    public class EntitySpawningManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _server;
        private readonly ServerLandscapeManager _landscapeManager;
        private readonly IEntitySpawningControler _entitySpawningControler;
        private readonly UtopiaWorldConfiguration _worldParam;
        private FastRandom _fastRandom;

        public EntitySpawningManager(ServerCore server, IEntitySpawningControler entitySpawningControler)
        {
            _server = server;
            _entitySpawningControler = entitySpawningControler;
            _worldParam = _server.WorldParameters.Configuration as UtopiaWorldConfiguration;
            _landscapeManager = server.LandscapeManager;
            _fastRandom = new FastRandom();

            //This spawn logic can only be down on UtopiaWorldConfiguration and associated processor.
            if (_worldParam != null)
            {
                _server.Clock.ClockTimers.Add(new Clock.GameClockTimer(0, 0, 1, 0, server.Clock, UtopiaSpawningLookup));
            }
        }

        /// <summary>
        /// This method should be called every "in game hour".
        /// </summary>
        private void UtopiaSpawningLookup(DateTime gametime)
        {
            logger.Debug("SpawningLookup raised {0}", gametime);

            foreach (var chunk in _landscapeManager.GetBufferedChunks())
            {
                //TODO Check if chunk can start a spawning refresh. Need to add a LastSpawningRefresh value + the trick to avoid all chunk to be refreshed at the same time.

                var chunkBiome = _worldParam.ProcessorParam.Biomes[chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType];

                foreach (var spawnableEntities in chunkBiome.SpawnableEntities)
                {
                    //Remark : isChunkGenerationSpawning is set to true for static entities, and false for dynamic entities, maybe worth renaming the properties
                    //The aim of it is to avoid dynamic entity creation at chunk generation time (Pure chunk).
                    //Apply creation constaint :
                    //1) if static entity with is isWildChunkNeeded and chunk is not wild => Do nothing
                    if(spawnableEntities.isWildChunkNeeded && chunk.BlockData.ChunkMetaData.IsWild == false) continue;

                    //TODO Check against the Dictionnary<BluePrintID, CurrentAmountOfEntity> stored on the serverchunk of the maximum entity amount for this chunk is not reached.
                    //This information must be "buffered" in a dictionnary (and so initialized at chunk loading), or computed "live"
                    //For dynamic entity, will be worth to have the chunk ID stored inside the dynamic entity as "BirthChunk" in order to be able to count them.

                    // TODO if chunk is pure and entity is static => Do nothing (To avoid to have all chunk generated by server go is not pure state and thus avoiding pure client chunk generation anymore).

                    // TODO Check that MaxEntityAmount of this entity is not already present in the chunk (Both static & dynamic)

                    // TODO check entity SpawningDayTime is corresponding to the current time in game (Can only spawn during specific part of day)

                    // TODO check entity SpawningSeasons is corresponding to the current season in game (Can only spawn during specific season)

                    ByteChunkCursor chunkCursor = new ByteChunkCursor(chunk.BlockData.GetBlocksBytes(), chunk.BlockData.ColumnsInfo);
                    // ==> Maybe worth to automaticaly create this specialize cursor at server chunk creation ? Multithreading problem ?
                    //It is only use for reading chunk block data in fast way
                    Vector3D entityLocation;
                    if (_entitySpawningControler.TryGetSpawnLocation(spawnableEntities, chunk, chunkCursor, _fastRandom, out entityLocation))
                    {
                        //The entity location has been validated !
                        //Create the entity at the entityLocation place !
                    }
                }
            }
        }

    }
}
