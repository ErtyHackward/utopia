using System.Collections.Generic;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Describes a container-like entity
    /// </summary>
    public interface IContainerEntity
    {
        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        int Capacity { get; }

        // todo: change items to provide entity and entities count
        /// <summary>
        /// Gets or sets a list of contained items
        /// </summary>
        List<Entity> Items { get; set; }
    }
}
