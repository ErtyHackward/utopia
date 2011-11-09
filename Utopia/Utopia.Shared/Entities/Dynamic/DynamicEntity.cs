using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents dynamic voxel entity (players, robots, animals, NPC)
    /// </summary>
    public abstract class DynamicEntity : Entity, IDynamicEntity
    {
        private readonly VoxelModel _model;

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
        
        /// <summary>
        /// Gets voxel entity model
        /// </summary>
        public VoxelModel Model
        {
            get { return _model; }
        }

        public void CommitModel()
        {
            OnVoxelModelChanged(new VoxelModelEventArgs { Model = _model });
        }

        protected DynamicEntity()
        {
            _model = new VoxelModel();   
        }

        #region Properties

        public DynamicEntityState _entityState;

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

        /// <summary>
        /// Gets or sets dynamic entity id
        /// </summary>
        public uint DynamicId { get; set; }

        #endregion
        
        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);

            Model.Load(reader);

            DisplacementMode = (EntityDisplacementModes)reader.ReadByte();
            MoveSpeed = reader.ReadSingle();
            RotationSpeed = reader.ReadSingle();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            Model.Save(writer);

            writer.Write((byte)DisplacementMode);
            writer.Write(MoveSpeed);
            writer.Write(RotationSpeed);
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
