using System;
using System.ComponentModel;
using System.Drawing.Design;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents dynamic voxel entity (players, robots, animals, NPC)
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(CharacterEntity))]
    [ProtoInclude(101, typeof(GodEntity))]
    public abstract class DynamicEntity : Entity, IDynamicEntity
    {
        public DynamicEntityState EntityState;
        private Quaternion _headRotation;

        private DynamicEntityHealthState _healthState;
        private DynamicEntityAfflictionState _afflictions;

        #region Properties

        /// <summary>
        /// Gets voxel entity model
        /// </summary>
        [Browsable(false)]
        public VoxelModelInstance ModelInstance { get; set; }

        /// <summary>
        /// Gets or sets entity state (this field should be refreshed before using the tool)
        /// </summary>
        [Browsable(false)]
        DynamicEntityState IDynamicEntity.EntityState
        {
            get { return EntityState; }
            set { EntityState = value; }
        }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        [Browsable(false)]
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
        /// Gets or sets dynamic entity id
        /// </summary>
        [ProtoMember(1)]
        [Browsable(false)]
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

        /// <summary>
        /// Gets or sets current voxel model name
        /// </summary>
        [ProtoMember(5)]
        [Editor(typeof(ModelSelector), typeof(UITypeEditor))]
        public virtual string ModelName { get; set; }

        /// <summary>
        /// Indicates if user can do any changes in the world or not
        /// </summary>
        [ProtoMember(6)]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets entity head rotation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(7)]
        public virtual Quaternion HeadRotation
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
                        head = Quaternion.Lerp(BodyRotation, head, 1f - 1.6f / offset.Angle);
                        BodyRotation = head;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets entity body rotation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(8)]
        public Quaternion BodyRotation { get; set; }

        [ProtoMember(9)]
        public Energy Health { get; set; }

        [ProtoMember(10)]
        public Energy Stamina { get; set; }

        [ProtoMember(11)]
        public Energy Oxygen { get; set; }

        [Browsable(false)]
        [ProtoMember(12)]
        public DynamicEntityHealthState HealthState
        {
            get { return _healthState; }
            set
            {
                if (_healthState == value) return;
                HealthStateChangeEventArgs eventArg = new HealthStateChangeEventArgs() { DynamicEntityId = DynamicId, NewState = value, PreviousState = _healthState };
                _healthState = value;
                OnHealthStateChanged(eventArg);
            }
        }

        [Browsable(false)]
        [ProtoMember(13)]
        public DynamicEntityAfflictionState Afflictions
        {
            get { return _afflictions; }
            set
            {
                if (_afflictions == value) return;
                AfflictionStateChangeEventArgs eventArg = new AfflictionStateChangeEventArgs() { DynamicEntityId = DynamicId, NewState = value, PreviousState = _afflictions };
                _afflictions = value;
                OnAfflictionStateChanged(eventArg);
            }
        }

        #endregion

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
        /// Calls tool use and fires use event from current entity state
        /// </summary>
        public IToolImpact ToolUse(ITool tool)
        {
            var arg = EntityUseEventArgs.FromState(this);
            arg.Tool = tool;

            if (tool != null)
                arg.Impact = tool.Use(this);
            else
            {
                arg.Impact = new ToolImpact{ Message = "Null tool" };
            }
            
            OnUse(arg);

            if (ModelInstance != null)
                ModelInstance.TryPlay("Use");
            
            return arg.Impact;
        }
        
        /// <summary>
        /// Occurs when entity changes its position
        /// </summary>
        public event EventHandler<EntityMoveEventArgs> PositionChanged;
        protected virtual void OnPositionChanged(EntityMoveEventArgs e)
        {
            if (PositionChanged != null) PositionChanged(this, e);
        }

        public event EventHandler<HealthStateChangeEventArgs> HealthStateChanged;
        protected virtual void OnHealthStateChanged(HealthStateChangeEventArgs e)
        {
            if (HealthStateChanged != null) HealthStateChanged(this, e);
        }

        public event EventHandler<AfflictionStateChangeEventArgs> AfflictionStateChanged;
        protected virtual void OnAfflictionStateChanged(AfflictionStateChangeEventArgs e)
        {
            if (AfflictionStateChanged != null) AfflictionStateChanged(this, e);
        }

        #endregion
        
        protected DynamicEntity()
        {
            HeadRotation = Quaternion.Identity;
            BodyRotation = Quaternion.Identity;
            EntityState = new DynamicEntityState();
            Health = new Energy();
            Stamina = new Energy();
            Oxygen = new Energy();
        }

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
