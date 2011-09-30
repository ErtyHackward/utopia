using System;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Network
{
    /// <summary>
    /// Performs items and containers events communication with the server
    /// </summary>
    public class ItemMessageTranslator : IDisposable
    {
        private readonly PlayerCharacter _playerEntity;
        private readonly ServerConnection _connection;
        private Entity _lockedEntity;
        private ContainedSlot _tempSlot;
        private bool _pendingOperation;
        private ISlotContainer<ContainedSlot> _sourceContainer;

        /// <summary>
        /// Occurs when server locks the entity requested
        /// </summary>
        public event EventHandler EntityLocked;

        private void OnEntityLocked()
        {
            var handler = EntityLocked;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when lock operation was failed
        /// </summary>
        public event EventHandler EntityLockFailed;

        private void OnEntityLockFailed()
        {
            var handler = EntityLockFailed;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Creates new
        /// </summary>
        /// <param name="server"></param>
        /// <param name="playerEntity"></param>
        public ItemMessageTranslator(Server server, PlayerCharacter playerEntity)
        {
            if (server == null) throw new ArgumentNullException("server");
            if (playerEntity == null) throw new ArgumentNullException("playerEntity");

            _playerEntity = playerEntity;
            
            _playerEntity.Inventory.ItemPut += InventoryItemPut;
            _playerEntity.Inventory.ItemTaken += InventoryItemTaken;

            _connection = server.ServerConnection;

            _connection.MessageEntityLockResult += ConnectionMessageEntityLockResult;
            

        }

        void ConnectionMessageEntityLockResult(object sender, ProtocolMessageEventArgs<EntityLockResultMessage> e)
        {
            if (e.Message.LockResult == LockResult.SuccessLocked)
            {
                OnEntityLocked();
            }
            else
            {
                _lockedEntity = null;
                OnEntityLockFailed();
            }

        }

        // handling player inventory requests
        private void InventoryItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            // at this point we need to remember what was taken to create appropriate message
            if (_pendingOperation)
                throw new InvalidOperationException("Unable to take another item, release first previous taken item");
            _tempSlot = e.Slot;
            _pendingOperation = true;
            _sourceContainer = (ISlotContainer<ContainedSlot>)sender;
        }

        // handling player inventory requests
        private void InventoryItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            if (!_pendingOperation)
                throw new InvalidOperationException("Unable to put item without taking it first");

            // check if no need to send anything
            if (_sourceContainer == sender && e.Slot.GridPosition == _tempSlot.GridPosition && e.Slot.ItemsCount == _tempSlot.ItemsCount)
            {
                _sourceContainer = null;
                _pendingOperation = false;
                return;
            }

            var destId = sender == _playerEntity.Inventory ? _playerEntity.EntityId : _lockedEntity.EntityId;
            var srcId = _sourceContainer == _playerEntity.Inventory ? _playerEntity.EntityId : (_lockedEntity == null ? 0 : _lockedEntity.EntityId);

            var msg = new ItemTransferMessage
            {
                SourceContainerSlot = _tempSlot.GridPosition,
                SourceContainerEntityId = srcId,
                ItemsCount = e.Slot.ItemsCount,
                DestinationContainerSlot = e.Slot.GridPosition,
                DestinationContainerEntityId = destId
            };

            if (srcId == 0)
                msg.ItemEntityId = _tempSlot.Item.EntityId;

            _connection.SendAsync(msg);

            if (e.Slot.ItemsCount == _tempSlot.ItemsCount)
            {
                _sourceContainer = null;
                _pendingOperation = false;
            }
            else
            {
                _tempSlot.ItemsCount -= e.Slot.ItemsCount;
            }
        }

        /// <summary>
        /// Drops all currently picked entities to the real world
        /// </summary>
        public void DropToWorld()
        {
            if (!_pendingOperation)
                throw new InvalidOperationException("Unable to put item without taking it first");

            _connection.SendAsync(new ItemTransferMessage
            {
                SourceContainerEntityId = _sourceContainer == _playerEntity.Inventory ? _playerEntity.EntityId : _lockedEntity.EntityId,
                SourceContainerSlot= _tempSlot.GridPosition,
                ItemsCount = _tempSlot.ItemsCount,
                ItemEntityId = _tempSlot.Item.EntityId,
                DestinationContainerEntityId = 0,
            });

            _sourceContainer = null;
            _pendingOperation = false;
        }

        /// <summary>
        /// Takes currently locked item from the world
        /// </summary>
        public void TakeFromWorld()
        {
            if (_pendingOperation)
                throw new InvalidOperationException("Unable to take another item, release first previous taken item");

            if (_lockedEntity == null)
                throw new InvalidOperationException("Lock world item before trying to take it");

            if (!(_lockedEntity is IItem))
                throw new InvalidOperationException("Locked entity should implement IItem interface");

            _pendingOperation = true;
            _sourceContainer = null;
            _tempSlot = new ContainedSlot { Item = (IItem)_lockedEntity, ItemsCount = 1 };
        }

        /// <summary>
        /// Sends request to the server to obtain container lock, when received LockResult event will fire
        /// </summary>
        /// <param name="entity"></param>
        public void RequestLock(Entity entity)
        {
            if (entity == null) 
                throw new ArgumentNullException("entity");
            if (_lockedEntity != null)
                throw new InvalidOperationException("Some entity was already locked or requested to be locked. Unable to lock more than one entities at once");
            _lockedEntity = entity;
            _connection.SendAsync(new EntityLockMessage { EntityId = entity.EntityId, Lock = true });
        }

        /// <summary>
        /// Releases last locked container
        /// </summary>
        public void ReleaseLock()
        {
            if (_lockedEntity == null)
                throw new InvalidOperationException("Unable to release the lock because no entity was locked");
            _connection.SendAsync(new EntityLockMessage { EntityId = _lockedEntity.EntityId, Lock = false });
        }

        /// <summary>
        /// Releases all resources taken by this instance
        /// </summary>
        public void Dispose()
        {
            if (_lockedEntity != null)
            {
                ReleaseLock();
            }
            _playerEntity.Inventory.ItemPut -= InventoryItemPut;
            _playerEntity.Inventory.ItemTaken -= InventoryItemTaken;
            _connection.MessageEntityLockResult -= ConnectionMessageEntityLockResult;
        }
    }
}
