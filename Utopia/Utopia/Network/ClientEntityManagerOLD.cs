using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Network
{
    public interface IClientEntityManager
    {
        void AddEntity(IDynamicEntity entity);
        void RemoveEntity(IDynamicEntity entity);
        void RemoveEntityById(uint entityId);
        IDynamicEntity GetEntityById(uint p);
    }

    /// <summary>
    /// This is just example of client entity manager
    /// </summary>
    public class ClientEntityManagerOLD : IClientEntityManager
    {
        // enumerate this entities to draw
        Dictionary<uint, IDynamicEntity> _dynamicEntities = new Dictionary<uint,IDynamicEntity>();

        public void AddEntity(IDynamicEntity entity)
        {
            _dynamicEntities.Add(entity.EntityId, entity);
        }

        public void RemoveEntity(IDynamicEntity entity)
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
    }
}
