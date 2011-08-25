namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Represents the entity that can be used by living entity
    /// </summary>
    public interface IUsableEntity
    {
        bool Use(LivingEntity caller);
    }
}
