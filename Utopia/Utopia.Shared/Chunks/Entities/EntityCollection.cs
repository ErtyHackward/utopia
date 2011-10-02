using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a threadsafe collection of entities
    /// </summary>
    public class EntityCollection
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly object _syncRoot = new object();

        #region Events

        public List<Entity> Data { get { return _entities; } }

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

        /// <summary>
        /// Gets entities count in collection
        /// </summary>
        public int Count
        {
            get { return _entities.Count; }
        }

        /// <summary>
        /// Gets parent chunk class
        /// </summary>
        public AbstractChunk Chunk { get; private set; }

        public EntityCollection(AbstractChunk chunk)
        {
            Chunk = chunk;
        }

        /// <summary>
        /// Adds entity to collection (with locking)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentId"></param>
        public void Add(Entity entity, uint parentId = 0)
        {
            lock (_syncRoot)
                _entities.Add(entity);

            OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, ParentEntityId = parentId });
        }

        /// <summary>
        /// Tries to add entity into collection
        /// </summary>
        /// <param name="entity">Entity object to add</param>
        /// <param name="parentId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryAdd(Entity entity, uint parentId = 0, int timeout = 0)
        {
            if (Monitor.TryEnter(_syncRoot, timeout))
            {
                try
                {
                    _entities.Add(entity);
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                    OnEntityAdded(new EntityCollectionEventArgs { Entity = entity, ParentEntityId = parentId });
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
        public void Remove(Entity entity, uint parentId = 0)
        {
            lock (_syncRoot)
                _entities.Remove(entity);

            OnEntityRemoved(new EntityCollectionEventArgs { Entity = entity, ParentEntityId = parentId });
        }

        /// <summary>
        /// Tries to remove entity from collection
        /// </summary>
        /// <param name="entity">Entity object to remove</param>
        /// <param name="parentId"></param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryRemove(Entity entity, uint parentId, int timeout = 0)
        {
            if (Monitor.TryEnter(_syncRoot, timeout))
            {
                try
                {
                    _entities.Remove(entity);
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                    OnEntityRemoved(new EntityCollectionEventArgs { Entity = entity, ParentEntityId = parentId });
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes entity by ID
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parentEntityId"></param>
        /// <param name="entity"></param>
        public void RemoveById(uint p, uint parentEntityId, out Entity entity)
        {
            lock (_syncRoot)
            {
                var index = _entities.FindIndex(e => e.EntityId == p);
                if (index != -1)
                {
                    entity = _entities[index];
                    _entities.RemoveAt(index);
                    OnEntityRemoved(new EntityCollectionEventArgs { Entity = entity, ParentEntityId = parentEntityId });
                }
                else entity = null;
            }
        }

        /// <summary>
        /// Removes all entites from the collection
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _entities.Clear();
            }
        }

        /// <summary>
        /// Use this method to enumerate over the entites with exclusive lock. (Use it only for fast operations)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Entity> EnumerateFast()
        {
            lock (_syncRoot)
            {
                foreach (var entity in _entities)
                {
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// Use this method to emumerate over the copy of the list of entities. (Use it for slow operations )
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Entity> EnumerateSlow()
        {
            List<Entity> listCopy;
            lock (_syncRoot)
            {
                listCopy = new List<Entity>(_entities);
            }

            return listCopy;
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
                    var entity = EntityFactory.Instance.CreateFromBytes(reader);
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
                    entity.Save(writer);
                }
            }
        }

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ContainsId(uint p)
        {
            lock (_syncRoot)
            {
                return _entities.Find(e => e.EntityId == p) != null;
            }
        }

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="p"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContainsId(uint p, out Entity entity)
        {
            lock (_syncRoot)
            {
                return (entity = _entities.Find(e => e.EntityId == p)) != null;
            }
        }
    }
}
