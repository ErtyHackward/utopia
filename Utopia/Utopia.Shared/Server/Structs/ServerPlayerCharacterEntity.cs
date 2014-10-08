using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Contains PlayerCharacter server logic
    /// </summary>
    public class ServerPlayerCharacterEntity : ServerPlayerEntity
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _dontSendInventoryEvents;
        private PlayerCharacter _playerCharacter;

        public PlayerCharacter PlayerCharacter
        {
            get { return _playerCharacter; }
            private set { 
                if (_playerCharacter == value)
                    return;

                if (_playerCharacter != null)
                {
                    _playerCharacter.Inventory.ItemPut -= Inventory_ItemPut;
                    _playerCharacter.Inventory.ItemTaken -= Inventory_ItemTaken;
                }
                
                _playerCharacter = value;
                base.DynamicEntity = value;

                if (_playerCharacter != null)
                {
                    _playerCharacter.Inventory.ItemPut += Inventory_ItemPut;
                    _playerCharacter.Inventory.ItemTaken += Inventory_ItemTaken;
                }
            }
        }

        public override IDynamicEntity DynamicEntity
        {
            get { return base.DynamicEntity; }
            set { 
                PlayerCharacter = (PlayerCharacter)value; 
            }
        }

        void Inventory_ItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            // skip player own messages
            if (_dontSendInventoryEvents)
            {
                return;
            }

            var msg = new ItemTransferMessage
            {
                SourceContainerEntityLink = PlayerCharacter.GetLink(),
                SourceContainerSlot = e.Slot.GridPosition,
                ItemsCount = e.Slot.ItemsCount,
                SourceEntityId = PlayerCharacter.DynamicId
            };

            // inform client about his inventory change from outside
            Connection.Send(msg);
            CurrentArea.OnCustomMessage(PlayerCharacter.DynamicId, msg);
        }

        void Inventory_ItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            // skip player own messages
            if (_dontSendInventoryEvents)
            {
                return;
            }

            // inform client about his inventory change from outside
            var msg = new ItemTransferMessage { 
                DestinationContainerEntityLink = PlayerCharacter.GetLink(), 
                DestinationContainerSlot = e.Slot.GridPosition, 
                ItemsCount = e.Slot.ItemsCount, 
                ItemEntityId = e.Slot.Item.BluePrintId, 
                SourceEntityId = PlayerCharacter.DynamicId
            };
            Connection.Send(msg);
            CurrentArea.OnCustomMessage(PlayerCharacter.DynamicId, msg);
        }

        public ServerPlayerCharacterEntity(ClientConnection connection, DynamicEntity entity, ServerCore server) : base(connection, entity, server)
        {
            PlayerCharacter = (PlayerCharacter)entity;
        }

        public override void Use(EntityUseMessage entityUseMessage)
        {
            if (entityUseMessage.DynamicEntityId != PlayerCharacter.DynamicId)
                return;

            base.Use(entityUseMessage);

            try
            {
                _dontSendInventoryEvents = true;
                HandleEntityUseMessage(entityUseMessage);
            }
            finally
            {
                _dontSendInventoryEvents = false;
            }
        }

        private void HandleEntityUseMessage(EntityUseMessage entityUseMessage)
        {
            var playerCharacter = PlayerCharacter;

            var toolImpact = playerCharacter.ReplayUse(entityUseMessage);
            
            CurrentArea.UseFeedback(new UseFeedbackMessage
            {
                Token = entityUseMessage.Token,
                Impact = toolImpact,
                OwnerDynamicId = playerCharacter.DynamicId
            });
        }

        public override void ItemTransfer(ItemTransferMessage itm)
        {
            _dontSendInventoryEvents = true;

            try
            {
                if (!PlayerCharacter.ReplayTransfer(itm))
                    ItemError();
            }
            finally
            {
                _dontSendInventoryEvents = false;
            }
        }

        private void ItemError()
        {
            Connection.Send(new ErrorMessage { 
                ErrorCode = ErrorCodes.DesyncDetected, 
                Message = "Invalid transfer operation" 
            });
        }

        public override void RetranslateMessage(IBinaryMessage message)
        {
            base.RetranslateMessage(message);

            {
                var msg = message as EntityHealthMessage;
                if (msg != null)
                {
                    _playerCharacter.HealthImpact(msg.Change);
                    return;
                }
            }

            {
                var msg = message as EntityHealthStateMessage;
                if (msg != null)
                {
                    _playerCharacter.HealthState = msg.HealthState;

                    if (msg.HealthState == DynamicEntityHealthState.Normal)
                        _playerCharacter.DisplacementMode = EntityDisplacementModes.Walking;

                    return;
                }
            }

            {
                var msg = message as EntityAfflictionStateMessage;
                if (msg != null)
                {
                    _playerCharacter.Afflictions = msg.AfflictionState;
                    return;
                }
            }
        }
    }
}