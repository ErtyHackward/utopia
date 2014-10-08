using System;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Pure-server class for server area management, wraps dynamic entity
    /// </summary>
    public abstract class ServerDynamicEntity
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected readonly ServerCore _server;
        
        private Faction _faction;
        private MapArea _currentArea;
        private IDynamicEntity _dynamicEntity;
        private DateTime _lastSaved;
        private bool _needSave;
        
        public event EventHandler<ServerDynamicEntityMoveEventArgs> PositionChanged;

        private void OnPositionChanged(ServerDynamicEntityMoveEventArgs e)
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        public bool NeedSave {
            get { return _needSave && _lastSaved.AddSeconds(10) < DateTime.Now; }
        }

        /// <summary>
        /// Gets or sets currently locked entity by the player
        /// </summary>
        public IEntity LockedEntity { get; set; }

        public ServerCore Server { get { return _server; } }

        public Faction Faction 
        { 
            get { return _faction; }
            set 
            {
                var faction = value;

                if (_dynamicEntity.FactionId != 0)
                {
                    throw new InvalidOperationException("already has a faction");
                }

                if (_dynamicEntity.FactionId != faction.FactionId)
                {
                    _dynamicEntity.FactionId = faction.FactionId;
                    if (!faction.MembersIds.Contains(_dynamicEntity.DynamicId))
                        faction.MembersIds.Add(_dynamicEntity.DynamicId);

                    _faction = faction;
                }
            }
        }
        
        /// <summary>
        /// Gets wrapped entity
        /// </summary>
        public virtual IDynamicEntity DynamicEntity
        {
            get { return _dynamicEntity; }
            set { 
                if (_dynamicEntity == value)
                    return;

                if (_dynamicEntity != null)
                {
                    _dynamicEntity.PositionChanged -= EntityPositionChanged;

                    var characterEntity = _dynamicEntity as CharacterEntity;

                    if (characterEntity != null)
                    {
                        characterEntity.InventoryUpdated -= characterEntity_InventoryUpdated;
                    }
                }

                _dynamicEntity = value;

                if (_dynamicEntity != null)
                {
                    _dynamicEntity.PositionChanged += EntityPositionChanged;

                    var characterEntity = _dynamicEntity as CharacterEntity;

                    if (characterEntity != null)
                    {
                        characterEntity.InventoryUpdated += characterEntity_InventoryUpdated;
                    }

                    if (_dynamicEntity.FactionId != 0)
                    {
                        var faction = _server.GlobalStateManager.GlobalState.Factions.First(f => f.FactionId == _dynamicEntity.FactionId);

                        if (!faction.MembersIds.Contains(_dynamicEntity.DynamicId))
                            faction.MembersIds.Add(_dynamicEntity.DynamicId);

                        _faction = faction;
                    }
                }
            }
        }

        void characterEntity_InventoryUpdated(object sender, EventArgs e)
        {
            _needSave = true;
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
        
        protected ServerDynamicEntity(ServerCore server, IDynamicEntity entity)
        {
            _server = server;
            DynamicEntity = entity;
            DynamicEntity.Controller = this;
        }

        void EntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            OnPositionChanged(new ServerDynamicEntityMoveEventArgs { 
                ServerDynamicEntity = this, 
                PreviousPosition = e.PreviousPosition 
            });
        }

        /// <summary>
        /// Called when some entity goes out of view range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e) { }

        /// <summary>
        /// Called when some entity get closer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e) { }

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
        }

        /// <summary>
        /// Perform equipment change
        /// </summary>
        /// <param name="entityEquipmentMessage"></param>
        public virtual void Equip(EntityEquipmentMessage entityEquipmentMessage) { }

        public virtual void ItemTransfer(ItemTransferMessage itemTransferMessage) { }

        public void Save()
        {
            _server.EntityStorage.SaveDynamicEntity(_dynamicEntity);
            _needSave = false;
            _lastSaved = DateTime.Now;
        }

        /// <summary>
        /// Retranslates message to the current area
        /// </summary>
        /// <param name="message"></param>
        public virtual void RetranslateMessage(IBinaryMessage message)
        {
            CurrentArea.OnCustomMessage(DynamicEntity.DynamicId, message);
        }
    }

    public class ServerDynamicEntityMoveEventArgs : EventArgs
    {
        public ServerDynamicEntity ServerDynamicEntity { get; set; }
        public Vector3D PreviousPosition { get; set; } 
    }
}
