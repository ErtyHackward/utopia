using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Entities
{
    public class EntityManager : IEntityManager, IDisposable 
    {
        #region Private variables
        // enumerate this entities to draw
        private Dictionary<uint, IDynamicEntity> _dynamicEntities = new Dictionary<uint, IDynamicEntity>();
        #endregion

        #region Public Variables/Properties
        #endregion
        public EntityManager()
        {
        }

        #region Private Methods
        #endregion

        #region Public Methods
        public void AddEntity(Shared.Chunks.Entities.Interfaces.IDynamicEntity entity)
        {
            _dynamicEntities.Add(entity.EntityId, entity);
        }

        public void RemoveEntity(Shared.Chunks.Entities.Interfaces.IDynamicEntity entity)
        {
            _dynamicEntities.Remove(entity.EntityId);
        }

        public void RemoveEntityById(uint entityId)
        {
            _dynamicEntities.Remove(entityId);
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            return _dynamicEntities[p];
        }

        public void Dispose()
        {
        }
        #endregion


    }
}
