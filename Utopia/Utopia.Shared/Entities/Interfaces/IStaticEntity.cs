﻿namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an entity that can be stored in chunk entiteis
    /// </summary>
    public interface IStaticEntity : IEntity
    {
        /// <summary>
        /// Gets or sets static entity id. This id is unique only in current container. Invalid without Container property set
        /// </summary>
        uint StaticId { get; set; }

        /// <summary>
        /// Gets or sets current parent container
        /// </summary>
        IStaticContainer Container { get; set; }
    }
}
