using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Server.Managers
{
    public interface IEntityStorage
    {
        void SaveEntity(IEntity entity);
        void SaveEntity(uint entityId, byte[] bytes);
        IEntity LoadEntity(uint entityId);
        uint GetMaximumId();

        byte[] LoadEntityBytes(uint p);
    }
}