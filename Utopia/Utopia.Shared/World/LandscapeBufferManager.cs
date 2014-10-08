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
using Utopia.Shared.LandscapeEntities;
using ProtoBuf;
using System.IO;
using System.IO.Compression;
using ProtoBuf.Meta;
using S33M3CoreComponents.Config;
using S33M3DXEngine.Main;

namespace Utopia.Shared.World
{
    [ProtoContract]
    public class LandscapeBufferManager : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static bool WithoutLandscapeBuffer;

        #region Private Variables
        private object _syncLock = new object();
        private Dictionary<Vector3I, LandscapeChunkBuffer> _buffers;
        private Vector3I _chunkRangeLookUp = new Vector3I(3, 0, 3);
        private string _bufferPath;

        private Vector3I _lastSinglePlayerChunkPosition;
        #endregion

        #region Public Properties
        [ProtoMember(1, OverwriteList = true)]
        public Dictionary<Vector3I, LandscapeChunkBuffer> Buffer
        {
            get { return _buffers; }
            set { _buffers = value; }
        }

        public UtopiaProcessor Processor { get; set; }
        #endregion

        public LandscapeBufferManager()
        {
            _buffers = new Dictionary<Vector3I, LandscapeChunkBuffer>();
        }

        public void Dispose()
        {
            SaveBuffer();
        }
        
        #region Public Methods
        public void CleanUpClient(Vector3I chunkPosition,  VisualWorldParameters vwp)
        {
            //Clean Up LandscapeChunkBuffer that are too far from current player position for single player mode
            if (chunkPosition == _lastSinglePlayerChunkPosition) return;
            _lastSinglePlayerChunkPosition = chunkPosition;
            S33M3DXEngine.Threading.ThreadsManager.RunAsync(() => CleanUpTreadedWork(vwp, _lastSinglePlayerChunkPosition));
        }

        public void FlushBufferedChunk(Vector3I chunkPosition)
        {
            //Clean Up LandscapeChunkBuffer that are too far from current player position for single player mode
            lock (_syncLock)
            {
                //Remove the buffered data
                _buffers.Remove(chunkPosition);
            }
        }

        private void CleanUpTreadedWork(VisualWorldParameters vwp, Vector3I lastSinglePlayerChunkPosition)
        {
            //Single Player Mode = Remove buffer data based on current player chunk position
            if (Monitor.TryEnter(_syncLock, 0))
            {
                //Get the list of Buffer
                var buffer2Remove = new List<LandscapeChunkBuffer>();
                foreach (var buffer in _buffers.Values)
                {
                    if (Math.Abs((lastSinglePlayerChunkPosition.X - buffer.ChunkLocation.X)) > ((vwp.VisibleChunkInWorld.X / 2.0) + _chunkRangeLookUp.X * 2) ||
                        Math.Abs((lastSinglePlayerChunkPosition.Z - buffer.ChunkLocation.Z)) > ((vwp.VisibleChunkInWorld.Y / 2.0) + _chunkRangeLookUp.Z * 2))
                    {
                        buffer2Remove.Add(buffer);
                    }
                }
                if (buffer2Remove.Count > 0)
                {
                    logger.Trace("Landscape buffer cleanup, {0} buffers removed", buffer2Remove.Count);

                    //remove the buffer that won't be used anymore
                    foreach (var b in buffer2Remove) _buffers.Remove(b.ChunkLocation);
                }

                Monitor.Exit(_syncLock);
            }
        }

        public void SetBufferPath(string Path)
        {
            _bufferPath = Path;
        }

        public LandscapeChunkBuffer Get(Vector3I chunkLocation)
        {
            LandscapeChunkBuffer buffer;
            while (TryGet(chunkLocation, out buffer) == false)
            {
                Thread.Sleep(1);
            }

            return buffer;
        }

