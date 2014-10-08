using System;
using Ninject;
using Utopia.Entities.Managers;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Network
{
    /// <summary>
    /// Performs items and containers events communication with the server. (Inventory exchange, containers locking)
    /// </summary>
    public class ItemMessageTranslator : IDisposable
    {
        private readonly ServerComponent _server;
        private PlayerCharacter _playerEntity;
        private ContainedSlot _tempSlot;
        private bool _pendingOperation;
        private ISlotContainer<ContainedSlot> _sourceContainer;
        private IEntity _lockedEntity;
        private bool _skipInventoryEvents;

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

        private ISlotContainer<ContainedSlot> _container;
        private PlayerEntityManager _playerEntityManager;

        /// <summary>
        /// Gets or sets second container to perform transfer operations
        /// </summary>
        public ISlotContainer<ContainedSlot> Container
        {
            get { return _container; }
            set {

                if (_container == value)
                    return;

                if (_container != null)
                {
                    _container.ItemPut -= InventoryItemPut;
                    _container.ItemTaken -= InventoryItemTaken;
                    _container.ItemExchanged -= InventoryItemExchanged;
                }

                _container = value;

                if (_container != null)
                {
                    _container.ItemPut += InventoryItemPut;
                    _container.ItemTaken += InventoryItemTaken;
                    _container.ItemExchanged += InventoryItemExchanged;
                }
            }
        }

        public PlayerCharacter PlayerEntity
        {
            get { return _playerEntity; }
            set { 
                if (_playerEntity == value)
                    return;

                if (_playerEntity != null)
                {
                    _playerEntity.Inventory.ItemPut -= InventoryItemPut;
                    _playerEntity.Inventory.ItemTaken -= InventoryItemTaken;
                    _playerEntity.Inventory.ItemExchanged -= InventoryItemExchanged;

                    _playerEntity.Equipment.ItemPut -= InventoryItemPut;
                    _playerEntity.Equipment.ItemTaken -= InventoryItemTaken;
                    _playerEntity.Equipment.ItemExchanged -= InventoryItemExchanged;
                }

                _playerEntity = value;

                if (_playerEntity != null)
                {
                    _playerEntity.Inventory.ItemPut += InventoryItemPut;
                    _playerEntity.Inventory.ItemTaken += InventoryItemTaken;
                    _playerEntity.Inventory.ItemExchanged += InventoryItemExchanged;

                    _playerEntity.Equipment.ItemPut += InventoryItemPut;
                    _playerEntity.Equipment.ItemTaken += InventoryItemTaken;
                    _playerEntity.Equipment.ItemExchanged += InventoryItemExchanged;
                }

            }
        }

        [Inject]
        public PlayerEntityManager PlayerEntityManager
        {
            get { return _playerEntityManager; }
            set { 
                _playerEntityManager = value;

                if (PlayerEntityManager == null) throw new ArgumentNullException("playerEntityManager");

                PlayerEntity = _playerEntityManager.PlayerCharacter;
                _playerEntityManager.PlayerEntityChanged += playerEntityManager_PlayerEntityChanged;
            }
        }

        /// <summary>
        /// Creates new instance of ItemMessageTranslator.
        /// </summary>
        /// <param name="server"></param>
        public ItemMessageTranslator(ServerComponent server)
        {
            if (server == null) throw new ArgumentNullException("server");
            
            _server = server;
            _server.MessageEntityLockResult += ServerMessageEntityLockResult;
            _server.MessageItemTransfer += _server_MessageItemTransfer;

            Enabled = true;
        }

        void playerEntityManager_PlayerEntityChanged(object sender, PlayerEntityChangedEventArgs e)
        {
            PlayerEntity = e.PlayerCharacter;
        }
        
        /// <summary>
        /// Sends request to the server to obtain container lock, when received LockResult event will fire
        /// </summary>
        /// <param name="entity"></param>
        public void RequestLock(IEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            if (_lockedEntity != null)
                throw new InvalidOperationException("Some entity was already locked or requested to be locked. Unable to lock more than one entity at once");
            _lockedEntity = entity;
            _server.ServerConnection.Send(new EntityLockMessage { EntityLink = entity.GetLink(), Lock = true });
        }

        /// <summary>
        /// Releases last locked container
        /// </summary>
        public void ReleaseLock()
        {
            if (_lockedEntity == null)
                throw new InvalidOperationException("Unable to release the lock because no entity was locked");
            _server.ServerConnection.Send(new EntityLockMessage { EntityLink = _lockedEntity.GetLink(), Lock = false });
            _lockedEntity = null;
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

            PlayerEntity.Inventory.ItemPut -= InventoryItemPut;
            PlayerEntity.Inventory.ItemTaken -= InventoryItemTaken;
            PlayerEntity.Inventory.ItemExchanged -= InventoryItemExchanged;

            PlayerEntity.Equipment.ItemPut -= InventoryItemPut;
            PlayerEntity.Equipment.ItemTaken -= InventoryItemTaken;
            PlayerEntity.Equipment.ItemExchanged -= InventoryItemExchanged;

            _server.MessageEntityLockResult -= ServerMessageEntityLockResult;
            _server.MessageItemTransfer -= _server_MessageItemTransfer;
        }

        void ServerMessageEntityLockResult(object sender, ProtocolMessageEventArgs<EntityLockResultMessage> e)
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

        void _server_MessageItemTransfer(object sender, ProtocolMessageEventArgs<ItemTransferMessage> e)
        {
            try
            {
                _skipInventoryEvents = true;

                if (e.Message.DestinationContainerEntityLink.DynamicEntityId == PlayerEntity.DynamicId)
                {
                    // god gave us an item(s)

                    var item = PlayerEntity.EntityFactory.CreateFromBluePrint<Item>((ushort)e.Message.ItemEntityId);
                    PlayerEntity.Inventory.PutItem(item, e.Message.ItemsCount);
                }

                if (e.Message.SourceContainerEntityLink.DynamicEntityId == PlayerEntity.DynamicId)
                {
                    // god took us an item(s)
                    PlayerEntity.Inventory.TakeItem(e.Message.SourceContainerSlot, e.Message.ItemsCount);
                }
            }
            finally
            {
                _skipInventoryEvents = false;
            }

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

            _server.ServerConnection.Send(msg);

            _tempSlot.Item = e.Exchanged.Item;
            _tempSlot.ItemsCount = e.Exchanged.ItemsCount;
            _pendingOperation = true;
        }


        // handling player inventory requests
        private void InventoryItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            if (!Enabled)
                return;

            if (_skipInventoryEvents)
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
            _server.ServerConnection.Send(new ItemTransferMessage { SourceContainerSlot = new Vector2I(-2, slot), ItemEntityId = entityId });
        }

        // handling player inventory requests
        private void InventoryItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            if (!Enabled)
                return;

            if (_skipInventoryEvents)
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

            // special check, if user puts items at the place where it just take the stack, we decrease that amount and wait
            if (_sourceContainer == sender && e.Slot.GridPosition == _tempSlot.GridPosition)
            {
                _tempSlot.ItemsCount -= e.Slot.ItemsCount;
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

            _server.ServerConnection.Send(msg);

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

            _server.ServerConnection.Send(new ItemTransferMessage
            {
                SourceContainerEntityLink = _sourceContainer.Parent.GetLink(),
                SourceContainerSlot = srcPosition,
                ItemsCount = _tempSlot.ItemsCount,
                ItemEntityId = _tempSlot.Item.StaticId,
                DestinationContainerEntityLink = EntityLink.Empty
            });

            _sourceContainer = null;
            _pendingOperation = false;
        }
    }
}
