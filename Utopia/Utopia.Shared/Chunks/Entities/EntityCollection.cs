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

        /// <summary>
        /// Gets entities count in collection
        /// </summary>
        public int Count
        {
            get { return _entities.Count; }
        }

        /// <summary>
        /// Adds entity to collection (with locking)
        /// </summary>
        /// <param name="entity"></param>
        public void Add(Entity entity)
        {
            lock (_syncRoot)
                _entities.Add(entity);
        }

        /// <summary>
        /// Tries to add entity into collection
        /// </summary>
        /// <param name="entity">Entity object to add</param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryAdd(Entity entity, int timeout = 0)
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
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes entity from collection (with locking)
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(Entity entity)
        {
            lock (_syncRoot)
                _entities.Remove(entity);
        }

        /// <summary>
        /// Tries to remove entity from collection
        /// </summary>
        /// <param name="entity">Entity object to remove</param>
        /// <param name="timeout">Number of milliseconds to wait for lock</param>
        /// <returns>True if succeed otherwise False</returns>
        public bool TryRemove(Entity entity, int timeout = 0)
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
                }
                return true;
            }
            return false;
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
        /// <param name="factory">Factory to produce Entites classes</param>
        /// <param name="ms">Memory stream to deserialize</param>
        /// <param name="offset">offset in memory stream</param>
        /// <param name="length">bytes amount to process</param>
        public void LoadEntities(EntityFactory factory, MemoryStream ms, int offset, int length)
        {
            var reader = new BinaryReader(ms);
            ms.Position = offset;

            while (ms.Position < offset + length)
            {
                var classId = reader.ReadUInt16();
                var entity = factory.CreateEntity((EntityClassId)classId);
                Add(entity);
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

        public void RemoveById(uint p, out Entity entity)
        {
            lock (_syncRoot)
            {
                var index = _entities.FindIndex(e => e.EntityId == p);
                if (index != -1)
                {
                    entity = _entities[index];
                    _entities.RemoveAt(index);
                }
                else entity = null;
            }
        }
    }


}
