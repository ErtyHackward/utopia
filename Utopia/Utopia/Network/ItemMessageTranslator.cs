using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Network
{
    /// <summary>
    /// Performs items and containers events communication with the server. (Inventory exchange, containers locking)
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

        public bool Enabled { get; set; }

        /// <summary>
        /// Creates new instance of ItemMessageTranslator.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="playerEntity"></param>
        public ItemMessageTranslator(ServerComponent server, PlayerCharacter playerEntity)
        {
            if (server == null) throw new ArgumentNullException("server");
            if (playerEntity == null) throw new ArgumentNullException("playerEntity");

            _playerEntity = playerEntity;
            
            _playerEntity.Inventory.ItemPut += InventoryItemPut;
            _playerEntity.Inventory.ItemTaken += InventoryItemTaken;
            _playerEntity.Inventory.ItemExchanged += InventoryItemExchanged;

            _playerEntity.Equipment.ItemPut += InventoryItemPut;
            _playerEntity.Equipment.ItemTaken += InventoryItemTaken;
            _playerEntity.Equipment.ItemExchanged += InventoryItemExchanged;

            _connection = server.ServerConnection;

            _connection.MessageEntityLockResult += ConnectionMessageEntityLockResult;

            Enabled = true;
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
            _playerEntity.Inventory.ItemExchanged -= InventoryItemExchanged;

            _playerEntity.Equipment.ItemPut -= InventoryItemPut;
            _playerEntity.Equipment.ItemTaken -= InventoryItemTaken;
            _playerEntity.Equipment.ItemExchanged -= InventoryItemExchanged;

            _connection.MessageEntityLockResult -= ConnectionMessageEntityLockResult;
        }

        void InventoryItemExchanged(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            if (!Enabled)
                return;

            if (!_pendingOperation)
                throw new InvalidOperationException("Unable to exchange item without taking it first");

            var destContainer = (SlotContainer<ContainedSlot>)sender;
            
            var srcLink = _sourceContainer.Parent.GetLink();
            var destLink = destContainer.Parent.GetLink();

            var srcPosition = _tempSlot.GridPosition;
            var destPosition = e.Slot.GridPosition;

            if (_sourceContainer is CharacterEquipment)
                srcPosition.X = -1;
            if (destContainer is CharacterEquipment)
                destPosition.X = -1;

            var msg = new ItemTransferMessage
            {
                SourceContainerSlot = srcPosition,
                SourceContainerEntityLink = srcLink,
                ItemsCount = e.Slot.ItemsCount,
                DestinationContainerSlot = destPosition,
                DestinationContainerEntityLink = destLink,
                IsSwitch = true
            };

            if (srcLink.IsEmpty)
                msg.ItemEntityId = _tempSlot.Item.StaticId;

            _connection.SendAsync(msg);

            _tempSlot.Item = e.Exchanged.Item;
            _tempSlot.ItemsCount = e.Exchanged.ItemsCount;
            _pendingOperation = true;
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
            if (!Enabled)
                return;

            // at this point we need to remember what was taken to create appropriate message
            if (_pendingOperation)
                throw new InvalidOperationException("Unable to take another item, release first previous taken item");

            _tempSlot = e.Slot;
            _pendingOperation = true;
            _sourceContainer = (ISlotContainer<ContainedSlot>)sender;
        }

        public void SetToolBar(int slot, uint entityId)
        {
            _connection.SendAsync(new ItemTransferMessage { SourceContainerSlot = new Vector2I(-2, slot),  ItemEntityId = entityId });
        }

        // handling player inventory requests
        private void InventoryItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            if (!Enabled)
                return;

            if (!_pendingOperation)
                throw new InvalidOperationException("Unable to put item without taking it first");

            // check if no need to send anything
            if (_sourceContainer == sender && e.Slot.GridPosition == _tempSlot.GridPosition && e.Slot.ItemsCount == _tempSlot.ItemsCount)
            {
                _sourceContainer = null;
                _pendingOperation = false;
                return;
            }

            var destContainer = (SlotContainer<ContainedSlot>)sender;

            var srcLink = _sourceContainer.Parent.GetLink();
            var destLink = destContainer.Parent.GetLink();

            var srcPosition = _tempSlot.GridPosition;
            var destPosition = e.Slot.GridPosition;

            if (_sourceContainer is CharacterEquipment)
                srcPosition.X = -1;
            if (destContainer is CharacterEquipment)
                destPosition.X = -1;

            var msg = new ItemTransferMessage
            {
                SourceContainerSlot = srcPosition,
                SourceContainerEntityLink = srcLink,
                ItemsCount = e.Slot.ItemsCount,
                DestinationContainerSlot = destPosition,
                DestinationContainerEntityLink = destLink
            };

            if (srcLink.IsEmpty)
                msg.ItemEntityId = _tempSlot.Item.StaticId;

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

            var srcPosition = _tempSlot.GridPosition;

            if (_sourceContainer is CharacterEquipment)
            {
                srcPosition.X = -1;
            }

            _connection.SendAsync(new ItemTransferMessage
            {
                SourceContainerEntityLink = _sourceContainer == _playerEntity.Inventory ? _playerEntity.GetLink() : _lockedEntity.GetLink(),
                SourceContainerSlot = srcPosition,
                ItemsCount = _tempSlot.ItemsCount,
                ItemEntityId = _tempSlot.Item.StaticId,
                DestinationContainerEntityLink = EntityLink.Empty
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
            _connection.SendAsync(new EntityLockMessage { EntityLink = entity.GetLink(), Lock = true });
        }

        /// <summary>
        /// Releases last locked container
        /// </summary>
        public void ReleaseLock()
        {
            if (_lockedEntity == null)
                throw new InvalidOperationException("Unable to release the lock because no entity was locked");
            _connection.SendAsync(new EntityLockMessage { EntityLink = _lockedEntity.GetLink(), Lock = false });
        }
    }
}
