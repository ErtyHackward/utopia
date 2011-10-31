using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Pure-server class for server area management, wraps dynamic entity
    /// </summary>
    public abstract class ServerDynamicEntity
    {

        public event EventHandler<ServerDynamicEntityMoveEventArgs> PositionChanged;

        private void OnPositionChanged(ServerDynamicEntityMoveEventArgs e)
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets or sets currently locked entity by the player
        /// </summary>
        public IEntity LockedEntity { get; set; }

        /// <summary>
        /// Gets wrapped entity
        /// </summary>
        public IDynamicEntity DynamicEntity { get; private set; }

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

        private MapArea _currentArea;

        /// <summary>
        /// Gets or sets current entity area
        /// </summary>
        public MapArea CurrentArea
        {
            get
            {
                return _currentArea;
            }
            set
            {
                if (_currentArea != value)
                {
                    if (_currentArea != null)
                    {
                        _currentArea.EntityInViewRange -= AreaEntityInViewRange;
                        _currentArea.EntityOutOfViewRange -= AreaEntityOutOfViewRange;
                    }

                    _currentArea = value;

                    if (_currentArea != null)
                    {
                        _currentArea.EntityInViewRange += AreaEntityInViewRange;
                        _currentArea.EntityOutOfViewRange += AreaEntityOutOfViewRange;
                    }
                }
            }
        }


        protected ServerDynamicEntity(IDynamicEntity entity)
        {
            DynamicEntity = entity;
            entity.PositionChanged += EntityPositionChanged;
        }

        void EntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            OnPositionChanged(new ServerDynamicEntityMoveEventArgs { ServerDynamicEntity = this, PreviousPosition = e.PreviousPosition });
        }

        /// <summary>
        /// Called when some entity goes out of view range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e)
        {

        }

        /// <summary>
        /// Called when some entity get closer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e)
        {

        }

        /// <summary>
        /// Perform dynamic update (AI logic)
        /// </summary>
        public abstract void Update(DynamicUpdateState gameTime);

        public override int GetHashCode()
        {
            return DynamicEntity.GetHashCode();
        }

        /// <summary>
        /// Perform using (tool or toolless use)
        /// </summary>
        /// <param name="entityUseMessage"></param>
        public virtual void Use(EntityUseMessage entityUseMessage)
        {
            // update entity state
            var state = new DynamicEntityState
            {
                IsPickingActive = entityUseMessage.IsBlockPicked || entityUseMessage.IsEntityPicked,
                IsEntityPicked = entityUseMessage.IsEntityPicked,
                NewBlockPosition = entityUseMessage.NewBlockPosition,
                PickedBlockPosition = entityUseMessage.PickedBlockPosition,
                PickedEntityId = entityUseMessage.PickedEntityId,
            };
            DynamicEntity.EntityState = state;
        }

        /// <summary>
        /// Perform equipment change
        /// </summary>
        /// <param name="entityEquipmentMessage"></param>
        public virtual void Equip(EntityEquipmentMessage entityEquipmentMessage)
        {
            

        }

        public virtual void ItemTransfer(ItemTransferMessage itemTransferMessage)
        {
            
        }
    }

    public class ServerDynamicEntityMoveEventArgs : EventArgs
    {
        public ServerDynamicEntity ServerDynamicEntity { get; set; }
        public Vector3D PreviousPosition { get; set; } 
    }
}
