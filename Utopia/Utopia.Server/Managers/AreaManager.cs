using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utopia.Server.Events;
using Utopia.Server.Structs;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Structs;
using System.Threading;
using Utopia.Shared.Structs.Helpers;
using S33M3Resources.Structs;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Manages the dynamic entites
    /// </summary>
    public class AreaManager : IDisposable
    {
        private readonly Server _server;
        private readonly ConcurrentDictionary<Vector2I, MapArea> _areas = new ConcurrentDictionary<Vector2I, MapArea>();
        private readonly Dictionary<uint, ServerDynamicEntity> _dynamicEntities = new  Dictionary<uint, ServerDynamicEntity>();
        private readonly object _areaManagerSyncRoot = new object();
        private readonly Timer _entityUpdateTimer;
        private DateTime _lastUpdate = DateTime.MinValue;

        #region Properties
#if DEBUG
        public volatile int entityAreaChangesCount;

        /// <summary>
        /// Use only for test purposes. Thread safty is NOT guaranteed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServerDynamicEntity> EnumerateEntities()
        {
            return _dynamicEntities.Values;
        }
#endif
        /// <summary>
        /// Gets total count of entities
        /// </summary>
        public int EntitiesCount
        {
            get { return _dynamicEntities.Count; }
        }
        #endregion

        #region Events

        /// <summary>
        /// Occurs when new entity was added somewhere on map
        /// </summary>
        public event EventHandler<AreaEntityEventArgs> EntityAdded;

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

        public event EventHandler BeforeUpdate;

        private void OnBeforeUpdate()
        {
            var handler = BeforeUpdate;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler AfterUpdate;

        private void OnAfterUpdate()
        {
            var handler = AfterUpdate;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
        
        public AreaManager(Server server)
        {
            _server = server;
            _server.LandscapeManager.ChunkLoaded += LandscapeManagerChunkLoaded;
            _server.LandscapeManager.ChunkUnloaded += LandscapeManagerChunkUnloaded;
            _entityUpdateTimer = new Timer(UpdateDynamic, null, 0, 100);
        }

        void LandscapeManagerChunkLoaded(object sender, LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged += ChunkBlocksChanged;
            e.Chunk.Entities.EntityAdded += EntitiesEntityAdded;
            e.Chunk.Entities.EntityRemoved += EntitiesEntityRemoved;
        }

        void LandscapeManagerChunkUnloaded(object sender, LandscapeManagerChunkEventArgs e)
        {
            e.Chunk.BlocksChanged -= ChunkBlocksChanged;
            e.Chunk.Entities.EntityAdded -= EntitiesEntityAdded;
            e.Chunk.Entities.EntityRemoved -= EntitiesEntityRemoved;
        }

        void EntitiesEntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            GetArea(e.Entity.Position).OnStaticEntityRemoved(e);
        }

        void EntitiesEntityAdded(object sender, EntityCollectionEventArgs e)
        {
            GetArea(e.Entity.Position).OnStaticEntityAdded(e);
        }

        void ChunkBlocksChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            var chunk = (ServerChunk)sender;

            chunk.LastAccess = DateTime.Now;

            var globalPos = new Vector3I[e.Locations.Length];

            e.Locations.CopyTo(globalPos, 0);
            BlockHelper.ConvertToGlobal(chunk.Position, globalPos);

            // tell entities about blocks change
            var eargs = new BlocksChangedEventArgs 
            { 
                ChunkPosition = chunk.Position, 
                BlockValues = e.Bytes, 
                Locations = e.Locations, 
                GlobalLocations = globalPos 
            };

            GetArea(new Vector3D(eargs.ChunkPosition.X * AbstractChunk.ChunkSize.X, 0, eargs.ChunkPosition.Y * AbstractChunk.ChunkSize.Z)).OnBlocksChanged(eargs);
        }

        // update dynamic entities
        private void UpdateDynamic(object o)
        {
            if (Monitor.TryEnter(_areaManagerSyncRoot))
            {
                try
                {
                    var state = new DynamicUpdateState
                    {
                        ElapsedTime = _lastUpdate == DateTime.MinValue ? TimeSpan.Zero : _server.Clock.Now - _lastUpdate,
                        CurrentTime = _server.Clock.Now
                    };

                    _lastUpdate = _server.Clock.Now;

                    Update(state);

                }
                finally
                {
                    Monitor.Exit(_areaManagerSyncRoot);
                }
            }
            else
            {
                TraceHelper.Write("Warning! Server is overloaded. Try to decrease dynamic entities count");
            }
        }

        private MapArea GetArea(Vector3D position)
        {
            var pos = new Vector2I((int)Math.Floor(position.X / (MapArea.AreaSize.X)) * MapArea.AreaSize.X, (int)Math.Floor(position.Z / (MapArea.AreaSize.Y)) * MapArea.AreaSize.Y);

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

            double tooFarAway = MapArea.AreaSize.X * MapArea.AreaSize.X + MapArea.AreaSize.Y * MapArea.AreaSize.Y;

            var currentArea = GetArea(new Vector3D(e.Entity.DynamicEntity.Position.X, 0,
                                                  e.Entity.DynamicEntity.Position.Z));

            var previousArea = GetArea(new Vector3D(e.PreviousPosition.X, 0,
                                                   e.PreviousPosition.Z));

            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var prev = GetArea(new Vector3D(e.PreviousPosition.X + x * MapArea.AreaSize.X, 0,
                                                   e.PreviousPosition.Z + z*MapArea.AreaSize.Y));
                    
                    if(Vector2I.DistanceSquared(currentArea.Position,prev.Position) > tooFarAway )
                    {
                        // area is too far away, stop listening it
                        e.Entity.RemoveArea(prev);

                        // tell them that entity goes away
                        prev.OnEntityOutOfViewRange(e.Entity);
                    }
                    
                    var now = GetArea(new Vector3D(e.Entity.DynamicEntity.Position.X + x*MapArea.AreaSize.X, 0,
                                                  e.Entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Y));

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
            // maybe we need to generate new unique entity id
            if (entity.DynamicEntity.DynamicId == 0)
            {

            }

            lock (_dynamicEntities)
            {
                if (_dynamicEntities.ContainsKey(entity.DynamicEntity.DynamicId))
                    throw new InvalidOperationException("Such entity is already in manager");

                _dynamicEntities.Add(entity.DynamicEntity.DynamicId, entity);
            }

            MapArea entityArea = null;

            // listen all 9 areas and add at center area
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var area = GetArea(new Vector3D(entity.DynamicEntity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Y));
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
            lock (_dynamicEntities)
            {
                removed = _dynamicEntities.Remove(entity.DynamicEntity.DynamicId);
            }

            if (!removed) return;

            // remove entity from areas
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    var area = GetArea(new Vector3D(entity.DynamicEntity.Position.X + x * MapArea.AreaSize.X, 0,
                                                   entity.DynamicEntity.Position.Z + z * MapArea.AreaSize.Y));
                    if (x == 0 && z == 0)
                        area.RemoveEntity((int)entity.DynamicEntity.DynamicId);

                    area.OnEntityOutOfViewRange(entity);

                    entity.RemoveArea(area);
                }
            }

            OnEntityRemoved(new AreaEntityEventArgs { Entity = entity });
        }

        public void Update(DynamicUpdateState gameTime)
        {
            OnBeforeUpdate();

            //foreach (var mapArea in _areas)
            //{
            //    UpdateArea(mapArea.Value, gameTime);
            //}

            Parallel.ForEach(_areas, a => UpdateArea(a.Value, gameTime));
            OnAfterUpdate();
        }

        private void UpdateArea(MapArea area, DynamicUpdateState gameTime)
        {
            foreach (var entity in area.Enumerate())
            {
                entity.Update(gameTime);    
            }
        }

        public void Dispose()
        {
            _server.LandscapeManager.ChunkLoaded -= LandscapeManagerChunkLoaded;
            _server.LandscapeManager.ChunkUnloaded -= LandscapeManagerChunkUnloaded;
            _entityUpdateTimer.Dispose();
        }

        public bool TryFind(uint entityId, out ServerDynamicEntity entity)
        {
            return _dynamicEntities.TryGetValue(entityId, out entity);
        }

        /// <summary>
        /// Search for dynamic entity and returns entity or null
        /// </summary>
        /// <param name="dynamicEntityId"></param>
        /// <returns></returns>
        public ServerDynamicEntity Find(uint dynamicEntityId)
        {
            ServerDynamicEntity result;
            _dynamicEntities.TryGetValue(dynamicEntityId, out result);
            return result;
        }
    }
}
