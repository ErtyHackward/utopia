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
        private readonly List<ServerChunk> _saveList = new List<ServerChunk>();
        private readonly Queue<AStar<AStarNode3D>> _pathPool = new Queue<AStar<AStarNode3D>>();
        
        private delegate Path3D CalculatePathDelegate(Location3<int> start, Location3<int> goal);
        public delegate void PathCalculatedDeleagte(Path3D path);

        /// <summary>
        /// Gets main server memory chunk storage.
        /// </summary>
        private readonly Dictionary<IntVector2, ServerChunk> _chunks = new Dictionary<IntVector2, ServerChunk>();
        
        #region Events

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkLoaded;

        protected void OnChunkLoaded(LandscapeManagerChunkEventArgs e)
        {
            var handler = ChunkLoaded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<LandscapeManagerChunkEventArgs> ChunkUnloaded;

        protected void OnChunkUnloaded(LandscapeManagerChunkEventArgs e)
        {
            EventHandler<LandscapeManagerChunkEventArgs> handler = ChunkUnloaded;
            if (handler != null) handler(this, e);
        }

        #endregion

        public WorldGenerator WorldGenerator
        {
            get { return _generator; }
        }

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
        public ServerChunk GetChunk(IntVector2 position)
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
                            chunk = new ServerChunk { CompressedBytes = data };
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
            if (_saveList.Count == 0)
                return;

            lock (_saveList)
            {
                var positions = new IntVector2[_saveList.Count];
                var datas = new List<byte[]>(_saveList.Count);

                for (int i = 0; i < _saveList.Count; i++)
                {
                    _saveList[i].NeedSave = false;
                    positions[i] = _saveList[i].Position;
                    datas.Add(_saveList[i].CompressedBytes);
                }

                _chunksStorage.SaveChunksData(positions, datas.ToArray());
                _saveList.Clear();
            }
        }

        /// <summary>
        /// Provides chunk and internal chunk position of global Vector3
        /// </summary>
        /// <param name="position">Global position</param>
        /// <param name="chunk">a chunk containing this position</param>
        /// <param name="cubePosition">a cube position inside the chunk</param>
        public void GetBlockAndChunk(DVector3 position, out ServerChunk chunk, out Location3<int> cubePosition)
        {
            cubePosition.X = (int)Math.Floor(position.X);
            cubePosition.Y = (int)Math.Floor(position.Y);
            cubePosition.Z = (int)Math.Floor(position.Z);

            chunk = GetChunk(new IntVector2(cubePosition.X / AbstractChunk.ChunkSize.X, cubePosition.Z / AbstractChunk.ChunkSize.Z));

            cubePosition.X = cubePosition.X % AbstractChunk.ChunkSize.X;
            cubePosition.Z = cubePosition.Z % AbstractChunk.ChunkSize.Z;
        }

        /// <summary>
        /// Gets landscape cursor
        /// </summary>
        /// <param name="blockPosition">global block position</param>
        /// <returns></returns>
        public LandscapeCursor GetCursor(Location3<int> blockPosition)
        {
            return new LandscapeCursor(this, blockPosition);
        }

        public LandscapeCursor GetCursor(DVector3 entityPosition)
        {
            return GetCursor(new Location3<int>((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z)));
        }

        /// <summary>
        /// Returns block position based on entity position
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        public static Location3<int> EntityToBlockPosition(DVector3 entityPosition)
        {
            return new Location3<int>((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z));
        }

        /// <summary>
        /// Calculates path asynchronously and fires callback when done
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="callback"></param>
        public void CalculatePathAsync(Location3<int> start, Location3<int> goal, PathCalculatedDeleagte callback)
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
        public Path3D CalculatePath(Location3<int> start, Location3<int> goal)
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

            calculator.FindPath(startNode);

            var path = new Path3D { Start = start, Goal = goal };

            var list = new List<Location3<int>>();

            foreach (var node3D in calculator.Solution)
            {
                list.Add(node3D.Cursor.GlobalPosition);
            }

            path.Points = list;

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
