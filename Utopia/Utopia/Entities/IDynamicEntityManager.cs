using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using S33M3Engines.D3D;

namespace Utopia.Entities
{
    public interface IDynamicEntityManager : IGameComponent
    {
        void AddEntity(IDynamicEntity entity);
        void RemoveEntity(IDynamicEntity entity);
        void RemoveEntityById(uint entityId);
        IDynamicEntity GetEntityById(uint p);
    }

}
