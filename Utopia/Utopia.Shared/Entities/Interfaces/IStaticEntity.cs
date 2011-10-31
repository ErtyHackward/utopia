namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an entity that can be stored in chunk entiteis
    /// </summary>
    public interface IStaticEntity : IEntity
    {
        /// <summary>
        /// Gets or sets static entity id
        /// </summary>
        uint StaticId { get; set; }
    }
}
