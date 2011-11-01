using Utopia.Shared.Chunks;

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

        /// <summary>
        /// Gets or sets current parent chunk
        /// </summary>
        AbstractChunk ParentChunk { get; set; }
    }
}
