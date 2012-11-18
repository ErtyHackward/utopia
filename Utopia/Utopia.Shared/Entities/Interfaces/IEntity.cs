using System;
using System.ComponentModel;
using System.IO;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Base interface for entities
    /// </summary>
    public interface IEntity : ICloneable
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
        /// Entity Collision Type
        /// </summary>
        Entity.EntityCollisionType CollisionType { get; set; }

        /// <summary>
        /// Y Force that will be given to anyone that is colliding with this entity, only working in "Model" collision mode
        /// </summary>
        double YForceOnSideHit { get; set; }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        ushort ClassId { get; }

        /// <summary>
        /// Gets entity BluePrint ID
        /// </summary>
        ushort BluePrintId { get; set; }

        /// <summary>
        /// Entity size
        /// </summary>
        Vector3 DefaultSize { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        Vector3D Position { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        EntityType Type { get; }

        /// <summary>
        /// This entity is mandatory for the system to run properly
        /// </summary>
        bool isSystemEntity { get; set; }

        /// <summary>
        /// Indicates that the entity must be locked to be used
        /// </summary>
        [Browsable(false)]
        bool RequiresLock { get; }

        /// <summary>
        /// Gets or sets value indicating the entity is locked
        /// This is runtime parameter that is not stored
        /// </summary>
        [Browsable(false)]
        bool Locked { get; set; }

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