using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Events;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Threadsafe map area. Contains dynamic entities.
    /// </summary>
    public class MapArea
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Size of each Area
        /// </summary>
        public static Vector2I AreaSize = new Vector2I(16 * 8, 16 * 8);

        private readonly object _syncRoot = new object();
        private readonly ConcurrentDictionary<int, ServerDynamicEntity> _entities = new ConcurrentDictionary<int, ServerDynamicEntity>();
        private Rectangle _rectangle;

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        #region Events
        
        /* 
         * Each entity may listen one or more events of its MapArea.
         * Entity should start listening at AreaEnter and remove listening at AreaLeave
         * Here should be added all required multicast events like direction change and tool using
         */

        /// <summary>
        /// Occurs when tool processing is finished
        /// </summary>
        public event EventHandler<EntityUseFeedbackEventArgs> EntityUseFeedback;

        protected virtual void OnEntityUseFeedback(EntityUseFeedbackEventArgs e)
        {
            var handler = EntityUseFeedback;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when one of dynamic entities changes its view direction
        /// </summary>
        public event EventHandler<EntityViewEventArgs> EntityView;

        private void OnEntityView(EntityViewEventArgs e)
        {
            var handler = EntityView;
            if (handler != null) handler(this, e);
        }
        
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

        /// <summary>
        /// Occurs when some entity was added to this area
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityAdded;

        private void OnEntityAdded(ServerDynamicEntityEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity was removed from this area
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityRemoved;

        private void OnEntityRemoved(ServerDynamicEntityEventArgs e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when one or more blocks in any of chunks in that area was chanded
        /// </summary>
        public event EventHandler<BlocksChangedEventArgs> BlocksChanged;

        public void OnBlocksChanged(BlocksChangedEventArgs e)
        {
            var handler = BlocksChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when some entity get far away
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityOutOfViewRange;

        public void OnEntityOutOfViewRange(ServerDynamicEntity iDynamicEntity)
        {
            var handler = EntityOutOfViewRange;
            if (handler != null) handler(this, new ServerDynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        /// <summary>
        /// Occurs when some entity is close enough to listen to
        /// </summary>
        public event EventHandler<ServerDynamicEntityEventArgs> EntityInViewRange;

        public void OnEntityInViewRange(ServerDynamicEntity iDynamicEntity)
        {
            var handler = EntityInViewRange;
            if (handler != null) handler(this, new ServerDynamicEntityEventArgs { Entity = iDynamicEntity });
        }

        /// <summary>
        /// Occurs when some entity appears in one of area chunks
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> StaticEntityAdded;

        public void OnStaticEntityAdded(EntityCollectionEventArgs e)
        {
            var handler = StaticEntityAdded;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when some entity removed from one of area chunks
        /// </summary>
        public event EventHandler<EntityCollectionEventArgs> StaticEntityRemoved;

        public void OnStaticEntityRemoved(EntityCollectionEventArgs e)
        {
            var handler = StaticEntityRemoved;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ProtocolMessageEventArgs<EntityLockMessage>> EntityLockChanged;

        public void OnEntityLockChanged(ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            var handler = EntityLockChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ProtocolMessageEventArgs<EntityVoxelModelMessage>> VoxelModelChanged;

        public void OnEntityVoxelModel(ProtocolMessageEventArgs<EntityVoxelModelMessage> e)
        {
            var handler = VoxelModelChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ServerProtocolMessageEventArgs> CustomMessage;

        public void OnCustomMessage(uint sourceId, IBinaryMessage message)
        {
            var handler = CustomMessage;
            if (handler != null) handler(this, new ServerProtocolMessageEventArgs { 
                DynamicId = sourceId, 
                Message = message 
            });
        }
        
        #endregion

        /// <summary>
        /// Gets area top left position
        /// </summary>
        public Vector2I Position { get; private set; }
        
        /// <summary>
        /// Bounding rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get { return _rectangle; }
            private set { _rectangle = value; }
        }

        public MapArea(Vector2I topLeftPoint)
        {
            Position = topLeftPoint;

            Rectangle = new Rectangle(topLeftPoint.X, topLeftPoint.Y, AreaSize.X, AreaSize.Y);
        }

        public void AddEntity(ServerDynamicEntity entity)
        {
            if (_entities.TryAdd(entity.GetHashCode(), entity))
            {
                // add events to retranslate
                entity.PositionChanged += EntityPositionChanged;
                entity.DynamicEntity.ViewChanged += EntityViewChanged;

                OnEntityAdded(new ServerDynamicEntityEventArgs { Entity = entity });
            }
            else
            {
                logger.Error("Failed to add entity to the area {0} {1}", entity.DynamicEntity.DynamicId, Position);
            }
        }

        public void RemoveEntity(int entityId)
        {
            ServerDynamicEntity e;
            if (_entities.TryRemove(entityId, out e))
            {
                // remove events from re-translating
                e.PositionChanged -= EntityPositionChanged;
                e.DynamicEntity.ViewChanged -= EntityViewChanged;

                OnEntityRemoved(new ServerDynamicEntityEventArgs { Entity = e });
            }
            else
            {
                logger.Error("Failed to remove entity from the area {0} {1}", entityId, Position);
            }
        }
        
        void EntityViewChanged(object sender, EntityViewEventArgs e)
        {
            // retranslate
            OnEntityView(e);
        }

        private void EntityPositionChanged(object sender, ServerDynamicEntityMoveEventArgs e)
        {
            // retranslate
            OnEntityMoved(new EntityMoveEventArgs { 
                Entity = e.ServerDynamicEntity.DynamicEntity, 
                PreviousPosition = e.PreviousPosition 
            });
            
            // we need to tell area manager that entity leaves us, to put it into new area
            if (!_rectangle.Contains(e.ServerDynamicEntity.DynamicEntity.Position.AsVector3()))
            {
                OnEntityLeave(new EntityLeaveAreaEventArgs { 
                    Entity = e.ServerDynamicEntity, 
                    PreviousPosition = e.PreviousPosition 
                });

                RemoveEntity(e.ServerDynamicEntity.GetHashCode());
            }
        }

        public void UseFeedback(UseFeedbackMessage msg)
        {
            OnEntityUseFeedback(new EntityUseFeedbackEventArgs { Message = msg });
        }

        public IEnumerable<ServerDynamicEntity> Enumerate()
        {
            foreach (var entity in _entities)
            {
                yield return entity.Value;
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
        
        public bool ContainsEntity(uint entityId)
        {
            return _entities.ContainsKey((int)entityId);
        }
    }

    public class ServerProtocolMessageEventArgs : EventArgs
    {
        public uint DynamicId { get; set; }

        public IBinaryMessage Message { get; set; }
    }

    public class EntityUseFeedbackEventArgs : EventArgs
    {
        public UseFeedbackMessage Message { get; set; }
    }

    public class ServerDynamicEntityEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
    }
}