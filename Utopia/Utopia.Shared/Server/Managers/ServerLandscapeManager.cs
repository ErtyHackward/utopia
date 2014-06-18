using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.AStar;
using Utopia.Shared.Server.Events;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;
using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;

namespace Utopia.Shared.Server.Managers
{
    /// <summary>
    /// Provides functions to work with chunk based landscape
    /// </summary>
    public class ServerLandscapeManager : LandscapeManager<ServerChunk>, IDisposable, IServerLandscapeManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _server;
        private readonly IChunksStorage _chunksStorage;
        private readonly WorldGenerator _generator;
        private readonly HashSet<ServerChunk> _chunksToSave = new HashSet<ServerChunk>();
        private readonly Queue<AStar<AStarNode3D>> _pathPool = new Queue<AStar<AStarNode3D>>();
        private readonly Stopwatch _saveStopwatch = new Stopwatch();

        private readonly Timer _cleanUpTimer;
        private readonly Timer _saveTimer;

        private delegate Path3D CalculatePathDelegate(Vector3I start, Vector3I goal, Predicate<AStarNode3D> isGoal = null);
        public delegate void PathCalculatedDeleagte(Path3D path);

        /// <summary>
        /// Gets main server memory chunk storage.
        /// </summary>
        private readonly Dictionary<Vector3I, ServerChunk> _chunks = new Dictionary<Vector3I, ServerChunk>();
        
        #region Events

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkLoaded;

        protected void OnChunkLoaded(LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged += ChunkBlocksChanged;
            e.Chunk.Entities.CollectionDirty += EntitiesCollectionDirty;

            var handler = ChunkLoaded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkUnloaded;
        protected void OnChunkUnloaded(LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged -= ChunkBlocksChanged;
            e.Chunk.Entities.CollectionDirty -= EntitiesCollectionDirty;

            //Flush the generator buffered chunk if needed.
            _generator.FlushBuffers(e.Chunk.Position);

            var handler = ChunkUnloaded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ServerLandscapeManagerBlockChangedEventArgs> BlockChanged;
        private void OnBlockChanged(ServerLandscapeManagerBlockChangedEventArgs e)
        {
            var handler = BlockChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        public int ChunkLiveTimeMinutes { get; set; }

        public int CleanUpInterval { get; set; }

        public int SaveInterval { get; set; }

        public int ChunkCountLimit { get; set; }

        public WorldGenerator WorldGenerator
        {
            get { return _generator; }
        }

        public int ChunksInMemory
        {
            get { return _chunks.Count; }
        }

        public EntityFactory EntityFactory { get; internal set; }

        /// <summary>
        /// Gets time of last executed save operation
        /// </summary>
        public double SaveTime { get; set; }

        /// <summary>
        /// Gets amount of chunks saved last time
        /// </summary>
        public int ChunksSaved { get; set; }

        public ServerLandscapeManager(ServerCore server, IChunksStorage chunksStorage, WorldGenerator generator, EntityFactory factory, int chunkLiveTimeMinutes, int cleanUpInterval, int saveInterval, int chunksLimit, WorldParameters wp)
            : base(wp)
        {
            ChunkLiveTimeMinutes = chunkLiveTimeMinutes;
            CleanUpInterval = cleanUpInterval;
            SaveInterval = saveInterval;
            ChunkCountLimit = chunksLimit;
            EntityFactory = factory;
            

            if (chunksStorage == null) throw new ArgumentNullException("chunksStorage");
            if (generator == null) throw new ArgumentNullException("generator");

            _server = server;
            _chunksStorage = chunksStorage;
            _generator = generator;

            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            _cleanUpTimer = new Timer(CleanUp, null, CleanUpInterval, CleanUpInterval);
            _saveTimer = new Timer(SaveChunks, null, SaveInterval, SaveInterval);
        }

        private void RequestSave(ServerChunk chunk)
        {
            chunk.NeedSave = true;
            chunk.PureGenerated = false;

            if (Monitor.TryEnter(_chunksToSave,5000))
            {
                if (!_chunksToSave.Contains(chunk))
                    _chunksToSave.Add(chunk);
                Monitor.Exit(_chunksToSave);
            }
            else
            {
                logger.Debug("Unable to aquire lock to save the chunk");
            }
        }

        void ChunkBlocksChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            var ea = new ServerLandscapeManagerBlockChangedEventArgs((IAbstractChunk)sender, e);

            OnBlockChanged(ea);
            var serverChunk = (ServerChunk)sender;
            RequestSave(serverChunk);
        }

        void EntitiesCollectionDirty(object sender, EventArgs e)
        {
            RequestSave(((EntityCollection)sender).Chunk as ServerChunk);
        }

        void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageGetChunks += ConnectionMessageGetChunks;
        }

        void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageGetChunks -= ConnectionMessageGetChunks;
        }
        
