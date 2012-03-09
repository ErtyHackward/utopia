using System.IO;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Base interface for entities
    /// </summary>
    public interface IEntity
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

        /// <summary>
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        void Save(BinaryWriter writer);

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        void Load(BinaryReader reader, EntityFactory factory);
    }
}