using System;
using System.Diagnostics;
using System.IO;
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

        protected void OnUse(EntityUseEventArgs e)
        {
            var handler = Use;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when entity changes its position
        /// </summary>
        public event EventHandler<EntityMoveEventArgs> PositionChanged;

        protected void OnPositionChanged(EntityMoveEventArgs e)
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when entity voxel model was changed
        /// </summary>
        public event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        protected void OnVoxelModelChanged(VoxelModelEventArgs e)
        {
            var handler = VoxelModelChanged;
            if (handler != null) handler(this, e);
        }

        #endregion
        
        protected DynamicEntity()
        {

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
        /// The speed at wich the dynamic entity can walk
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// The speed at wich the dynamic is doing move rotation
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        /// The displacement mode use by this entity (Walk, swim, fly, ...)
        /// </summary>
        public EntityDisplacementModes DisplacementMode { get; set; }
        
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
                }
            }
        }

        /// <summary>
        /// Gets or sets dynamic entity id
        /// </summary>
        public uint DynamicId { get; set; }

        #endregion

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);

            var containsModel = reader.ReadBoolean();

            if (containsModel)
            {
                ModelInstance = new VoxelModelInstance();
                ModelInstance.Load(reader);
            }

            DynamicId = reader.ReadUInt32();
            DisplacementMode = (EntityDisplacementModes)reader.ReadByte();
            MoveSpeed = reader.ReadSingle();
            RotationSpeed = reader.ReadSingle();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            if (ModelInstance != null)
            {
                writer.Write(true);
                ModelInstance.Save(writer);
            }
            else writer.Write(false);

            writer.Write(DynamicId);
            writer.Write((byte)DisplacementMode);
            writer.Write(MoveSpeed);
            writer.Write(RotationSpeed);
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
