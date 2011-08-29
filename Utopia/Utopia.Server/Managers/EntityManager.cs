using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    // todo: finish entity manager (use areas)
    /// <summary>
    /// Manages the active entites
    /// </summary>
    public class EntityManager
    {
        private object _syncRoot = new object();
        private List<IActiveEntity> _activeEntities = new List<IActiveEntity>();

        public event EventHandler<EntityManagerEventArgs> EntityAdded;

        public event EventHandler<EntityManagerEventArgs> EntityRemoved;

        public event EventHandler<EntityManagerEventArgs> EntityMoved;

        public EntityManager()
        {
            
        }

        public void Update(DateTime gameTime)
        {
            lock (_syncRoot)
            {
                for (int i = _activeEntities.Count - 1; i >= 0; i--)
                {
                    _activeEntities[i].Update(gameTime);
                }
            }
        }
    }

    /// <summary>
    /// Threadsafe map area. Contains dynamic entities.
    /// </summary>
    public class MapArea
    {
        /// <summary>
        /// Size of each Area
        /// </summary>
        public static Location2<int> AreaSize = new Location2<int>(16 * 20, 16 * 20);

        private object _syncRoot = new object();
        private List<ServerDynamicEntity> _entities = new List<ServerDynamicEntity>();

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        /// <summary>
        /// Bounding rectangle
        /// </summary>
        public Rectangle Rectangle { get; private set; }
        
        public MapArea(IntVector2 topLeftPoint)
        {
            Rectangle = new Rectangle(topLeftPoint.X, topLeftPoint.Y, AreaSize.X, AreaSize.Z);
        }

        public void AddEntity(ServerDynamicEntity entity)
        {
            lock (_syncRoot)
                _entities.Add(entity);
        }

        public void RemoveEntity(ServerDynamicEntity entity)
        {
            lock (_syncRoot)
                _entities.Remove(entity);
        }

        public IEnumerable<IActiveEntity> Enumerate()
        {
            lock (_syncRoot)
            {
                foreach (var serverDynamicEntity in _entities)
                {
                    yield return serverDynamicEntity.Entity;
                }
            }
        }

    }

    /// <summary>
    /// Wrapper around active entity class, because entity should not know about areas
    /// </summary>
    public class ServerDynamicEntity
    {
        public MapArea Area1 { get; set; }
        public MapArea Area2 { get; set; }
        public IActiveEntity Entity { get; set; }

        public ServerDynamicEntity()
        {
            
        }
    }

    public interface IActiveEntity
    {
        /// <summary>
        /// Perform 
        /// </summary>
        void Update(DateTime gameTime);
    }
}