        private void ConnectionMessageGetChunks(object sender, ProtocolMessageEventArgs<GetChunksMessage> e)
        {
            var connection = (ClientConnection)sender;

            //TraceHelper.Write("GetChunks!" + e.Message.Range.Position + " " + e.Message.Range.Size);

            try
            {
                var range = e.Message.Range;

                // list to get indicies
                var positionsList = e.Message.Positions == null ? null : new List<Vector3I>(e.Message.Positions);

                range.Foreach(pos =>
                {

                    var chunk = _server.LandscapeManager.GetChunk(pos);

                    if (e.Message.Flag == GetChunksMessageFlag.AlwaysSendChunkData)
                    {
                        goto sendAllData;
                    }

                    //goto sendAllData;

                    // do we have hashes from client?
                    if (e.Message.HashesCount > 0 && positionsList != null)
                    {
                        //Has the position from the Range has been forwarded inside the location/hash arrays ??
                        int hashIndex = positionsList.IndexOf(pos);

                        if (hashIndex != -1)
                        {
                            if (e.Message.Md5Hashes[hashIndex] == chunk.GetMd5Hash())
                            {
                                connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkMd5Equal, ChunkHash = chunk.GetMd5Hash() });
                                return;
                            }
                        }
                    }

                    if (chunk.PureGenerated)
                    {
                        connection.Send(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkCanBeGenerated, ChunkHash = chunk.GetMd5Hash() });
                        return;
                    }

                sendAllData:
                    // send data anyway
                    connection.Send(new ChunkDataMessage
                    {
                        Position = pos,
                        ChunkHash = chunk.GetMd5Hash(),
                        Flag = ChunkDataMessageFlag.ChunkWasModified,
                        Data = chunk.Compress()
                    });

                });
            }
            catch (IOException)
            {
                // client was disconnected
            }
        }

        
        private void TrimChunks()
        {
            if (_chunks.Count <= ChunkCountLimit)
                return;

            lock (_chunks)
            {
                var list = new List<ServerChunk>(_chunks.Values);

                list.Sort((c1, c2) => c2.LastAccess.CompareTo(c1.LastAccess));

                for (var i = list.Count-1; i > ChunkCountLimit; i--)
                {
                    RemoveChunk(list[i]);
                }
            }
        }

        // this functions executes in other thread
        private void CleanUp(object o)
        {
            CleanUp(ChunkLiveTimeMinutes);
        }

        // this functions executes in other thread
        private void SaveChunks(object obj)
        {
            SaveChunks();
            if (ChunksSaved > 0)
            {
                logger.Info("Chunks saved: {1} Took: {0} ms", SaveTime, ChunksSaved);
            }
        }

        /// <summary>
        /// Returns a copy of list of all chunks in memory
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServerChunk> GetBufferedChunks()
        {
            List<ServerChunk> chunks;
            lock (_chunks)
            {
                chunks = new List<ServerChunk>(_chunks.Values);
            }

            return chunks;
        }

        public ServerChunk GenerateChunk(Vector3I chunkPos)
        {
            var generatedChunk = _generator.GetChunk(chunkPos);

            if (generatedChunk != null)
            {
                return new ServerChunk(generatedChunk) { Position = chunkPos, LastAccess = DateTime.Now };
            }
            return null;
        }

        public void WipeChunk(Vector3I chunkPos)
        {
            var chunk = GetChunk(chunkPos);

            if (chunk.PureGenerated)
                return;

            RemoveChunk(chunk);

            chunk = GenerateChunk(chunkPos);
            RequestSave(chunk);
            SaveChunks();
        }

        /// <summary>
        /// Gets chunk. First it tries to get cached in memory value, then it checks the database, and then it generates the chunk
        /// </summary>
        /// <param name="position">chunk position</param>
        /// <returns></returns>
        public override ServerChunk GetChunk(Vector3I position)
        {
            ServerChunk chunk;
            // search chunk in memory or load it
            if (_chunks.ContainsKey(position))
            {
                chunk = _chunks[position];
                chunk.LastAccess = DateTime.Now;
            }
            else
            {
                lock (_chunks)
                {
                    if (!_chunks.ContainsKey(position))
                    {
                        var data = _chunksStorage.LoadChunkData(position);

                        if (data == null)
                        {
                            chunk = GenerateChunk(position);
                        }
                        else
                        {

                            try
                            {
                                chunk = new ServerChunk { Position = position };
                                chunk.Decompress(data);
                                EntityFactory.PrepareEntities(chunk.Entities);
                            }
                            catch (Exception e)
                            {
                                logger.Error("Error when decompressing chunk {1}: {0}", e.Message, position);
                                chunk = GenerateChunk(position);
                            }
                        }

                        _chunks.Add(position, chunk);
                        OnChunkLoaded(new LandscapeManagerChunkEventArgs { Chunk = chunk });

                        if (_chunks.Count > ChunkCountLimit)
                        {
                            // remove excess chunks
                            new ThreadStart(TrimChunks).BeginInvoke(null, null);
                        }
                    }
                    else
                    {
                        chunk = _chunks[position];
                        chunk.LastAccess = DateTime.Now;
                    }
                }
            }
            return chunk;
        }

        public override TerraCube GetCubeAt(Vector3I vector3I)
        {
            Vector3I internalPos;
            Vector3I chunkPos;

            BlockHelper.GlobalToLocalAndChunkPos(vector3I, out internalPos, out chunkPos);

            var chunk = GetChunk(chunkPos);

            return new TerraCube(chunk.BlockData[internalPos]);
        }

        public override ILandscapeCursor GetCursor(Vector3I blockPosition)
        {
            return new LandscapeCursor(this, blockPosition, _wp);
        }

        public void CleanUp(int chunkAgeMinutes)
        {
            var chunksToRemove = new List<ServerChunk>();

            lock (_chunks)
            {
                // remove all chunks that was used very long time ago                    
                chunksToRemove.AddRange(_chunks.Values.Where(chunk => chunk.LastAccess < DateTime.Now.AddMinutes(-chunkAgeMinutes)));

                foreach (var chunk in chunksToRemove)
                {
                    RemoveChunk(chunk);
                }
            }
        }

        private void RemoveChunk(ServerChunk chunk)
        {
            _chunks.Remove(chunk.Position);
            OnChunkUnloaded(new LandscapeManagerChunkEventArgs { Chunk = chunk });
        }

        public void SaveChunks()
        {
            if (Monitor.TryEnter(_chunksToSave, 5000))
            {
                try
                {
                    SaveTime = 0;
                    ChunksSaved = 0;

                    if (_chunksToSave.Count == 0)
                        return;

                    _saveStopwatch.Restart();
                    var positions = new Vector3I[_chunksToSave.Count];
                    var datas = new List<byte[]>(_chunksToSave.Count);

                    int index = 0;
                    foreach (var serverChunk in _chunksToSave)
                    {
                        serverChunk.NeedSave = false;
                        positions[index] = serverChunk.Position;
                        datas.Add(serverChunk.Compress());
                        index++;
                    }

                    _chunksStorage.SaveChunksData(positions, datas.ToArray());
                    _chunksToSave.Clear();
                    _saveStopwatch.Stop();

                    SaveTime = _saveStopwatch.Elapsed.TotalMilliseconds;
                    ChunksSaved = positions.Length;
                }
                finally
                {
                    Monitor.Exit(_chunksToSave);
                }
            }
            else
            {
                logger.Debug("Unable to aquire lock for saving chunks in 5sec, skipping");
            }
        }

        /// <summary>
        /// Calculates path asynchronously and fires callback when done
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="callback"></param>
        public void CalculatePathAsync(Vector3I start, Vector3I goal, PathCalculatedDeleagte callback, Predicate<AStarNode3D> isGoal = null)
        {
            var d = new CalculatePathDelegate(CalculatePath);
            d.BeginInvoke(start, goal, isGoal, PathCalculated, callback);
        }

        private void PathCalculated(IAsyncResult result)
        {
            var d = (CalculatePathDelegate)((AsyncResult)result).AsyncDelegate;
            var resultDelegate = (PathCalculatedDeleagte)result.AsyncState;
            var path = d.EndInvoke(result);
            resultDelegate(path);
        }

        /// <summary>
        /// Calculates path in current thread and returns the result
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public Path3D CalculatePath(Vector3I start, Vector3I goal, Predicate<AStarNode3D> isGoalNode = null)
        {
            AStar<AStarNode3D> calculator = null;
            lock (_pathPool)
            {
                if (_pathPool.Count > 0)
                    calculator = _pathPool.Dequeue();
            }

            if (calculator == null)
                calculator = new AStar<AStarNode3D>();

            var goalNode = new AStarNode3D(GetCursor(goal), null, null, 1);
            var startNode = new AStarNode3D(GetCursor(start), null, goalNode, 1);

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            calculator.FindPath(startNode, isGoalNode);
#if DEBUG
            sw.Stop();
#endif
            var path = new Path3D { Start = start, Goal = goal };
#if DEBUG
            path.PathFindTime = sw.Elapsed.TotalMilliseconds;
            path.IterationsPerformed = calculator.Iterations;
#endif

            if (calculator.Solution != null)
            {
                var list = new List<Vector3I>();
                
                foreach (var node3D in calculator.Solution)
                {
                    list.Add(node3D.Cursor.GlobalPosition);
                }

                path.Points = list;
            }

            lock (_pathPool)
            {
                _pathPool.Enqueue(calculator);
            }

            return path;
        }

        public void Dispose()
        {
            SaveChunks();

            lock (_chunks)
            {
                _chunks.Clear();
            }

            _cleanUpTimer.Dispose();
            _saveTimer.Dispose();

            SaveChunks();
        }

        
    }
}
