using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Structs;
using SharpDX.Direct3D;
using S33M3Engines.Buffers;
using S33M3Engines;
using S33M3Engines.Textures;
using SharpDX.Direct3D11;
using S33M3Engines.StatesManager;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using System.Diagnostics;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Shared.Chunks.Entities;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Managers
{
    public class StaticEntityManager : DrawableGameComponent, IStaticEntityManager
    {
        #region Private variables
        private D3DEngine _d3dEngine;
        private WorldFocusManager _worldFocusManager;
        private IStaticSpriteEntityRenderer _spriteRenderer;
        private VisualSpriteEntity[] _spriteEntitiesToRender;
        private IWorldChunks _worldChunks;
        private PlayerEntityManager _player;
        #endregion

        #region Public variables/properties
        #endregion

        public StaticEntityManager(D3DEngine d3dEngine,
                                   WorldFocusManager worldFocusManager,
                                   IStaticSpriteEntityRenderer spriteRenderer,
                                   IWorldChunks worldChunks,
                                   PlayerEntityManager player)
        {
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _spriteRenderer = spriteRenderer;
            _worldChunks = worldChunks;
            _player = player;

            this.UpdateOrder = 11;
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
        }

        public override void Dispose()
        {
        }
        #region Private Methods
        #endregion

        #region Public Methods
        public override void Update(ref GameTime timeSpent)
        {
            VisualChunk chunk;
            double minDistanceChunkModified = double.MaxValue;
            double currentChunkModifiedDistance;
            _spriteRenderer.BeginSpriteCollectionRefresh();
            //Check inside the visible chunks (Not frustum culled) the statics entities that needs to be rendered
            for (int i = 0; i < _worldChunks.Chunks.Length; i++)
            {
                chunk = _worldChunks.SortedChunks[i];
                if (chunk.State != ChunkState.DisplayInSyncWithMeshes)
                {
                    currentChunkModifiedDistance = Vector3D.Distance(_player.VisualEntity.Position, new Vector3D(chunk.ChunkPositionBlockUnit.X, _player.VisualEntity.Position.Y, chunk.ChunkPositionBlockUnit.Y));
                    if (currentChunkModifiedDistance < minDistanceChunkModified) minDistanceChunkModified = currentChunkModifiedDistance;
                }
                if (chunk.isFrustumCulled == false && chunk.State == ChunkState.DisplayInSyncWithMeshes)
                {
                    for (int j = 0; j < chunk.VisualSpriteEntities.Count; j++)
                    {
                        _spriteRenderer.AddPointSpriteVertex(ref chunk.VisualSpriteEntities[j].Vertex);
                    }
                }
            }

            if (minDistanceChunkModified >= 48)
                _spriteRenderer.Update(ref timeSpent);
        }
        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public override void Draw(int Index)
        {
            _spriteRenderer.Draw(Index);
        }

        #endregion
    }
}

