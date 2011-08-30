using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Management;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Manages the dynamic entites
    /// </summary>
    public class EntityManager
    {
        private readonly ConcurrentDictionary<IntVector2, MapArea> _areas = new ConcurrentDictionary<IntVector2, MapArea>();
        private readonly HashSet<IDynamicEntity> _allEntities = new HashSet<IDynamicEntity>();

#if DEBUG
        public volatile int entityAreaChangesCount;
#endif
        #region Events

        /// <summary>
        /// Occurs when new entity was added somewhere on map
        /// </summary>
        public event EventHandler<EntityManagerEventArgs> EntityAdded;

        private void OnEntityAdded(EntityManagerEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when entity was removed from map
        /// </summary>
        public event EventHandler<EntityManagerEventArgs> EntityRemoved;

        private void OnEntityRemoved(EntityManagerEventArgs e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        #endregion

        private void GetAreas(Vector3 position, out MapArea one, out MapArea two)
        {
            var pos1 = new IntVector2((int)Math.Floor((double)position.X / (MapArea.AreaSize.X)) * MapArea.AreaSize.X, (int)Math.Floor((double)position.Z / (MapArea.AreaSize.Z)) * MapArea.AreaSize.Z);

            var pos2 = new IntVector2(
                (int)Math.Floor(((double)position.X - MapArea.AreaSize.X / 2) / (MapArea.AreaSize.X / 2)) * MapArea.AreaSize.X + MapArea.AreaSize.X / 2,
                (int)Math.Floor(((double)position.Z - MapArea.AreaSize.Z / 2) / (MapArea.AreaSize.Z / 2)) * MapArea.AreaSize.Z + MapArea.AreaSize.Z / 2
                );
            
            if (_areas.ContainsKey(pos1))
            {
                one = _areas[pos1];
            }
            else
            {
                var area = new MapArea(pos1);
                ListenArea(area);
                _areas.TryAdd(pos1, area);
                one = area;
            }

            if (_areas.ContainsKey(pos2))
            {
                two = _areas[pos2];
            }
            else
            {
                var area = new MapArea(pos2);
                ListenArea(area);
                _areas.TryAdd(pos2, area);
                two = area;
            }
        }

        private void ListenArea(MapArea area)
        {
            // listening area leave to move entity to new area
            area.EntityLeave += AreaEntityLeave;
        }

        private void AreaEntityLeave(object sender, EntityLeaveAreaEventArgs e)
        {
#if DEBUG
            // temp counter
            Interlocked.Increment(ref entityAreaChangesCount);
#endif
            // move entity to new areas
            PutInAreas(e.Entity);
        }

        private void PutInAreas(IDynamicEntity entity)
        {
            // current areas
            MapArea one, two;
            GetAreas(entity.Position, out one, out two);

            // changing old Area to new
            if (!one.ContainsEntity(entity))
                one.AddEntity(entity);

            if (!two.ContainsEntity(entity))
                two.AddEntity(entity);
        }

        public void AddEntity(IDynamicEntity entity)
        {
            lock (_allEntities)
            {
                if (_allEntities.Contains(entity))
                    throw new InvalidOperationException("Such entity is already in manager");

                _allEntities.Add(entity);
            }

            PutInAreas(entity);

            OnEntityAdded(new EntityManagerEventArgs { Entity = entity });
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            bool removed;
            lock (_allEntities)
            {
                removed = _allEntities.Remove(entity);
            }

            if (!removed) return;

            // remove entity from areas
            MapArea one, two;
            GetAreas(entity.Position, out one, out two);
            one.RemoveEntity(entity);
            two.RemoveEntity(entity);

            OnEntityRemoved(new EntityManagerEventArgs { Entity = entity });
        }

        public void Update(DateTime gameTime)
        {
            Parallel.ForEach(_areas, a => UpdateArea(a.Value, gameTime));
        }

        private void UpdateArea(MapArea area, DateTime gameTime)
        {
            foreach (var entity in area.Enumerate())
            {
                entity.Update(gameTime);    
            }
        }
    }
}
