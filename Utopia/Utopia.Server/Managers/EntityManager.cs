using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private MapArea GetArea(Vector3 position)
        {
            var pos = new IntVector2((int)Math.Floor((double)position.X / (MapArea.AreaSize.X)) * MapArea.AreaSize.X, (int)Math.Floor((double)position.Z / (MapArea.AreaSize.Z)) * MapArea.AreaSize.Z);

            MapArea area;            
            if (_areas.ContainsKey(pos))
            {
                area = _areas[pos];
            }
            else
            {
                area = new MapArea(pos);
                area = _areas.GetOrAdd(pos, area);
                ListenArea(area);
            }

            return area;
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
            // we need to tell entity which areas are far away and not need to listen anymore

            // erty: I'm not happy with next code, if you know how make it better, you are welcome!

            double tooFarAway = MapArea.AreaSize.X + MapArea.AreaSize.Z;

            var currentArea = GetArea(new Vector3(e.Entity.Position.X, 0,
                                                  e.Entity.Position.Z));

            var previousArea = GetArea(new Vector3(e.PreviousPosition.X, 0,
                                                  e.PreviousPosition.Z));

            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var prev = GetArea(new Vector3(e.PreviousPosition.X + x*MapArea.AreaSize.X, 0,
                                                   e.PreviousPosition.Z + z*MapArea.AreaSize.Z));
                    
                    if(IntVector2.DistanceSquared(currentArea.Position,prev.Position) > tooFarAway )
                    {
                        // area is too far away, stop listening it
                        e.Entity.RemoveArea(prev);
                    }
                    
                    var now = GetArea(new Vector3(e.Entity.Position.X + x*MapArea.AreaSize.X, 0,
                                                  e.Entity.Position.Z + z*MapArea.AreaSize.Z));

                    if(IntVector2.DistanceSquared(previousArea.Position, now.Position) > tooFarAway)
                    {
                        // area is too far away from previous center, listen it
                        e.Entity.AddArea(now);
                    }
                }
            }

        }

        public void AddEntity(IDynamicEntity entity)
        {
            lock (_allEntities)
            {
                if (_allEntities.Contains(entity))
                    throw new InvalidOperationException("Such entity is already in manager");

                _allEntities.Add(entity);
            }

            // listen all 9 areas and add at center area
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var area = GetArea(new Vector3(entity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.Position.Z + z * MapArea.AreaSize.Z));
                    entity.AddArea(area);
                    if (x == 0 && z == 0)
                        area.AddEntity(entity);
                }
            }

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
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var area = GetArea(new Vector3(entity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.Position.Z + z * MapArea.AreaSize.Z));
                    if (x == 0 && z == 0)
                        area.RemoveEntity(entity);

                    entity.RemoveArea(area);
                }
            }

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
