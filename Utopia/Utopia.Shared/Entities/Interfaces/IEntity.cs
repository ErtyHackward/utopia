using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Base interface for entities
    /// </summary>
    public interface IEntity : IBinaryStorable
    {
        /// <summary>
        /// Pickable entity Property
        /// </summary>
        bool IsPickable { get; }

        /// <summary>
        /// Player Collision checked entity Property
        /// </summary>
        bool IsPlayerCollidable { get; }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        ushort ClassId { get; }

        /// <summary>
        /// Entity size
        /// </summary>
        Vector3 Size { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        Vector3D Position { get; set; }

        /// <summary>
        /// Gets or sets entity rotation information
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        EntityType Type { get; }

        /// <summary>
        /// Returns link to the entity
        /// </summary>
        /// <returns></returns>
        EntityLink GetLink();
    }
}