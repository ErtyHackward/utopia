using System.Collections.Generic;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Server.Interfaces
{
    public interface IEntityStorage
    {
        void SaveDynamicEntity(IDynamicEntity entity);

        void SaveEntity(uint entityId, byte[] bytes);

        IEntity LoadEntity(uint entityId);

        uint GetMaximumId();

        byte[] LoadEntityBytes(uint p);

        void SaveState(GlobalState state);

        GlobalState LoadState();

        IEnumerable<IDynamicEntity> AllEntities();

        void RemoveEntity(uint id);
    }
}