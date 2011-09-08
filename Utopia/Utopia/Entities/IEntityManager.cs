using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Entities
{
    public interface IEntityManager
    {
        void AddEntity(IDynamicEntity entity);
        void RemoveEntity(IDynamicEntity entity);
        void RemoveEntityById(uint entityId);
        IDynamicEntity GetEntityById(uint p);
    }

}
