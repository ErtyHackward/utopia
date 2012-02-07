using System;
using System.IO;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents dynamic voxel entity (players, robots, animals, NPC)
    /// </summary>
    public abstract class DynamicEntity : Entity, IDynamicEntity
    {
        public DynamicEntityState _entityState;

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
        public DynamicEntityState EntityState
        {
            get { return _entityState; }
            set { _entityState = value; }
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
        public uint DynamicId { get; set; }

        #endregion

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);
            
            ModelInstance = new VoxelModelInstance();
            ModelInstance.Load(reader);
            
            DynamicId = reader.ReadUInt32();
            DisplacementMode = (EntityDisplacementModes)reader.ReadByte();
            MoveSpeed = reader.ReadSingle();
            RotationSpeed = reader.ReadSingle();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);

            ModelInstance.Save(writer);

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
