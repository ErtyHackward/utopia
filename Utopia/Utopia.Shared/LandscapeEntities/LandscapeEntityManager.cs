using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeEntityManager
    {
        public class EntityChunkMeshCollection
        {
            public List<LandscapeEntityChunkMesh> Collection = new List<LandscapeEntityChunkMesh>();
            public bool isProcessed = false;
        }

        #region Private Variables
        private Dictionary<Vector2I, EntityChunkMeshCollection> _pendingLandscapeEntities;
        private object _dicoSync = new object();
        private Stack<EntityChunkMeshCollection> _newEntities;
        #endregion

        #region Public Properties
        #endregion

        public LandscapeEntityManager()
        {
            _pendingLandscapeEntities = new Dictionary<Vector2I, EntityChunkMeshCollection>();
            _newEntities = new Stack<EntityChunkMeshCollection>();
        }

        #region Public Methods
        public void Add(LandscapeEntityChunkMesh landscapeEntity)
        {
            lock (_dicoSync)
            {
                EntityChunkMeshCollection entityChunkList;
                if (_pendingLandscapeEntities.TryGetValue(landscapeEntity.ChunkLocation, out entityChunkList) == false)
                {
                    entityChunkList = new EntityChunkMeshCollection();
                    _pendingLandscapeEntities.Add(landscapeEntity.ChunkLocation, entityChunkList);
                    _newEntities.Push(entityChunkList);
                }
                entityChunkList.Collection.Add(landscapeEntity);
            }
        }

        public EntityChunkMeshCollection Get(Vector2I chunkLocation)
        {
            EntityChunkMeshCollection result;

            lock (_dicoSync)
            {
                if (_pendingLandscapeEntities.TryGetValue(chunkLocation, out result))
                {
                    _pendingLandscapeEntities.Remove(chunkLocation);
                    result.isProcessed = true;
                }
            }
            return result;
        }

        List<LandscapeEntityChunkMesh> _newList = new List<LandscapeEntityChunkMesh>();
        public List<LandscapeEntityChunkMesh> GetNew()
        {
            _newList.Clear();
            if (_newEntities.Count == 0) return _newList;
            lock (_dicoSync)
            {
                while (_newEntities.Count > 0)
                {
                    var chunkEntities = _newEntities.Pop();
                    if (chunkEntities.isProcessed == false)
                    {
                        foreach (var entity in chunkEntities.Collection)
                        {
                            _newList.Add(entity);
                        }
                    }
                }
            }
            return _newList;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
