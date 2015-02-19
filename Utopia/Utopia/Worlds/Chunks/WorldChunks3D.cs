using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.World;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Provides chunk management using 3d chunk layout
    /// </summary>
    public class WorldChunks3D : DrawableGameComponent, IWorldChunks
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly D3DEngine _d3DEngine;
        private readonly CameraManager<ICameraFocused> _camManager;
        private readonly IPlayerManager _playerManager;
        private readonly Dictionary<Vector3I, VisualChunk3D> _chunks = new Dictionary<Vector3I, VisualChunk3D>();
        private VisualChunk3D[] _sortedChunks;

        public VisualWorldParameters VisualWorldParameters { get; set; }

        public VisualChunkBase GetChunk(Vector3I chunkPosition)
        {
            VisualChunk3D chunk;
            _chunks.TryGetValue(chunkPosition, out chunk);
            return chunk;
        }

        public IEnumerable<VisualChunk3D> VisibleChunks()
        {
            return _sortedChunks.Where(chunk => !chunk.Graphics.IsFrustumCulled);
        }

        public VisualChunkBase GetBaseChunk(Vector3I chunkPosition)
        {
            return GetChunk(chunkPosition);
        }

        public bool ResyncChunk(Vector3I chunkPosition, bool forced)
        {
            Logger.Warn("Requested chunk resync, but not implemented!");
            //TODO: implement the method
            return false;
        }

        public bool ShowDebugInfo { get; set; }

        public WorldChunks3D(   D3DEngine d3DEngine,
                                CameraManager<ICameraFocused> camManager,
                                IPlayerManager player)
        {
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _playerManager = player;
        }

        /// <summary>
        /// Sort the chunks array if needed
        /// </summary>
        private void SortChunks()
        {
            if (_sortedChunks != null || _camManager.ActiveCamera == null) 
                return;

            //Compute Distance Squared from Chunk Center to Camera
            foreach (var chunk in _chunks.Values)
            {
                chunk.DistanceFromPlayer = MVector3.Distance2D(chunk.ChunkCenter, _playerManager.CameraWorldPosition);
            }

            _sortedChunks = _chunks.Values.OrderBy(x => x.DistanceFromPlayer).ToArray();
        }

        private void ChunkVisibilityTest()
        {
            foreach (var chunk in _sortedChunks)
            {
                chunk.Graphics.IsFrustumCulled = !_camManager.ActiveCamera.Frustum.IntersectsWithoutFar(ref chunk.ChunkWorldBoundingBox);
            }
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            SortChunks();
            ChunkVisibilityTest();


        }

        public string GetDebugInfo()
        {
            return null;
        }
    }
}
