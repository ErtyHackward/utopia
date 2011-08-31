namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Represents the entity that can be used by other entity
    /// </summary>
    public interface IUsableEntity
    {
        bool Use(IDynamicEntity caller);
    }
}
