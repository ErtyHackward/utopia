using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeEntityManager
    {
        #region Private Variables
        private object _syncLock = new object();
        private Dictionary<Vector2I, LandscapeEntityChunkBuffer> _buffer;
        private Vector2I _chunkRangeLookUp = new Vector2I(3, 3);
        #endregion

        #region Public Properties
        public UtopiaProcessor Processor { get; set; }
        #endregion

        public LandscapeEntityManager()
        {
            _buffer = new Dictionary<Vector2I, LandscapeEntityChunkBuffer>();
        }

        #region Public Methods
        public LandscapeEntityChunkBuffer Get(Vector2I chunkLocation)
        {
            LandscapeEntityChunkBuffer buffer;
            lock (_syncLock)
            {
                if (_buffer.TryGetValue(chunkLocation, out buffer) == false)
                {
                    buffer = new LandscapeEntityChunkBuffer() { ChunkLocation = chunkLocation };
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

        #endregion

        #region Private Methods
        private void GenerateLandscapeEntities(LandscapeEntityChunkBuffer buffer)
        {
            //Get the Range the minimum chunk range that need to be computed to validate current chunk landscape entities generation
            Range2I chunkRange = new Range2I(buffer.ChunkLocation - _chunkRangeLookUp, new Vector2I(_chunkRangeLookUp.X * 2, _chunkRangeLookUp.Y * 2));
            List<LandscapeEntityChunkBuffer> chunkBuffers = new List<LandscapeEntityChunkBuffer>(chunkRange.Count);
            foreach (Vector2I chunkPosition in chunkRange)
            {
                //Check if all those chunks have been / have their landscape item processed (They could potentially impact this chunk).
                LandscapeEntityChunkBuffer surrendingChunkBuffer;
                bool need2Process = false;
                lock (_syncLock)
                {
                    if (_buffer.TryGetValue(chunkPosition, out surrendingChunkBuffer) == false)
                    {
                        surrendingChunkBuffer = new LandscapeEntityChunkBuffer() { ChunkLocation = chunkPosition, ProcessingState = LandscapeEntityChunkBuffer.LandscapeEntityChunkBufferState.NotProcessed };
                        _buffer.Add(chunkPosition, surrendingChunkBuffer);
                    }

                    if (surrendingChunkBuffer.ProcessingState == LandscapeEntityChunkBuffer.LandscapeEntityChunkBufferState.NotProcessed)
                    {
                        surrendingChunkBuffer.ProcessingState = LandscapeEntityChunkBuffer.LandscapeEntityChunkBufferState.Processing;
                        need2Process = true;
                    }
                }

                chunkBuffers.Add(surrendingChunkBuffer);

                if (need2Process)
                {
                    //Process Landscape
                    //Process Entity landscape creation

                    //==> Send results entity landscape generated inside chunks (Using Parser)
                    surrendingChunkBuffer.ProcessingState = LandscapeEntityChunkBuffer.LandscapeEntityChunkBufferState.Processed;
                }
            }

            //Here, all chunk have been at least processed, some could still be in Processing state, waiting for all of them to finish.
            while (chunkBuffers.Count(x => x.ProcessingState != LandscapeEntityChunkBuffer.LandscapeEntityChunkBufferState.Processed) != 0)
            {
                //Wait for all lanscape entity from our range to be processed
                Thread.Sleep(1);
            }
        }
        #endregion

    }
}
