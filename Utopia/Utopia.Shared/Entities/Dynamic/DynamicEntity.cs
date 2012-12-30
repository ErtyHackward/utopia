using System;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents dynamic voxel entity (players, robots, animals, NPC)
    /// </summary>
    [ProtoContract]
    public abstract class DynamicEntity : Entity, IDynamicEntity
    {
        public DynamicEntityState EntityState;
        private Quaternion _headRotation;

        #region Events
        /// <summary>
        /// Occurs when entity changes its view direction
        /// </summary>
        public event EventHandler<EntityViewEventArgs> ViewChanged;

        protected void OnViewChanged(EntityViewEventArgs e)
        {
            var handler = ViewChanged;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when entity performs "use" operation
        /// </summary>
        public event EventHandler<EntityUseEventArgs> Use;

        protected virtual void OnUse(EntityUseEventArgs e)
        {
            var handler = Use;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when entity changes its position
        /// </summary>
        public event EventHandler<EntityMoveEventArgs> PositionChanged;

        protected virtual void OnPositionChanged(EntityMoveEventArgs e)
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        #endregion
        
        protected DynamicEntity()
        {
            HeadRotation = Quaternion.Identity;
            BodyRotation = Quaternion.Identity;
        }

        #region Properties

        /// <summary>
        /// Gets voxel entity model
        /// </summary>
        public VoxelModelInstance ModelInstance { get; set; }
        
        /// <summary>
        /// Gets or sets entity state (this field should be refreshed before using the tool)
        /// </summary>
        DynamicEntityState IDynamicEntity.EntityState
        {
            get { return EntityState; }
            set { EntityState = value; }
        }
        
        /// <summary>
        /// Gets or sets current voxel model name
        /// </summary>
        public virtual string ModelName { get; set; } 

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public override Vector3D Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                if (base.Position != value)
                {
                    var prev = base.Position;
                    base.Position = value;
                    OnPositionChanged(new EntityMoveEventArgs { Entity = this, PreviousPosition = prev });
                }
            }
        }

        /// <summary>
        /// Gets or sets entity head rotation
        /// </summary>
        public Quaternion HeadRotation 
        {
            get 
            {
                return _headRotation;
            }
            set
            {
                if (_headRotation != value)
                {
                    _headRotation = value;
                    OnViewChanged(new EntityViewEventArgs { Entity = this });
                    
                    // we want to change body rotation
                    // leave only y-axis rotation for the body
                    
                    var head = _headRotation;

                    head.X = 0;
                    head.Z = 0;
                    head.Normalize();

                    // calculate the difference between head and body rotation
                    var headInvert = head;
                    headInvert.Invert();
                    var offset = BodyRotation * headInvert;
                    
                    // allow to rotate the head up to 160 degrees
                    if (offset.Angle > 1.6f)
                    {
                        // remove excess rotation
                        head = Quaternion.Lerp(BodyRotation, head, 1f -  1.6f / offset.Angle);
                        BodyRotation = head;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets entity body rotation
        /// </summary>
        public Quaternion BodyRotation { get; set; }

        /// <summary>
        /// Gets or sets dynamic entity id
        /// </summary>
        [ProtoMember(1)]
        public uint DynamicId { get; set; }

        /// <summary>
        /// The displacement mode use by this entity (Walk, swim, fly, ...)
        /// </summary>
        [ProtoMember(2)]
        public EntityDisplacementModes DisplacementMode { get; set; }

        /// <summary>
        /// The speed at wich the dynamic entity can walk
        /// </summary>
        [ProtoMember(3)]
        public float MoveSpeed { get; set; }

        /// <summary>
        /// The speed at wich the dynamic is doing move rotation
        /// </summary>
        [ProtoMember(4)]
        public float RotationSpeed { get; set; }

        #endregion

        public override int GetHashCode()
        {
            return (int)DynamicId;
        }

        /// <summary>
        /// Returns link to the entity
        /// </summary>
        /// <returns></returns>
        public override EntityLink GetLink()
        {
            return new EntityLink(DynamicId);
        }
    }
}
