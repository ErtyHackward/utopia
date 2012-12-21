using System;
using System.Collections.Generic;
using System.Threading;
using ProtoBuf;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a threadsafe collection of entities
    /// </summary>
    [ProtoContract]
    public class EntityCollection : IStaticContainer
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private SortedList<uint, IStaticEntity> _entities = new SortedList<uint, IStaticEntity>();
        private readonly object _syncRoot = new object();
        private bool _initialisation;

        #region Events
        /// <summary>
        /// Occurs when collection change
        /// </summary>
        public event EventHandler CollectionDirty;

        protected void OnCollectionDirty()
        {
            if (_initialisation) return;
            var handler = CollectionDirty;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Occurs when new static entity was added
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> EntityAdded;

        private void OnEntityAdded(EntityCollectionEventArgs e)
        {
            e.Chunk = Chunk;
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when collection is cleared
        /// </summary>
        public event EventHandler CollectionCleared;

        protected void OnCollectionCleared()
        {
            if (_initialisation) return;
            var handler = CollectionCleared;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when some static entity was removed
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> EntityRemoved;

        private void OnEntityRemoved(EntityCollectionEventArgs e)
        {
            e.Chunk = Chunk;
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        #endregion

        public bool IsDirty { get; set; }

        [ProtoMember(1, OverwriteList = true)]
        public List<KeyValuePair<uint, IEntity>> SerializeEntities {
            get
            {
                var list = new List<KeyValuePair<uint, IEntity>>();

                foreach (var staticEntity in _entities)
                {
                    list.Add(new KeyValuePair<uint, IEntity>(staticEntity.Key, staticEntity.Value));    
                }

                return list;
            }
            set {
                _entities.Clear();
                foreach (var keyValuePair in value)
                {
                    var entity = (IStaticEntity)keyValuePair.Value;
                    Add(entity, 0, true);
                }
            }
        }


        public SortedList<uint, IStaticEntity> Entities
        {
            get { return _entities; }
            set { _entities = value; }
        }

        /// <summary>
        /// Gets entities count in collection
        /// </summary>
        public int Count
        {
            get { return _entities.Count; }
        }
        
        /// <summary>
        /// Gets parent chunk object
        /// </summary>
        public AbstractChunk Chunk { get; set; }

        public EntityCollection()
        {

        }

        public EntityCollection(AbstractChunk chunk)
        {
            Chunk = chunk;
        }

        /// <summary>
        /// Informs collection that it was changed
        /// Should be used when the inside entity parameters changed
        /// </summary>
        public void SetDirty()
        {
            OnCollectionDirty();
        }

        /// <summary>
        /// Must be used if we want to copy the entities from another collection to this one
        /// </summary>
        /// <param name="entityCollection">The new entities collection</param>
        /// <param name="atChunkCreationTime"></param>
        public void Import(EntityCollection entityCollection, bool atChunkCreationTime = false)
        {
            lock (_syncRoot)
            {
                _initialisation = true;
                _entities.Clear();

                foreach (var entity in entityCollection.EnumerateFast())
                {
                    entity.Container = this;
                    _entities.Add(entity.StaticId, entity);
                }
                _initialisation = false;
            }

            foreach (var entity in entityCollection.EnumerateFast())
            {
                OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, SourceDynamicEntityId = 0, AtChunkCreationTime = atChunkCreationTime });
            }
        }

        /// <summary>
        /// Returns free unique number for this collection
        /// </summary>
        /// <returns></returns>
        public uint GetFreeId()
        {
            lock (_entities)
            {
                if(_entities.Count > 0)
                    return _entities[_entities.Keys[_entities.Count - 1]].StaticId + 1;
                return 1;
            }
        }
        
        /// <summary>
        /// Adds entity to collection (with locking). Assign new unique id for the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sourceDynamicId"></param>
        public void Add(IStaticEntity entity, uint sourceDynamicId = 0, bool atChunkCreationTime = false)
        {
            lock (_syncRoot)
            {
                entity.StaticId = GetFreeId();
                entity.Container = this;
                _entities.Add(entity.StaticId, entity);
            }
            
            IsDirty = true;
            OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, SourceDynamicEntityId = sourceDynamicId, AtChunkCreationTime = atChunkCreationTime });
            OnCollectionDirty();
        }

        /// <summary>
        /// Adds new static entity to the container. Updates static entity id and Container properties
        /// </summary>
        /// <param name="entity"></param>
        public void Add(IStaticEntity entity)
        {
            Add(entity, 0, false);
        }

        /// <summary>
        /// Tries to add entity into collection. Assign new unique id for the entity
        /// </summary>
        /// <param name="entity">Entity object to add</param>
        /// <param name="sourceDynamicId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <param name="atChunkCreationTime"></param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryAdd(IStaticEntity entity, uint sourceDynamicId = 0, int timeout = 0, bool atChunkCreationTime = false)
        {
            if (Monitor.TryEnter(_syncRoot, timeout))
            {
                try
                {
                    IsDirty = true;
                    entity.StaticId = GetFreeId();
                    entity.Container = this;
                    _entities.Add(entity.StaticId, entity);
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                    OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, SourceDynamicEntityId = sourceDynamicId, AtChunkCreationTime = atChunkCreationTime });
                    OnCollectionDirty();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes entity from collection (with locking)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sourceId"></param>
        public void Remove(IStaticEntity entity, uint sourceId = 0)
        {
            bool removed;
            lock (_syncRoot)
                removed = _entities.Remove(entity.StaticId);
            
            if (removed)
            {
                IsDirty = true;
                entity.Container = this;
                OnEntityRemoved(new EntityCollectionEventArgs {
                    Entity = entity, 
                    SourceDynamicEntityId = sourceId
                });
                OnCollectionDirty();
            }
        }

        /// <summary>
        /// Tries to remove entity from collection
        /// </summary>
        /// <param name="entity">Entity object to remove</param>
        /// <param name="sourceId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryRemove(IStaticEntity entity, uint sourceId, int timeout = 0)
        {
            if (Monitor.TryEnter(_syncRoot, timeout))
            {
                var removed = false;
                try
                {
                    IsDirty = true;
                    removed = _entities.Remove(entity.StaticId);
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                    if (removed)
                    {
                        entity.Container = null;
                        OnEntityRemoved(new EntityCollectionEventArgs {
                            Entity = entity, 
                            SourceDynamicEntityId = sourceId
                        });
                        OnCollectionDirty();
                    }
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// Removes entity by ID
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <param name="sourceDynamicEntityId"></param>
        /// <param name="entity"></param>
        public void RemoveById(uint staticEntityId, uint sourceDynamicEntityId, out IStaticEntity entity)
        {
            lock (_syncRoot)
            {
                if(_entities.TryGetValue(staticEntityId, out entity))
                {
                    IsDirty = true;
                    _entities.Remove(staticEntityId);
                    OnEntityRemoved(new EntityCollectionEventArgs { 
                        Entity = entity, 
                        SourceDynamicEntityId = sourceDynamicEntityId 
                    });
                    entity.Container = null; //Remove from its container after event raise !
                    OnCollectionDirty();
                }
            }
        }

        /// <summary>
        /// Removes entity by ID
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <param name="sourceDynamicEntityId"></param>
        /// <param name="entity"></param>
        public void RemoveById(uint staticEntityId, uint sourceDynamicEntityId = 0)
        {
            lock (_syncRoot)
            {
                IStaticEntity entity;
                if (_entities.TryGetValue(staticEntityId, out entity))
                {
                    IsDirty = true;
                    _entities.Remove(staticEntityId);
                    entity.Container = null; //Remove from its container
                    OnEntityRemoved(new EntityCollectionEventArgs
                    {
                        Entity = entity,
                        SourceDynamicEntityId = sourceDynamicEntityId
                    });
                    OnCollectionDirty();
                }
            }
        }

        /// <summary>
        /// Removes all entites from the collection
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                if (_entities.Count == 0) return;

                foreach (var staticEntity in _entities)
                {
                    staticEntity.Value.Container = null;
                }

                _entities.Clear();
                OnCollectionCleared();
                OnCollectionDirty();
            }
        }

        /// <summary>
        /// Use this method to enumerate over the entites with exclusive lock. (Use it only for fast operations)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IStaticEntity> EnumerateFast()
        {
            lock (_syncRoot)
            {
                foreach (var entity in _entities)
                {
                    yield return entity.Value;
                }
            }
        }

        /// <summary>
        /// Use this method to iterate over entities of type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> Enumerate<T>() where  T: IStaticEntity
        {
            lock (_syncRoot)
            {
                foreach (var entity in _entities)
                {
                    if(entity.Value is T)
                        yield return (T)entity.Value;
                }
            }
        }

        public void Foreach<T>(Action<T> action) where T : IStaticEntity
        {
            lock (_syncRoot)
            {
                foreach (var entity in _entities)
                {
                    if (entity.Value is T)
                        action((T)entity.Value);
                }
            }
        }

        public void RemoveAll<T>(Predicate<T> condition) where T : IStaticEntity
        {
            lock (_syncRoot)
            {
                for (int i = _entities.Count - 1; i >= 0; i--)
                {
                    var value = _entities[_entities.Keys[i]]; // O(1)
                    if (value is T && condition((T)value))
                    {
                        IsDirty = true;
                        _entities.RemoveAt(i); // O(Count)
                        OnEntityRemoved(new EntityCollectionEventArgs {
                            Entity = value
                        });
                        value.Container = null;
                    }
                }

                if (IsDirty)
                    OnCollectionDirty();
            }
        }

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <returns></returns>
        public bool ContainsId(uint staticEntityId)
        {
            lock (_syncRoot)
            {
                return _entities.ContainsKey(staticEntityId);
            }
        }

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContainsId(uint staticEntityId, out IStaticEntity entity)
        {
            lock (_syncRoot)
            {
                return _entities.TryGetValue(staticEntityId, out entity);
            }
        }

        /// <summary>
        /// Removes static entity from the container
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(IStaticEntity entity)
        {
            Remove(entity, 0);
        }

        /// <summary>
        /// Gets entity by its id
        /// </summary>
        /// <param name="staticId"></param>
        /// <returns></returns>
        public IStaticEntity GetStaticEntity(uint staticId)
        {
            lock (_syncRoot)
                return _entities[staticId];
        }
    }
}
