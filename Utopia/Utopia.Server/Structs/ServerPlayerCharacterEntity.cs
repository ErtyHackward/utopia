﻿using System;
using System.IO;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Contains PlayerCharacter server logic
    /// </summary>
    public class ServerPlayerCharacterEntity : ServerPlayerEntity
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _dontSendInventoryEvents;
        private ContainedSlot _itemTaken;
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
                base.DynamicEntity = value; 
            }
        }

        void Inventory_ItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            // skip player own messages
            if (_dontSendInventoryEvents)
            {
                return;
            }

            // inform client about his inventory change from outside
            Connection.Send(new ItemTransferMessage
            {
                SourceContainerEntityLink = PlayerCharacter.GetLink(),
                SourceContainerSlot = e.Slot.GridPosition,
                ItemsCount = e.Slot.ItemsCount
            });
        }

        void Inventory_ItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            // skip player own messages
            if (_dontSendInventoryEvents)
            {
                return;
            }

            // inform client about his inventory change from outside
            Connection.Send(new ItemTransferMessage { 
                DestinationContainerEntityLink = PlayerCharacter.GetLink(), 
                DestinationContainerSlot = e.Slot.GridPosition, 
                ItemsCount = e.Slot.ItemsCount, 
                ItemEntityId = e.Slot.Item.BluePrintId
            });
        }

        public ServerPlayerCharacterEntity(ClientConnection connection, DynamicEntity entity, Server server) : base(connection, entity, server)
        {
            PlayerCharacter = (PlayerCharacter)entity;
        }

        public override void Use(EntityUseMessage entityUseMessage)
        {
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

            if (entityUseMessage.UseType == UseType.Craft)
            {
                var impact = new ToolImpact();
                impact.Success = playerCharacter.Craft(entityUseMessage.RecipeIndex);
                CurrentArea.UseFeedback(new UseFeedbackMessage
                {
                    Token = entityUseMessage.Token,
                    Impact = impact,
                    OwnerDynamicId = playerCharacter.DynamicId
                });
                return;
            }

            // detect use type, if 0 then it is entity use, otherwise it is tool use
            if (entityUseMessage.ToolId != 0)
            {
                // find item
                var item = playerCharacter.FindItemById(entityUseMessage.ToolId);

                if (item != null)
                {
                    if (entityUseMessage.UseType == UseType.Use)
                    {
                        var toolImpact = playerCharacter.ToolUse();
                        CurrentArea.UseFeedback(new UseFeedbackMessage
                        {
                            Token = entityUseMessage.Token,
                            Impact = toolImpact,
                            OwnerDynamicId = playerCharacter.DynamicId
                        });
                    }
                    if (entityUseMessage.UseType == UseType.Put)
                    {
                        var toolImpact = playerCharacter.PutUse();
                        CurrentArea.UseFeedback(new UseFeedbackMessage
                        {
                            Token = entityUseMessage.Token,
                            Impact = toolImpact,
                            OwnerDynamicId = playerCharacter.DynamicId
                        });
                    }
                }
                else
                {
                    Connection.Send(new ChatMessage
                    {
                        DisplayName = "toolsystem",
                        Message = "Invalid toolid provided. Can not use the tool"
                    });
                }
            }
            else
            {
                var toolImpact = playerCharacter.HandUse();
                CurrentArea.UseFeedback(new UseFeedbackMessage
                {
                    Token = entityUseMessage.Token,
                    Impact = toolImpact,
                    OwnerDynamicId = playerCharacter.DynamicId
                });
            }
        }

        private bool TakeItem(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = PlayerCharacter;

            #region Take from world
            if (itemTransferMessage.SourceContainerEntityLink.IsEmpty && itemTransferMessage.DestinationContainerEntityLink.IsPointsTo(playerCharacter))
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    ServerChunk chunk;
                    if ((chunk = Server.LandscapeManager.SurroundChunks(playerCharacter.Position).First(c => c.Entities.ContainsId(itemTransferMessage.ItemEntityId))) != null)
                    {
                        IStaticEntity entity;
                        chunk.Entities.RemoveById(itemTransferMessage.ItemEntityId, playerCharacter.DynamicId, out entity);

                        _itemTaken = new ContainedSlot { Item = (Item)entity };
                        return true;
                    }
                }
            }
            #endregion

            // detect the container
            SlotContainer<ContainedSlot> container = null;

            var position = itemTransferMessage.SourceContainerSlot;
            var srcLink = itemTransferMessage.SourceContainerEntityLink;

            container = FindContainer(srcLink, position, out position);

            if (container == null)
                return false;

            _itemTaken = container.PeekSlot(position);

            if (!container.TakeItem(position, itemTransferMessage.ItemsCount))
            {
                _itemTaken = null;
                return false;
            }

            _itemTaken.ItemsCount = itemTransferMessage.ItemsCount;

            return true;
        }

        private void RollbackItem(ItemTransferMessage itm)
        {
            if (_itemTaken != null)
            {
                var playerCharacter = PlayerCharacter;
                var position = itm.SourceContainerSlot;
                SlotContainer<ContainedSlot> container = null;
                if (itm.SourceContainerEntityLink.IsPointsTo(playerCharacter))
                {
                    if (itm.SourceContainerSlot.X == -1)
                    {
                        container = playerCharacter.Equipment;
                        position.X = 0;
                    }
                    else
                        container = playerCharacter.Inventory;
                }

                if (container != null)
                    container.PutItem(_itemTaken.Item, position, _itemTaken.ItemsCount);
                else
                    throw new InvalidOperationException("Unable to rollback");

                _itemTaken = null;
            }
        }

        private bool PutItem(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = PlayerCharacter;

            #region Throw to world
            if (itemTransferMessage.SourceContainerEntityLink.IsPointsTo(playerCharacter) && itemTransferMessage.DestinationContainerEntityLink.IsEmpty)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    // check if entity have this item
                    var chunk = Server.LandscapeManager.GetChunkFromBlock(playerCharacter.Position);

                    // check if we have correct entityId
                    if (_itemTaken.Item.StaticId == itemTransferMessage.ItemEntityId)
                    {
                        // repeat for entities count
                        for (int i = 0; i < itemTransferMessage.ItemsCount; i++)
                        {
                            // put to current position
                            _itemTaken.Item.Position = playerCharacter.Position;

                            // throw it
                            chunk.Entities.Add(_itemTaken.Item, playerCharacter.DynamicId);

                        }
                        // ok
                        return true;
                    }
                }

                return false;
            }
            #endregion

            // detect the container

            var position = itemTransferMessage.DestinationContainerSlot;
            var destLink = itemTransferMessage.DestinationContainerEntityLink;

            var container = FindContainer(destLink, position, out position);

            if (container == null)
                return false;

            return container.PutItem(_itemTaken.Item, position, _itemTaken.ItemsCount);
        }

        /// <summary>
        /// Returns appropriate container from the player and provides correct position inside the container
        /// </summary>
        /// <param name="link"></param>
        /// <param name="position"></param>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        public SlotContainer<ContainedSlot> FindContainer(EntityLink link, Vector2I position, out Vector2I newPosition)
        {
            var playerCharacter = PlayerCharacter;

            newPosition = position;

            if (link.IsPointsTo(playerCharacter))
            {
                if (position.X == -1)
                {
                    newPosition.X = 0;
                    return playerCharacter.Equipment;
                }
                return playerCharacter.Inventory;
            }
            if (link.IsStatic)
            {
                var entity = link.ResolveStatic(Server.LandscapeManager);
                return (entity as Container).Content;
            }
            return null;
        }

        public override void ItemTransfer(ItemTransferMessage itm)
        {
            logger.Info("Transfer " + itm.ToString());

            _dontSendInventoryEvents = true;

            try
            {
                #region Switch
                if (itm.IsSwitch)
                {
                    var srcPosition = itm.SourceContainerSlot;
                    var dstPosition = itm.DestinationContainerSlot;

                    var srcContainer = FindContainer(itm.SourceContainerEntityLink, srcPosition, out srcPosition);
                    var dstContainer = FindContainer(itm.DestinationContainerEntityLink, dstPosition, out dstPosition);

                    if (srcContainer == null || dstContainer == null)
                    {
                        ItemError();
                        return;
                    }

                    // switching is allowed only if we have both slots busy
                    var srcSlot = srcContainer.PeekSlot(srcPosition);
                    var dstSlot = dstContainer.PeekSlot(dstPosition);

                    if (srcSlot == null || dstSlot == null)
                    {
                        ItemError();
                        return;
                    }

                    if (!srcContainer.TakeItem(srcSlot.GridPosition, srcSlot.ItemsCount))
                    {
                        ItemError();
                        return;
                    }
                    if (!dstContainer.TakeItem(dstSlot.GridPosition, dstSlot.ItemsCount))
                    {
                        ItemError();
                        return;
                    }
                    if (!srcContainer.PutItem(dstSlot.Item, srcSlot.GridPosition, dstSlot.ItemsCount))
                    {
                        ItemError();
                        return;
                    }
                    if (!dstContainer.PutItem(srcSlot.Item, dstSlot.GridPosition, srcSlot.ItemsCount))
                    {
                        ItemError();
                        return;
                    }

                    // ok
                    return;
                }
                #endregion

                if (itm.SourceContainerSlot.X == -2)
                {
                    // set toolbar slot
                    var playerCharacter = PlayerCharacter;

                    var item = playerCharacter.FindItemById(itm.ItemEntityId);

                    playerCharacter.Toolbar[itm.SourceContainerSlot.Y] = item.BluePrintId;
                    return;
                }


                if (TakeItem(itm))
                {
                    if (PutItem(itm))
                    {
                        // ok
                        return;
                    }
                    RollbackItem(itm);
                }

                // impossible to transfer
                ItemError();
            }
            finally
            {
                _dontSendInventoryEvents = false;
            }
        }

        private void ItemError()
        {
            Connection.Send(new ChatMessage { DisplayName = "inventory", Message = "Invalid transfer operation" });
        }

    }
}