using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpDX;
using Utopia.Server.Events;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Threadsafe map area. Contains dynamic entities.
    /// </summary>
    public class MapArea
    {
        /// <summary>
        /// Size of each Area
        /// </summary>
        public static Vector2I AreaSize = new Vector2I(16 * 8, 16 * 8);

        private readonly object _syncRoot = new object();
        private readonly ConcurrentDictionary<int, ServerDynamicEntity> _entities = new ConcurrentDictionary<int, ServerDynamicEntity>();
        private Rectangle _rectangle;

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        #region Events
        
        /* 
         * Each entity may listen one or more events of its MapArea.
         * Entity should start listening at AreaEnter and remove listening at AreaLeave
         * Here should be added all required multicast events like direction change and tool using
         */

        /// <summary>
        /// Occurs when some entity performs use operation
        /// </summary>
        public event EventHandler<EntityUseEventArgs> EntityUse;

        private void OnEntityUse(EntityUseEventArgs e)
        {
            var handler = EntityUse;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when one of dynamic entities changes its view direction
        /// </summary>
        public event EventHandler<EntityViewEventArgs> EntityView;

        private void OnEntityView(EntityViewEventArgs e)
        {
            var handler = EntityView;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when one of dynamic entities moves in this area
        /// </summary>
        public event EventHandler<EntityMoveEventArgs> EntityMoved;

        private void OnEntityMoved(EntityMoveEventArgs e)
        {
            var handler = EntityMoved;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some of entity leaves the area bounds
        /// </summary>
        public event EventHandler<EntityLeaveAreaEventArgs> EntityLeave;

        private void OnEntityLeave(EntityLeaveAreaEventArgs e)
        {
            var handler = EntityLeave;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity was added to this area
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityAdded;

        private void OnEntityAdded(ServerDynamicEntityEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity was removed from this area
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityRemoved;

        private void OnEntityRemoved(ServerDynamicEntityEventArgs e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when one or more blocks in any of chunks in that area was chanded
        /// </summary>
        public event EventHandler<BlocksChangedEventArgs> BlocksChanged;

        public void OnBlocksChanged(BlocksChangedEventArgs e)
        {
            var handler = BlocksChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity get far away
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityOutOfViewRange;

        public void OnEntityOutOfViewRange(ServerDynamicEntity iDynamicEntity)
        {
            var handler = EntityOutOfViewRange;
            if (handler != null) handler(this, new ServerDynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        /// <summary>
        /// Occurs when some entity is close enough to listen to
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityInViewRange;

        public void OnEntityInViewRange(ServerDynamicEntity iDynamicEntity)
        {
            var handler = EntityInViewRange;
            if (handler != null) handler(this, new ServerDynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        /// <summary>
        /// Occurs when some entity at this area changes its model
        /// </summary>
        public event EventHandler<AreaVoxelModelEventArgs> EntityModelChanged;

        private void OnEntityModelChanged(AreaVoxelModelEventArgs e)
        {
            var handler = EntityModelChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity at this area changes its equipment
        /// </summary>
        public event EventHandler<CharacterEquipmentEventArgs> EntityEquipment;

        private void OnEntityEquipment(CharacterEquipmentEventArgs e)
        {
            var handler = EntityEquipment;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity appears in one of area chunks
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> StaticEntityAdded;

        public void OnStaticEntityAdded(EntityCollectionEventArgs e)
        {
            var handler = StaticEntityAdded;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when some entity removed from one of area chunks
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> StaticEntityRemoved;

        public void OnStaticEntityRemoved(EntityCollectionEventArgs e)
        {
            var handler = StaticEntityRemoved;
            if (handler != null) handler(this, e);
        }

        #endregion

        /// <summary>
        /// Gets area top left position
        /// </summary>
        public Vector2I Position { get; private set; }
        
        /// <summary>
        /// Bounding rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get { return _rectangle; }
            private set { _rectangle = value; }
        }

        public MapArea(Vector2I topLeftPoint)
        {
            Position = topLeftPoint;

            Rectangle = new Rectangle(topLeftPoint.X, topLeftPoint.Y, topLeftPoint.X + AreaSize.X,
                                      topLeftPoint.Y + AreaSize.Y);
        }

        public void AddEntity(ServerDynamicEntity entity)
        {
            if (_entities.TryAdd(entity.GetHashCode(), entity))
            {
                // add events to retranslate
                entity.PositionChanged += EntityPositionChanged;
                entity.DynamicEntity.ViewChanged += EntityViewChanged;
                entity.DynamicEntity.Use += EntityUseHandler;
                entity.DynamicEntity.VoxelModelChanged += EntityVoxelModelChanged;
                
                CharacterEntity charEntity;
                if ((charEntity = entity.DynamicEntity as CharacterEntity) != null)
                {
                    charEntity.Equipment.ItemEquipped += EquipmentItemEquipped;
                }
                
                OnEntityAdded(new ServerDynamicEntityEventArgs {Entity = entity});
            }
        }

        void EquipmentItemEquipped(object sender, CharacterEquipmentEventArgs e)
        {
            OnEntityEquipment(e);
        }

        public void RemoveEntity(int entityId)
        {
            ServerDynamicEntity e;
            if (_entities.TryRemove(entityId, out e))
            {
                // remove events from re-translating
                e.PositionChanged -= EntityPositionChanged;
                e.DynamicEntity.ViewChanged -= EntityViewChanged;
                e.DynamicEntity.Use -= EntityUseHandler;
                e.DynamicEntity.VoxelModelChanged -= EntityVoxelModelChanged;

                CharacterEntity charEntity;
                if ((charEntity = e.DynamicEntity as CharacterEntity) != null)
                {
                    charEntity.Equipment.ItemEquipped -= EquipmentItemEquipped;
                }

                OnEntityRemoved(new ServerDynamicEntityEventArgs { Entity = e });
            }
        }

        void EntityUseHandler(object sender, EntityUseEventArgs e)
        {
            // retranslate
            OnEntityUse(e);
        }

        void EntityViewChanged(object sender, EntityViewEventArgs e)
        {
            // retranslate
            OnEntityView(e);
        }

        void EntityVoxelModelChanged(object sender, VoxelModelEventArgs e)
        {
            // retranslate
            //var ea = new AreaVoxelModelEventArgs{ Entity = (VoxelEntity)sender, Message = new 

           

            //OnEntityModelChanged(e);
        }

        private void EntityPositionChanged(object sender, ServerDynamicEntityMoveEventArgs e)
        {
            // retranslate
            OnEntityMoved(new EntityMoveEventArgs { Entity = e.ServerDynamicEntity.DynamicEntity, PreviousPosition = e.PreviousPosition });
            
            // we need to tell area manager that entity leaves us, to put it into new area
            if (!_rectangle.Contains(e.ServerDynamicEntity.DynamicEntity.Position.AsVector3()))
            {
                OnEntityLeave(new EntityLeaveAreaEventArgs { Entity = e.ServerDynamicEntity, PreviousPosition = e.PreviousPosition });

                RemoveEntity(e.ServerDynamicEntity.GetHashCode());
            }
        }

        public IEnumerable<ServerDynamicEntity> Enumerate()
        {
            foreach (var entity in _entities)
            {
                yield return entity.Value;
            }
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Position.Equals(obj);
        }
        
        public bool ContainsEntity(uint entityId)
        {
            return _entities.ContainsKey((int)entityId);
        }


    }

    public class ServerDynamicEntityEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
    }
}