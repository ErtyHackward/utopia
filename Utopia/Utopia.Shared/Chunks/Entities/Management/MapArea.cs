using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Management
{
    /// <summary>
    /// Threadsafe map area. Contains dynamic entities.
    /// </summary>
    public class MapArea
    {
        /// <summary>
        /// Size of each Area
        /// </summary>
        public static Location2<int> AreaSize = new Location2<int>(16 * 8, 16 * 8);

        private readonly object _syncRoot = new object();
        private readonly ConcurrentDictionary<IDynamicEntity, IDynamicEntity> _entities = new ConcurrentDictionary<IDynamicEntity, IDynamicEntity>();
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
        public event EventHandler<DynamicEntityEventArgs> EntityAdded;

        private void OnEntityAdded(DynamicEntityEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity was removed from this area
        /// </summary>
        public event EventHandler<DynamicEntityEventArgs> EntityRemoved;

        private void OnEntityRemoved(DynamicEntityEventArgs e)
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
        public event EventHandler<DynamicEntityEventArgs> EntityOutOfViewRange;

        public void OnEntityOutOfViewRange(IDynamicEntity iDynamicEntity)
        {
            var handler = EntityOutOfViewRange;
            if (handler != null) handler(this, new DynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        /// <summary>
        /// Occurs when some entity is close enough to listen to
        /// </summary>
        public event EventHandler<DynamicEntityEventArgs> EntityInViewRange;
        
        public void OnEntityInViewRange(IDynamicEntity iDynamicEntity)
        {
            var handler = EntityInViewRange;
            if (handler != null) handler(this, new DynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        #endregion

        /// <summary>
        /// Gets area top left position
        /// </summary>
        public IntVector2 Position { get; private set; }
        
        /// <summary>
        /// Bounding rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get { return _rectangle; }
            private set { _rectangle = value; }
        }

        public MapArea(IntVector2 topLeftPoint)
        {
            Position = topLeftPoint;

            Rectangle = new Rectangle(topLeftPoint.X, topLeftPoint.Y, topLeftPoint.X + AreaSize.X,
                                      topLeftPoint.Y + AreaSize.Z);
        }

        public void AddEntity(IDynamicEntity entity)
        {
            if (_entities.TryAdd(entity, entity))
            {
                // add events to retranslate
                entity.PositionChanged += EntityPositionChanged;
                entity.ViewChanged += EntityViewChanged;
                entity.Use += EntityUseHandler;

                OnEntityAdded(new DynamicEntityEventArgs {Entity = entity});
            }
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            IDynamicEntity e;
            if (_entities.TryRemove(entity, out e))
            {

                // remove events from re-translating
                entity.PositionChanged -= EntityPositionChanged;
                entity.ViewChanged -= EntityViewChanged;
                entity.Use -= EntityUseHandler;

                OnEntityRemoved(new DynamicEntityEventArgs {Entity = entity});
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

        private void EntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            // retranslate
            OnEntityMoved(e);
            
            // we need to tell area manager that entity leaves us, to put it into new area
            if (!_rectangle.Contains(e.Entity.Position.AsVector3()))
            {
                OnEntityLeave(new EntityLeaveAreaEventArgs { Entity = e.Entity, PreviousPosition = e.PreviousPosition });
                RemoveEntity(e.Entity);
            }
        }

        public IEnumerable<IDynamicEntity> Enumerate()
        {
            
            foreach (var entity in _entities)
            {
                yield return entity.Key;
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
        
        public bool ContainsEntity(IDynamicEntity iDynamicEntity)
        {
            return _entities.ContainsKey(iDynamicEntity);
        }


    }
}