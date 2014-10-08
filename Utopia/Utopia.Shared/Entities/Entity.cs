using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a base entity 
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(DynamicEntity))]
    [ProtoInclude(101, typeof(StaticEntity))]
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
        [ReadOnly(true)]
        [Category("Entity")]
        [ProtoMember(1)]
        public ushort BluePrintId { get; set; }

        /// <summary>
        /// Entity maximum size
        /// </summary>
        [Category("Appearance")]
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
        [Category("Gameplay")]
        [Description("Indicates if the entity is pickable by the hand. Otherwise you need the Extractor tool")]
        [ProtoMember(5)]
        public bool IsPickable { get; set; }

        /// <summary>
        /// Player Collision checked entity Property
        /// </summary>
        [Category("Gameplay")]
        [ProtoMember(6)]
        public bool IsPlayerCollidable { get; set; }

        /// <summary>
        /// Is this entity a system Entity (Mandatory for the system to run)
        /// </summary>
        [Browsable(false)]
        [ProtoMember(7)]
        public bool IsSystemEntity { get; set; }

        /// <summary>
        /// Gets or sets model collision type
        /// </summary>
        [Category("Physics")]
        [ProtoMember(8)]
        public EntityCollisionType CollisionType { get; set; }

        /// <summary>
        /// TODO: finish description
        /// </summary>
        [Category("Physics")]
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
        [Category("Appearance")]
        [Description("Define particules emiting behaviours")]
        public StaticEntityParticule[] Particules { get; set; }

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
        /// Faction of the entity (0 if not applicable)
        /// </summary>
        [ProtoMember(13)]
        [Browsable(false)]
        public uint FactionId { get; set; }

        //[ProtoMember(14)] ==> Not use it for compatibility reason.

        [Description("Low friction value will make the move on it easier = faster")]
        [Category("Physics")]
        [ProtoMember(15)]
        public float Friction { get; set; }

        [Description("When stop moving on the block, will the player continue to move")]
        [Category("Physics")]
        [ProtoMember(16)]
        public float SlidingValue { get; set; }

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
        /// Gets an optional entity controller
        /// Controller is a class that provides gameplay specific logic
        /// </summary>
        [Browsable(false)]
        public object Controller { get; set; }

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

        /// <summary>
        /// Late entity initialization after the factory creation
        /// </summary>
        internal virtual void FactoryInitialize()
        {

        }

    }
}
