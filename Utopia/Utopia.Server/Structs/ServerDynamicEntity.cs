using System;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Pure-server class for server area management, wraps dynamic entity
    /// </summary>
    public abstract class ServerDynamicEntity
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
        public IDynamicEntity DynamicEntity
        {
            get { return _dynamicEntity; }
            set { 
                if (_dynamicEntity == value)
                    return;

                if (_dynamicEntity != null)
                {
                    _dynamicEntity.PositionChanged -= EntityPositionChanged;
                }

                _dynamicEntity = value;

                if (_dynamicEntity != null)
                {
                    _dynamicEntity.PositionChanged += EntityPositionChanged;
                }
            }
        }

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
        private IDynamicEntity _dynamicEntity;

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
            DynamicEntity.Controller = this;
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
        public virtual void Update(DynamicUpdateState gameTime) { }

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
            DynamicEntity.EntityState = entityUseMessage.State;

            logger.Warn("SRV use EP:" + DynamicEntity.EntityState.IsEntityPicked + " BP:" + DynamicEntity.EntityState.IsBlockPicked);
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
