using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents dynamic entity (players, robots, animals, NPC)
    /// </summary>
    public abstract class DynamicEntity : VoxelEntity, IDynamicEntity
    {
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

        #endregion

        protected DynamicEntity()
        {
            
        }

        #region Properties

        /// <summary>
        /// Gets or sets entity state (this field should be refreshed before using the tool)
        /// </summary>
        public DynamicEntityState EntityState { get; set; }

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

        
        public override DVector3 Position
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

        public override SharpDX.Quaternion Rotation
        {
            get
            {
                return base.Rotation;
            }
            set
            {
                if (base.Rotation != value)
                {
                    base.Rotation = value;
                    OnViewChanged(new EntityViewEventArgs { Entity = this });
                }
            }
        }

        #endregion
        
        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);

            DisplacementMode = (EntityDisplacementModes)reader.ReadByte();
            MoveSpeed = reader.ReadSingle();
            RotationSpeed = reader.ReadSingle();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write((byte)DisplacementMode);
            writer.Write(MoveSpeed);
            writer.Write(RotationSpeed);
        }
    }
}
