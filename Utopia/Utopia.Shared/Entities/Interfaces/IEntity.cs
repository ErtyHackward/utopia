using System;
using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using System.Collections.Generic;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Base interface for entities
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(Entity))]
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
        /// Gets entity BluePrint ID
        /// </summary>
        ushort BluePrintId { get; set; }

        /// <summary>
        /// Faction of the entity (0 if not applicable)
        /// </summary>
        uint FactionId { get; set; }

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
        /// This entity is mandatory for the system to run properly
        /// </summary>
        bool IsSystemEntity { get; set; }

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

        StaticEntityParticule[] Particules { get; set; }

        /// <summary>
        /// Gets an optional entity controller
        /// Controller is a class that provides gameplay specific logic
        /// </summary>
        object Controller { get; set; }

        float SlidingValue { get; set; }
        float Friction { get; set; }
    }
}