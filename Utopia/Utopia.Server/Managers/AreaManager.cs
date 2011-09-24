using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpDX;
using Utopia.Server.Events;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Structs;
using System.Threading;
using S33M3Engines.Shared.Math;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Manages the dynamic entites
    /// </summary>
    public class AreaManager
    {
        private readonly ConcurrentDictionary<Vector2I, MapArea> _areas = new ConcurrentDictionary<Vector2I, MapArea>();
        private readonly HashSet<ServerDynamicEntity> _allEntities = new HashSet<ServerDynamicEntity>();

#if DEBUG
        public volatile int entityAreaChangesCount;

        /// <summary>
        /// Use only for test purposes. Thread safty is NOT guaranteed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServerDynamicEntity> EnumerateEntities()
        {
            return _allEntities;
        }
#endif
        #region Events

        /// <summary>
        /// Occurs when new entity was added somewhere on map
        /// </summary>
        public event EventHandler<AreaEntityEventArgs> EntityAdded;

        /// <summary>
        /// Gets total count of entities
        /// </summary>
        public int EntitiesCount
        {
            get { return _allEntities.Count; }
        }

        private void OnEntityAdded(AreaEntityEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when entity was removed from map
        /// </summary>
        public event EventHandler<AreaEntityEventArgs> EntityRemoved;

        private void OnEntityRemoved(AreaEntityEventArgs e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        #endregion

        /// <summary>
        /// Tells entities about blocks change event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeBlocksChanged(BlocksChangedEventArgs e)
        {
            GetArea(new Vector3D(e.ChunkPosition.X, 0, e.ChunkPosition.Y)).OnBlocksChanged(e);
        }

        private MapArea GetArea(Vector3D position)
        {
            var pos = new Vector2I((int)Math.Floor((double)position.X / (MapArea.AreaSize.X)) * MapArea.AreaSize.X, (int)Math.Floor((double)position.Z / (MapArea.AreaSize.Z)) * MapArea.AreaSize.Z);

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

            double tooFarAway = MapArea.AreaSize.X * MapArea.AreaSize.X + MapArea.AreaSize.Z * MapArea.AreaSize.Z;

            var currentArea = GetArea(new Vector3D(e.Entity.DynamicEntity.Position.X, 0,
                                                  e.Entity.DynamicEntity.Position.Z));

            var previousArea = GetArea(new Vector3D(e.PreviousPosition.X, 0,
                                                   e.PreviousPosition.Z));

            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var prev = GetArea(new Vector3D(e.PreviousPosition.X + x * MapArea.AreaSize.X, 0,
                                                   e.PreviousPosition.Z + z*MapArea.AreaSize.Z));
                    
                    if(Vector2I.DistanceSquared(currentArea.Position,prev.Position) > tooFarAway )
                    {
                        // area is too far away, stop listening it
                        e.Entity.RemoveArea(prev);

                        // tell them that entity goes away
                        prev.OnEntityOutOfViewRange(e.Entity);
                    }
                    
                    var now = GetArea(new Vector3D(e.Entity.DynamicEntity.Position.X + x*MapArea.AreaSize.X, 0,
                                                  e.Entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Z));

                    if(Vector2I.DistanceSquared(previousArea.Position, now.Position) > tooFarAway)
                    {
                        // area is too far away from previous center, listen it
                        e.Entity.AddArea(now);

                        // tell them that they should listen new entity
                        now.OnEntityInViewRange(e.Entity);
                    }
                }
            }
            e.Entity.CurrentArea = currentArea;

            currentArea.AddEntity(e.Entity);
        }

        public void AddEntity(ServerDynamicEntity entity)
        {
            lock (_allEntities)
            {
                if (_allEntities.Contains(entity))
                    throw new InvalidOperationException("Such entity is already in manager");

                _allEntities.Add(entity);
            }

            MapArea entityArea = null;

            // listen all 9 areas and add at center area
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var area = GetArea(new Vector3D(entity.DynamicEntity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Z));
                    entity.AddArea(area);
                    if (x == 0 && z == 0)
                    {
                        area.AddEntity(entity);
                        entityArea = area;
                    }
                    area.OnEntityInViewRange(entity);
                }
            }
            entity.CurrentArea = entityArea;

            OnEntityAdded(new AreaEntityEventArgs { Entity = entity });
        }

        public void RemoveEntity(ServerDynamicEntity entity)
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
                    var area = GetArea(new Vector3D(entity.DynamicEntity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Z));
                    if (x == 0 && z == 0)
                        area.RemoveEntity((int)entity.DynamicEntity.EntityId);

                    area.OnEntityOutOfViewRange(entity);

                    entity.RemoveArea(area);
                }
            }

            OnEntityRemoved(new AreaEntityEventArgs { Entity = entity });
        }

        public void Update(DynamicUpdateState gameTime)
        {
            //foreach (var mapArea in _areas)
            //{
            //    UpdateArea(mapArea.Value, gameTime);
            //}

            Parallel.ForEach(_areas, a => UpdateArea(a.Value, gameTime));
        }

        private void UpdateArea(MapArea area, DynamicUpdateState gameTime)
        {
            foreach (var entity in area.Enumerate())
            {
                entity.Update(gameTime);    
            }
        }
    }
}
