using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.Chunks;
using S33M3CoreComponents.Maths;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeEntityManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private object _syncLock = new object();
        private Dictionary<Vector2I, LandscapeChunkBuffer> _buffer;
        private Vector2I _chunkRangeLookUp = new Vector2I(3, 3);
        #endregion

        #region Public Properties
        public UtopiaProcessor Processor { get; set; }
        #endregion

        public LandscapeEntityManager()
        {
            _buffer = new Dictionary<Vector2I, LandscapeChunkBuffer>();
        }

        #region Public Methods
        public LandscapeChunkBuffer Get(Vector2I chunkLocation)
        {
            LandscapeChunkBuffer buffer;
            lock (_syncLock)
            {
                if (_buffer.TryGetValue(chunkLocation, out buffer) == false)
                {
                    buffer = new LandscapeChunkBuffer() { ChunkLocation = chunkLocation };
                    _buffer.Add(chunkLocation, buffer);
                }
            }

            if (buffer.isReady)
            {
                return buffer;
            }
            else
            {
                //Start needed landscape entity generation
                GenerateLandscapeEntities(buffer);
                return buffer;
            }

        }

        public void Insert(Vector2I chunkLocation, LandscapeEntity entity)
        {
            LandscapeChunkBuffer buffer;
            lock (_syncLock)
            {
                if (_buffer.TryGetValue(chunkLocation, out buffer) == false)
                {
                    buffer = new LandscapeChunkBuffer() { ChunkLocation = chunkLocation };
                    _buffer.Add(chunkLocation, buffer);
                }
            }

            if (buffer.Entities == null) buffer.Entities = new List<LandscapeEntity>();
            buffer.Entities.Add(entity);
        }
        #endregion

        #region Private Methods
        private void GenerateLandscapeEntities(LandscapeChunkBuffer buffer)
        {
            //Get the Range the minimum chunk range that need to be computed to validate current chunk landscape entities generation
            Range2I chunkRange = new Range2I(buffer.ChunkLocation - _chunkRangeLookUp, new Vector2I(_chunkRangeLookUp.X * 2, _chunkRangeLookUp.Y * 2));
            List<LandscapeChunkBuffer> chunkBuffers = new List<LandscapeChunkBuffer>(chunkRange.Count);
            foreach (Vector2I chunkPosition in chunkRange)
            {
                //Check if all those chunks have been / have their landscape item processed (They could potentially impact this chunk).
                LandscapeChunkBuffer surrendingChunkBuffer;
                bool need2Process = false;
                lock (_syncLock)
                {
                    if (_buffer.TryGetValue(chunkPosition, out surrendingChunkBuffer) == false)
                    {
                        surrendingChunkBuffer = new LandscapeChunkBuffer() { ChunkLocation = chunkPosition, ProcessingState = LandscapeChunkBuffer.LandscapeChunkBufferState.NotProcessed };
                        _buffer.Add(chunkPosition, surrendingChunkBuffer);
                    }

                    if (surrendingChunkBuffer.ProcessingState == LandscapeChunkBuffer.LandscapeChunkBufferState.NotProcessed)
                    {
                        surrendingChunkBuffer.ProcessingState = LandscapeChunkBuffer.LandscapeChunkBufferState.Processing;
                        need2Process = true;
                    }
                }

                chunkBuffers.Add(surrendingChunkBuffer);

                if (need2Process)
                {
                    //Process Landscape
                    FastRandom chunkNewRnd;
                    Biome chunkMasterBiome;
                    ChunkColumnInfo[] columnsInfo;
                    byte[] chunkBytes;
                    Processor.GenerateForLandscapeEntity(surrendingChunkBuffer.ChunkLocation, out chunkMasterBiome, out chunkBytes, out chunkNewRnd, out columnsInfo);

                    //Process chunk Entity landscape creation
                    Processor.LandscapeEntities.GenerateChunkItems(surrendingChunkBuffer.ChunkLocation, chunkMasterBiome, columnsInfo, chunkNewRnd);

                    surrendingChunkBuffer.chunkBytesBuffer = chunkBytes;
                    surrendingChunkBuffer.ColumnsInfoBuffer = columnsInfo;

                    surrendingChunkBuffer.ProcessingState = LandscapeChunkBuffer.LandscapeChunkBufferState.Processed;
                }
            }

            //Here, all chunk have been at least processed, some could still be in Processing state, waiting for all of them to finish.
            while (chunkBuffers.Count(x => x.ProcessingState != LandscapeChunkBuffer.LandscapeChunkBufferState.Processed) > 0)
            {
                //Wait for all lanscape entity from our range to be processed
                Thread.Sleep(1);
            }
        }
        #endregion

    }
}
