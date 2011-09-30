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
        private int _spriteEntitiesToRenderNbr;
        private IWorldChunks _worldChunks;
        private readonly int _spriteMaxSize = 50000;
        #endregion

        #region Public variables/properties
        #endregion

        public StaticEntityManager(D3DEngine d3dEngine,
                                   WorldFocusManager worldFocusManager,
                                   IStaticSpriteEntityRenderer spriteRenderer,
                                   IWorldChunks worldChunks)
        {
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _spriteRenderer = spriteRenderer;
            _spriteEntitiesToRender = new VisualSpriteEntity[_spriteMaxSize];
            _spriteRenderer.SpriteEntities = _spriteEntitiesToRender;
            _worldChunks = worldChunks;
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
            _spriteEntitiesToRenderNbr = -1;

            VisualChunk chunk;
            //Check inside the visible chunks (Not visible culled) the statics entities that needs to be rendered
            for (int i = 0; i < _worldChunks.Chunks.Length; i++)
            {
                chunk = _worldChunks.Chunks[i];
                if (chunk.isFrustumCulled == false && chunk.State == ChunkState.DisplayInSyncWithMeshes)
                {
                    for (int j = 0; j < chunk.VisualSpriteEntities.Count; j++)
                    {
                        _spriteEntitiesToRenderNbr++;
                        _spriteEntitiesToRender[_spriteEntitiesToRenderNbr] = chunk.VisualSpriteEntities[j];
                        if (_spriteEntitiesToRenderNbr >= _spriteMaxSize) break;
                    }
                }
            }
            _spriteRenderer.SpriteEntitiesNbr = _spriteEntitiesToRenderNbr;
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
