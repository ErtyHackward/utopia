using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a threadsafe collection of entities
    /// </summary>
    public class EntityCollection : IStaticContainer
    {
        private readonly SortedList<uint, IStaticEntity> _entities = new SortedList<uint, IStaticEntity>();
        private readonly object _syncRoot = new object();

        #region Events
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
        public AbstractChunk Chunk { get; private set; }

        public EntityCollection(AbstractChunk chunk)
        {
            Chunk = chunk;
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
        /// Adds entity to collection (with locking). Assing new unique id for the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentDynamicId"></param>
        public void Add(IStaticEntity entity, uint parentDynamicId = 0)
        {
            lock (_syncRoot)
            {
                entity.StaticId = GetFreeId();
                entity.Container = this;
                _entities.Add(entity.StaticId, entity);
            }
            IsDirty = true;
            OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, ParentDynamicEntityId = parentDynamicId });
        }

        /// <summary>
        /// Tries to add entity into collection. Assign new unique id for the entity
        /// </summary>
        /// <param name="entity">Entity object to add</param>
        /// <param name="parentDynamicId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryAdd(IStaticEntity entity, uint parentDynamicId = 0, int timeout = 0)
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
                    OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, ParentDynamicEntityId = parentDynamicId });
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes entity from collection (with locking)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentId"></param>
        public void Remove(IStaticEntity entity, uint parentId = 0)
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
                    ParentDynamicEntityId = parentId
                });
            }
        }

        /// <summary>
        /// Tries to remove entity from collection
        /// </summary>
        /// <param name="entity">Entity object to remove</param>
        /// <param name="parentId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryRemove(IStaticEntity entity, uint parentId, int timeout = 0)
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
                            ParentDynamicEntityId = parentId
                        });
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
        /// <param name="parentDynamicEntityId"></param>
        /// <param name="entity"></param>
        public void RemoveById(uint staticEntityId, uint parentDynamicEntityId, out IStaticEntity entity)
        {
            lock (_syncRoot)
            {
                if(_entities.TryGetValue(staticEntityId, out entity))
                {
                    IsDirty = true;
                    _entities.Remove(staticEntityId);
                    entity.Container = null;
                    OnEntityRemoved(new EntityCollectionEventArgs { 
                        Entity = entity, 
                        ParentDynamicEntityId = parentDynamicEntityId 
                    });
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
                foreach (var staticEntity in _entities)
                {
                    staticEntity.Value.Container = null;
                }

                _entities.Clear();
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
        public IEnumerable<T> Enumerate<T>()
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

        //TODO: test for perfomance
        public void RemoveAll<T>(Predicate<T> condition)
        {
            lock (_syncRoot)
            {
                for (int i = _entities.Count - 1; i >= 0; i--)
                {
                    var value = _entities[_entities.Keys[i]]; // O(1)
                    if (value is T && condition((T) value))
                    {
                        value.Container = null;
                        _entities.RemoveAt(i); // O(Count)
                        OnEntityRemoved(new EntityCollectionEventArgs {
                            Entity = value
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Performs loading of the entites from binary format
        /// </summary>
        /// <param name="ms">Memory stream to deserialize</param>
        /// <param name="offset">offset in memory stream</param>
        /// <param name="length">bytes amount to process</param>
        public void LoadEntities(MemoryStream ms, int offset, int length)
        {
            using (var reader = new BinaryReader(ms))
            {
                ms.Position = offset;

                while (ms.Position < offset + length)
                {
                    var entity = (IStaticEntity)EntityFactory.Instance.CreateFromBytes(reader);
                    Add(entity);
                }
            }
        }

        /// <summary>
        /// Performs saving of entites to binary format
        /// </summary>
        /// <param name="writer"></param>
        public void SaveEntities(BinaryWriter writer)
        {
            lock (_syncRoot)
            {
                foreach (var entity in _entities)
                {
                    entity.Value.Save(writer);
                }
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
        /// Adds new static entity to the container. Updates static entity id and Container properties
        /// </summary>
        /// <param name="entity"></param>
        public void Add(IStaticEntity entity)
        {
            Add(entity, 0);
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
