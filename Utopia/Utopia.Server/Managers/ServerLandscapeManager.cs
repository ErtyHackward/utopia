using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using S33M3Engines.Shared.Math;
using Utopia.Server.AStar;
using Utopia.Server.Events;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides functions to work with chunk based landscape
    /// </summary>
    public class ServerLandscapeManager : LandscapeManager<ServerChunk>, IDisposable
    {
        private readonly Server _server;
        private readonly IChunksStorage _chunksStorage;
        private readonly WorldGenerator _generator;
        private readonly HashSet<ServerChunk> _chunksToSave = new HashSet<ServerChunk>();
        private readonly Queue<AStar<AStarNode3D>> _pathPool = new Queue<AStar<AStarNode3D>>();
        private readonly Stopwatch _saveStopwatch = new Stopwatch();

        private readonly Timer _cleanUpTimer;
        private readonly Timer _saveTimer;

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

            var handler = ChunkUnloaded;
            if (handler != null) handler(this, e);
        }

        #endregion

        public int ChunkLiveTimeMinutes { get; set; }

        public int CleanUpInterval { get; set; }

        public int SaveInterval { get; set; }

        public WorldGenerator WorldGenerator
        {
            get { return _generator; }
        }

        public int ChunksInMemory
        {
            get { return _chunks.Count; }
        }

        /// <summary>
        /// Gets time of last executed save operation
        /// </summary>
        public double SaveTime { get; set; }

        /// <summary>
        /// Gets amount of chunks saved last time
        /// </summary>
        public int ChunksSaved { get; set; }

        public ServerLandscapeManager(Server server, IChunksStorage chunksStorage, WorldGenerator generator, int chunkLiveTimeMinutes, int cleanUpInterval, int saveInterval)
        {
            ChunkLiveTimeMinutes = chunkLiveTimeMinutes;
            CleanUpInterval = cleanUpInterval;
            SaveInterval = saveInterval;

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
                var positionsList = e.Message.Positions == null ? null : new List<Vector2I>(e.Message.Positions);

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
                                connection.SendAsync(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkMd5Equal, ChunkHash = chunk.GetMd5Hash() });
                                return;
                            }
                        }
                    }

                    if (chunk.PureGenerated)
                    {
                        connection.SendAsync(new ChunkDataMessage { Position = pos, Flag = ChunkDataMessageFlag.ChunkCanBeGenerated, ChunkHash = chunk.GetMd5Hash() });
                        return;
                    }

                sendAllData:
                    // send data anyway
                    connection.SendAsync(new ChunkDataMessage
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
                TraceHelper.Write("Chunks saved: {1} Took: {0} ms", SaveTime,
                                  ChunksSaved);
            }
        }

        /// <summary>
        /// Gets chunk. First it tries to get cached in memory value, then it checks the database, and then it generates the chunk
        /// </summary>
        /// <param name="position">chunk position</param>
        /// <returns></returns>
        public override ServerChunk GetChunk(Vector2I position)
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

                SaveTime = 0;
                ChunksSaved = 0;

                if (_chunksToSave.Count == 0)
                    return;

                _saveStopwatch.Restart();
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
                _saveStopwatch.Stop();

                SaveTime = _saveStopwatch.Elapsed.TotalMilliseconds;
                ChunksSaved = positions.Length;

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
            var sw = Stopwatch.StartNew();
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

            _cleanUpTimer.Dispose();
            _saveTimer.Dispose();
        }

        public IEnumerable<ServerChunk> SurroundChunks(Vector3D vector3D, float radius = 10)
        {
            // first we check current chunk, then 8 surrounding, then 16

            var chunkPosition = new Vector2I((int)Math.Floor(vector3D.X / AbstractChunk.ChunkSize.X), (int)Math.Floor(vector3D.Z / AbstractChunk.ChunkSize.Z));

            yield return GetChunk(chunkPosition);


            for (int i = 1; i * AbstractChunk.ChunkSize.X < radius; i++) // can be easily rewrited to handle situation when X and Z is not equal, hope it will not happen...
            {
                for (int x = -i; x <= i; x++)
                {
                    for (int y = -i; y <= i; y++)
                    {
                        // checking only border chunks
                        if (x == -i || x == i || y == -i || y == i)
                        {
                            yield return GetChunk(new Vector2I(chunkPosition.X + x, chunkPosition.Y + y));
                        }
                    }
                }
            }
        }

        public Vector3D GetHighestPoint(Vector3D vector2)
        {
            var chunk = GetChunk(vector2);

            var cx = (int)vector2.X % AbstractChunk.ChunkSize.X;
            var cz = (int)vector2.Z % AbstractChunk.ChunkSize.Z;

            if (cx < 0) cx = AbstractChunk.ChunkSize.X - cx;
            if (cz < 0) cz = AbstractChunk.ChunkSize.Z - cz;

            int y;

            for (y = 127; y >= 0; y--)
            {
                if(chunk.BlockData.GetBlock(new Vector3I(cx, y, cz)) != CubeId.Air)
                    break;
                
            }

            return new Vector3D(vector2.X, y + 1, vector2.Z); 
        }
    }
}
