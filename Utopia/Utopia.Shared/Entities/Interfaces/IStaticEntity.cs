using ProtoBuf;
using SharpDX;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an entity that can be stored in chunk entiteis
    /// </summary>
    [ProtoContract]
    public interface IStaticEntity : IEntity
    {
        /// <summary>
        /// Gets or sets static entity id. This id is unique only in current container. Invalid without Container property set
        /// </summary>
        uint StaticId { get; set; }

        /// <summary>
        /// Gets or sets entity world rotation
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets or sets current parent container
        /// </summary>
        IStaticContainer Container { get; set; }

        /// <summary>
        /// Is the item destroyed on world removed
        /// </summary>
        bool IsDestroyedOnWorldRemove { get; set; }

        /// <summary>
        /// Is the item destroyed on char death?
        /// </summary>
        bool IsDestroyedOnDeath { get; set; }

        /// <summary>
        /// Will be called before the entity is destroyed from world, without going into inventory
        /// </summary>
        void BeforeDestruction(IDynamicEntity destructor);
    }
}
