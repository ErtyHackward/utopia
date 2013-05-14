﻿using System;
using System.Linq;
using Utopia.Shared.Entities;
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
        private readonly Server _server;
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

        public Server Server { get { return _server; } }
        
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

                    if (_dynamicEntity.FactionId != 0)
                    {
                        var faction = _server.GlobalStateManager.GlobalState.Factions.First(f => f.FactionId == _dynamicEntity.FactionId);

                        if (!faction.MembersIds.Contains(_dynamicEntity.DynamicId))
                            faction.MembersIds.Add(_dynamicEntity.DynamicId);
                    }
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
        
        protected ServerDynamicEntity(Server server, IDynamicEntity entity)
        {
            _server = server;
            DynamicEntity = entity;
            DynamicEntity.Controller = this;
        }

        public void SetFaction(Faction faction)
        {
            if (_dynamicEntity.FactionId != 0)
            {
                throw new InvalidOperationException("already has a faction");
            }

            if (_dynamicEntity.FactionId != faction.FactionId)
            {
                _dynamicEntity.FactionId = faction.FactionId;
                faction.MembersIds.Add(_dynamicEntity.DynamicId);
            }
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
