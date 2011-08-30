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
        public static Location2<int> AreaSize = new Location2<int>(16 * 35, 16 * 35);

        private readonly object _syncRoot = new object();
        private readonly ConcurrentDictionary<IDynamicEntity, IDynamicEntity> _entities = new ConcurrentDictionary<IDynamicEntity, IDynamicEntity>();
        private Rectangle _rectangle;

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        #region Events
        
        /* Each entity may listen one or more events of its MapArea.
         * Entity should start listening at AreaEnter and remove listening at AreaLeave
         * Here should be added all required multicast events like direction change and tool using
         */

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
            _entities.TryAdd(entity,entity);

            // add events to retranslate
            entity.PositionChanged += EntityPositionChanged;

            // tell entity that it should listen new area if it want
            entity.AreaEnter(this);
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            IDynamicEntity e;
            _entities.TryRemove(entity,out e);

            //remove events from translating
            entity.PositionChanged -= EntityPositionChanged;

            // tell the entity that it should not more listen this area
            entity.AreaLeave(this);
        }

        private void EntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            OnEntityMoved(e);
            
            // we need to tell area manager that entity leaves us, to put it into new area
            if (!_rectangle.Contains(e.Entity.Position))
            {
                RemoveEntity(e.Entity);
                OnEntityLeave(new EntityLeaveAreaEventArgs {Entity = e.Entity});
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