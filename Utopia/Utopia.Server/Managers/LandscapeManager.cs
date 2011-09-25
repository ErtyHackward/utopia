using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using S33M3Engines.Shared.Math;
using Utopia.Server.AStar;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides functions to work with landscape
    /// </summary>
    public class LandscapeManager : IDisposable
    {
        private readonly IChunksStorage _chunksStorage;
        private readonly WorldGenerator _generator;
        private readonly HashSet<ServerChunk> _chunksToSave = new HashSet<ServerChunk>();
        private readonly Queue<AStar<AStarNode3D>> _pathPool = new Queue<AStar<AStarNode3D>>();
        
        private delegate Path3D CalculatePathDelegate(Vector3I start, Vector3I goal);
        public delegate void PathCalculatedDeleagte(Path3D path);

        /// <summary>
        /// Gets main server memory chunk storage.
        /// </summary>
        private readonly Dictionary<Vector2I, ServerChunk> _chunks = new Dictionary<Vector2I, ServerChunk>();
        
        #region Events

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkLoaded;

        protected void OnChunkLoaded(LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged += ChunkBlocksChanged;

            var handler = ChunkLoaded;
            if (handler != null) handler(this, e);
        }

        void ChunkBlocksChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            var serverChunk = (ServerChunk)sender;
            serverChunk.NeedSave = true;

            lock (_chunksToSave)
            {
                if(!_chunksToSave.Contains(serverChunk))
                    _chunksToSave.Add(serverChunk);
            }

        }

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkUnloaded;

        protected void OnChunkUnloaded(LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged -= ChunkBlocksChanged;

            EventHandler<LandscapeManagerChunkEventArgs> handler = ChunkUnloaded;
            if (handler != null) handler(this, e);
        }

        #endregion

        public WorldGenerator WorldGenerator
        {
            get { return _generator; }
        }

        public int ChunksInMemory
        {
            get { return _chunks.Count; }
        }

#if DEBUG
        /// <summary>
        /// Gets time of last executed save operation
        /// </summary>
        public double SaveTime { get; set; }
        /// <summary>
        /// Gets amount of chunks saved last time
        /// </summary>
        public int ChunksSaved { get; set; }
#endif 

        public LandscapeManager(IChunksStorage chunksStorage, WorldGenerator generator)
        {
            if (chunksStorage == null) throw new ArgumentNullException("chunksStorage");
            if (generator == null) throw new ArgumentNullException("generator");

            _chunksStorage = chunksStorage;
            _generator = generator;
        }

        /// <summary>
        /// Gets chunk. First it tries to get cached in memory value, then it checks the database, and then it generates the chunk
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public ServerChunk GetChunk(Vector2I position)
        {
            ServerChunk chunk = null;
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
                            var generatedChunk = _generator.GetChunk(position);

                            if (generatedChunk != null)
                            {
                                chunk = new ServerChunk(generatedChunk) { Position = position, LastAccess = DateTime.Now };
                            }
                        }
                        else
                        {
                            chunk = new ServerChunk { Position = position, CompressedBytes = data };
                            chunk.Decompress();
                        }

                        _chunks.Add(position, chunk);
                        OnChunkLoaded(new LandscapeManagerChunkEventArgs { Chunk = chunk });
                    }
                    else chunk = _chunks[position];
                }
            }
            return chunk;
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
                    _chunks.Remove(chunk.Position);
                    OnChunkUnloaded(new LandscapeManagerChunkEventArgs { Chunk = chunk });
                }
            }
        }

        public void SaveChunks()
        {
            lock (_chunksToSave)
            {
                if (_chunksToSave.Count == 0)
                    return;
                
#if DEBUG
                SaveTime = 0;
                ChunksSaved = 0;
                var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
                var positions = new Vector2I[_chunksToSave.Count];
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
#if DEBUG
                SaveTime = sw.Elapsed.TotalMilliseconds;
                ChunksSaved = positions.Length;
#endif
            }
        }

        /// <summary>
        /// Provides chunk and internal chunk position of global Vector3
        /// </summary>
        /// <param name="position">Global position</param>
        /// <param name="chunk">a chunk containing this position</param>
        /// <param name="cubePosition">a cube position inside the chunk</param>
        public void GetBlockAndChunk(Vector3D position, out ServerChunk chunk, out Vector3I cubePosition)
        {
            cubePosition.X = (int)Math.Floor(position.X);
            cubePosition.Y = (int)Math.Floor(position.Y);
            cubePosition.Z = (int)Math.Floor(position.Z);

            chunk = GetChunk(new Vector2I(cubePosition.X / AbstractChunk.ChunkSize.X, cubePosition.Z / AbstractChunk.ChunkSize.Z));

            cubePosition.X = cubePosition.X % AbstractChunk.ChunkSize.X;
            cubePosition.Z = cubePosition.Z % AbstractChunk.ChunkSize.Z;
        }

        /// <summary>
        /// Gets landscape cursor
        /// </summary>
        /// <param name="blockPosition">global block position</param>
        /// <returns></returns>
        public LandscapeCursor GetCursor(Vector3I blockPosition)
        {
            return new LandscapeCursor(this, blockPosition);
        }

        public LandscapeCursor GetCursor(Vector3D entityPosition)
        {
            return GetCursor(new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z)));
        }

        /// <summary>
        /// Returns block position based on entity position
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        public static Vector3I EntityToBlockPosition(Vector3D entityPosition)
        {
            return new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z));
        }

        /// <summary>
        /// Calculates path asynchronously and fires callback when done
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="callback"></param>
        public void CalculatePathAsync(Vector3I start, Vector3I goal, PathCalculatedDeleagte callback)
        {
            var d = new CalculatePathDelegate(CalculatePath);
            d.BeginInvoke(start, goal, PathCalculated, callback);
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
        public Path3D CalculatePath(Vector3I start, Vector3I goal)
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
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            calculator.FindPath(startNode);
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
            lock (_chunks)
            {
                _chunks.Clear();
            }
        }
    }
}
