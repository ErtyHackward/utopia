using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Management;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents dynamic entity (players, robots, animals, NPC)
    /// </summary>
    public abstract class DynamicEntity : Entity, IDynamicEntity
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

        public override SharpDX.Vector3 Position
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

        /// <summary>
        /// Perform actions when getting closer to area. Entity should add all needed event handlers
        /// </summary>
        /// <param name="area"></param>
        public abstract void AddArea(MapArea area);

        /// <summary>
        /// Perform actions when area is far away, entity should remove any event hadler it has
        /// </summary>
        /// <param name="area"></param>
        public abstract void RemoveArea(MapArea area);

        /// <summary>
        /// Perform dynamic update (AI logic)
        /// </summary>
        public abstract void Update(DateTime gameTime);
    }
}