        public bool TryGet(Vector3I chunkLocation, out LandscapeChunkBuffer buffer)
        {
            lock (_syncLock)
            {
                if (_buffers.TryGetValue(chunkLocation, out buffer) == false)
                {
                    buffer = new LandscapeChunkBuffer() { ChunkLocation = chunkLocation };
                    _buffers.Add(chunkLocation, buffer);
                }

                if (buffer.isReady) return true;
                if (buffer.isLocked) return false;
                buffer.isLocked = true;
            }

            GenerateLandscapeBuffer(buffer);
            buffer.isLocked = false;
            return true;
        }

        public void Insert(Vector3I chunkLocation, LandscapeEntity entity)
        {
            LandscapeChunkBuffer buffer;
            lock (_syncLock)
            {
                if (_buffers.TryGetValue(chunkLocation, out buffer) == false)
                {
                    buffer = new LandscapeChunkBuffer() { ChunkLocation = chunkLocation };
                    _buffers.Add(chunkLocation, buffer);
                }
            }

            if (buffer.Entities == null) buffer.Entities = new List<LandscapeEntity>();
            buffer.Entities.Add(entity);
        }
        #endregion

        #region Private Methods
        public void LoadBuffer()
        {
            if (_bufferPath == null || WithoutLandscapeBuffer) return;
            FileInfo fi = new FileInfo(_bufferPath);
            if (fi.Exists)
            {
                using (var ms = new FileStream(fi.FullName, FileMode.Open))
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    _buffers = Serializer.Deserialize<Dictionary<Vector3I, LandscapeChunkBuffer>>(zip);
                }
            }
        }

        private void SaveBuffer()
        {
            if (_bufferPath == null || WithoutLandscapeBuffer) return;

            //Serialize buffer, to easy loading back next session !
            using (var fs = new FileStream(_bufferPath, FileMode.Create))
            {
                using (var zip = new GZipStream(fs, CompressionMode.Compress))
                {
                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, _buffers);
                        //Serializer.SerializeWithLengthPrefix(ms, _buffer, PrefixStyle.Fixed32);
                        var array = ms.ToArray();
                        zip.Write(array, 0, array.Length);
                    }
                }
            }
        }

        private void GenerateLandscapeBuffer(LandscapeChunkBuffer buffer)
        {
            //Get the minimum chunk range that need to be computed to validate current chunk landscape entities generation
            var chunkRange = new Range3I(buffer.ChunkLocation - _chunkRangeLookUp, new Vector3I(_chunkRangeLookUp.X * 2, 1, _chunkRangeLookUp.Z * 2));
            var chunkBuffers = new List<LandscapeChunkBuffer>(chunkRange.Count);
            foreach (var chunkPosition in chunkRange)
            {
                //Check if all those chunks have been / have their landscape item processed (They could potentially impact this chunk).
                LandscapeChunkBuffer surrendingChunkBuffer;
                bool need2Process = false;
                lock (_syncLock)
                {
                    if (_buffers.TryGetValue(chunkPosition, out surrendingChunkBuffer) == false)
                    {
                        surrendingChunkBuffer = new LandscapeChunkBuffer { 
                            ChunkLocation = chunkPosition, 
                            ProcessingState = LandscapeChunkBuffer.LandscapeChunkBufferState.NotProcessed 
                        };
                        _buffers.Add(chunkPosition, surrendingChunkBuffer);
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
                    Processor.GenerateMacroLandscape(surrendingChunkBuffer.ChunkLocation, out chunkMasterBiome, out chunkBytes, out chunkNewRnd, out columnsInfo);

                    //Process chunk Entity landscape creation
                    Processor.LandscapeEntities.GenerateChunkItems(surrendingChunkBuffer.ChunkLocation, chunkMasterBiome, chunkBytes, columnsInfo, chunkNewRnd);
                   
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

            Processor.GenerateMicroLandscape(buffer);

            //Sort the final landscape entity items by their chunk origine.
            if(buffer.Entities != null) buffer.Entities = buffer.Entities.OrderBy(x => x.ChunkLocation.GetHashCode()).ToList();

            buffer.isReady = true;
        }
        #endregion



    }
}
