using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a base entity 
    /// </summary>
    [ProtoContract]
    public abstract class Entity : IEntity
    {
        private string _name = "No name";

        public enum EntityCollisionType : byte
        {
            BoundingBox,
            Model
        }

        /// <summary>
        /// Gets or sets Entity ID used in configuration to distinguish entities of the same final type
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public ushort BluePrintId { get; set; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        [Category("Entity")]
        [ProtoMember(2)]
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Entity maximum size
        /// </summary>
        [Category("Entity")]
        [ProtoMember(3)]
        public Vector3 DefaultSize { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        [Browsable(false)]
        [ProtoMember(4)]
        public virtual Vector3D Position { get; set; }

        /// <summary>
        /// Pickable entity Property
        /// </summary>
        [Category("Entity")]
        [ProtoMember(5)]
        public bool IsPickable { get; set; }

        /// <summary>
        /// Player Collision checked entity Property
        /// </summary>
        [Category("Entity")]
        [ProtoMember(6)]
        public bool IsPlayerCollidable { get; set; }

        /// <summary>
        /// Is this entity a system Entity (Mandatory for the system to run)
        /// </summary>
        [Browsable(false)]
        [ProtoMember(7)]
        public bool isSystemEntity { get; set; }

        /// <summary>
        /// Gets or sets model collision type
        /// </summary>
        [Category("Entity")]
        [ProtoMember(8)]
        public EntityCollisionType CollisionType { get; set; }

        /// <summary>
        /// TODO: finish description
        /// </summary>
        [Category("Entity")]
        [ProtoMember(9)]
        public double YForceOnSideHit { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        [ProtoMember(10)]
        [Category("Entity")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        
        [ProtoMember(11, OverwriteList=true)]
        [Category("Entity")]
        [Description("Define particules emiting behaviours")]
        public EntityParticule[] Particules { get; set; }

        /// <summary>
        /// Get or sets entity grouping category
        /// Used to group entities in the editor
        /// Can be null
        /// </summary>
        [ProtoMember(12)]
        [Category("Entity")]
        [Description("Allows to put entity in special group")]
        public string GroupName { get; set; }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        [Category("Entity")]
        public abstract ushort ClassId { get; }
        
        /// <summary>
        /// Indicates that the entity must be locked to be used
        /// </summary>
        [Browsable(false)]
        public virtual bool RequiresLock { get { return false; } }

        /// <summary>
        /// Gets or sets value indicating the entity is locked
        /// This is runtime parameter that is not stored
        /// </summary>
        [Browsable(false)]
        public virtual bool Locked { get; set; }

        /// <summary>
        /// Returns link to the entity
        /// </summary>
        /// <returns></returns>
        public abstract EntityLink GetLink();
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return _name;
        }

    }
}
