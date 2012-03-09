using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_DXEngine;

namespace Utopia.Components
{
    /// <summary>
    /// Provides entities and landscape physics interaction by means of the BEPUPhysics engine
    /// </summary>
    public class BepuPhysicsComponent : GameComponent
    {
        private readonly IWorldChunks _chunkManager;
        private readonly ActionsManager _actionsManager;
        private readonly D3DEngine _engine;
        private readonly Dictionary<VisualChunk, StaticMesh> _meshes = new Dictionary<VisualChunk, StaticMesh>();
        private VisualChunk[] _chunks;
        private Space _space;
        private readonly Queue<double> _perfCounter = new Queue<double>();


        /// <summary>
        /// Gets BepuPhysics space object
        /// </summary>
        public Space Space
        {
            get { return _space; }
        }

        /// <summary>
        /// Creates new instance of BEPUPhysics component
        /// </summary>
        /// <param name="chunkManager"></param>
        /// <param name="actionsManager"></param>
        public BepuPhysicsComponent(IWorldChunks chunkManager, ActionsManager actionsManager, D3DEngine engine)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");
            if (actionsManager == null) throw new ArgumentNullException("actionsManager");
            if (engine == null) throw new ArgumentNullException("engine");
            _chunkManager = chunkManager;
            _actionsManager = actionsManager;
            _engine = engine;

            // when array is initialized we need to listen all visual chunks mesh update event
            _chunkManager.ChunksArrayInitialized += ChunkManagerChunksArrayInitialized;

        }

        public override void Initialize()
        {
            _space = new Space();

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    Space.ThreadManager.AddThread();
                }
            }

            ListenChunks();
        }

        void ChunkManagerChunksArrayInitialized(object sender, EventArgs e)
        {
            ListenChunks();
        }

        private void ListenChunks()
        {
            RemoveChunksListening();

            _chunks = _chunkManager.Chunks;

            if (_chunks != null)
            {
                foreach (var visualChunk in _chunks)
                {
                    visualChunk.ChunkMeshUpdated += VisualChunkChunkMeshUpdated;
                }
            }
        }

        private void RemoveChunksListening()
        {
            if (_chunks != null)
            {
                foreach (var visualChunk in _chunks)
                {
                    visualChunk.ChunkMeshUpdated -= VisualChunkChunkMeshUpdated;
                }
                _chunks = null;
            }
        }

        void VisualChunkChunkMeshUpdated(object sender, EventArgs e)
        {
            var visualChunk = (VisualChunk)sender;

            // move of the chunk
            var gMove = new Vector3(visualChunk.ChunkPosition.X * AbstractChunk.ChunkSize.X, 0, visualChunk.ChunkPosition.Y * AbstractChunk.ChunkSize.Z);

            StaticMesh staticMesh;

            lock (_meshes)
            {
                if (_meshes.TryGetValue(visualChunk, out staticMesh))
                {
                    _space.Remove(staticMesh);
                    _meshes.Remove(visualChunk);
                }

                staticMesh = new StaticMesh(gMove, visualChunk.SolidCubeVertices, visualChunk.SolidCubeIndices);

                _meshes.Add(visualChunk, staticMesh);
                _space.Add(staticMesh);
            }

        }

        public override void Update(GameTime timeSpend)
        {
            if (_actionsManager.isTriggered(Actions.EntityUse))
            {

            }


            var sw = Stopwatch.StartNew();
            lock (_meshes)
            {
                _space.Update();
            }
            sw.Stop();
            _perfCounter.Enqueue(sw.Elapsed.TotalMilliseconds);

            if (_perfCounter.Count > 10)
                _perfCounter.Dequeue();
        }

        public override void Dispose()
        {
            _chunkManager.ChunksArrayInitialized -= ChunkManagerChunksArrayInitialized;
            RemoveChunksListening();
            if (_space != null)
            {
                _space.Dispose();
                _space = null;
            }
        }

        public string GetInfo()
        {
            if (_perfCounter.Count == 0) return "Loading";
            return string.Format("BepuPhysics avg time: {0} ms, {1} meshes", Math.Round(_perfCounter.Average(), 1), _meshes.Count);
        }
    } 
}
