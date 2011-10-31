using System;
using System.IO;
using System.Linq;
using Utopia.Server.Events;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerCharacterEntity : ServerDynamicEntity
    {
        private readonly Server _server;

        public ClientConnection Connection { get; private set; }

        /// <summary>
        /// Creates new instance of Server player entity that translates Entity Object Model events to player via network
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="server"></param>
        public ServerPlayerCharacterEntity(ClientConnection connection, DynamicEntity entity, Server server) : base(entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (entity == null) throw new ArgumentNullException("entity");
            Connection = connection;
            _server = server;
        }

        public override void AddArea(MapArea area)
        {
            area.EntityView += AreaEntityView;
            area.EntityMoved += AreaEntityMoved;
            area.EntityUse += AreaEntityUse;
            area.BlocksChanged += AreaBlocksChanged;
            area.EntityModelChanged += AreaEntityModelChanged;
            area.EntityEquipment += AreaEntityEquipment;
            area.StaticEntityAdded += AreaStaticEntityAdded;
            area.StaticEntityRemoved += AreaStaticEntityRemoved;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != this)
                {
                    //Console.WriteLine("TO: {0}, entity {1} in", Connection.Entity.EntityId, dynamicEntity.EntityId);
                    Connection.SendAsync(new EntityInMessage { Entity = (Entity)serverEntity.DynamicEntity });
                }
            }

        }

        void AreaStaticEntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            Connection.SendAsync(new EntityOutMessage { EntityId = e.Entity.StaticId, TakerEntityId = e.ParentDynamicEntityId });
        }

        void AreaStaticEntityAdded(object sender, EntityCollectionEventArgs e)
        {
            Connection.SendAsync(new EntityInMessage { Entity = e.Entity, ParentEntityId = e.ParentDynamicEntityId });
        }

        void AreaEntityEquipment(object sender, CharacterEquipmentEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityEquipmentMessage { Items = new[] { new EquipmentItem(e.Slot, e.EquippedItem.Item) } });
            }
        }

        void AreaEntityModelChanged(object sender, AreaVoxelModelEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                var ms = new MemoryStream();
                using (var writer = new BinaryWriter(ms))
                {
                    e.Entity.Model.Save(writer);
                }

                Connection.SendAsync(new EntityVoxelModelMessage { EntityModel = e.Entity.EntityId, Bytes = ms.ToArray() });
                ms.Dispose();
            }
        }

        protected override void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                //Console.WriteLine("TO: {0},  {1} entity out of view", Connection.Entity.EntityId, e.Entity.EntityId);
                Connection.SendAsync(new EntityOutMessage { EntityId = e.Entity.DynamicEntity.DynamicId });
            }
        }

        protected override void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                //Console.WriteLine("TO: {0},  {1} entity in view", Connection.Entity.EntityId, e.Entity.EntityId);
                Connection.SendAsync(new EntityInMessage { Entity = (Entity)e.Entity.DynamicEntity });
            }
        }

        public override void RemoveArea(MapArea area)
        {
            area.EntityView -= AreaEntityView;
            area.EntityMoved -= AreaEntityMoved;
            area.EntityUse -= AreaEntityUse;
            area.BlocksChanged -= AreaBlocksChanged;
            area.EntityModelChanged -= AreaEntityModelChanged;
            area.EntityEquipment -= AreaEntityEquipment;
            area.StaticEntityAdded -= AreaStaticEntityAdded;
            area.StaticEntityRemoved -= AreaStaticEntityRemoved;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != DynamicEntity)
                {
                    //Console.WriteLine("TO: {0}, entity {1} out (remove)", Connection.Entity.EntityId, dynamicEntity.EntityId);
                    Connection.SendAsync(new EntityOutMessage { EntityId = serverEntity.DynamicEntity.DynamicId });
                }
            }
        }

        public override void Update(DynamicUpdateState gameTime)
        {
            // no need to update something on real player
        }

        void AreaEntityUse(object sender, EntityUseEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityUseMessage 
                {
                    EntityId = e.Entity.DynamicId, 
                    NewBlockPosition = e.NewBlockPosition, 
                    PickedBlockPosition = e.PickedBlockPosition,
                    PickedEntityPosition = e.PickedEntityPosition,
                    ToolId = e.Tool.StaticId
                });
            }
        }

        void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityPositionMessage { EntityId = e.Entity.DynamicId, Position = e.Entity.Position });
            }
        }

        void AreaEntityView(object sender, EntityViewEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityDirectionMessage { EntityId = e.Entity.DynamicId, Direction = e.Entity.Rotation });
            }
        }

        void AreaBlocksChanged(object sender, BlocksChangedEventArgs e)
        {
            Connection.SendAsync(new BlocksChangedMessage { BlockValues = e.BlockValues, BlockPositions = e.GlobalLocations });
        }

        public override void Use(EntityUseMessage entityUseMessage)
        {
            // update entity state
            base.Use(entityUseMessage);

            // find tool
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            var tool = playerCharacter.FindToolById(entityUseMessage.ToolId);

            if (tool != null)
            {
                var toolImpact = tool.Use(playerCharacter, entityUseMessage.UseMode, true);

                // returning tool feedback
                Connection.SendAsync(new UseFeedbackMessage { Token = entityUseMessage.Token, EntityImpactBytes = toolImpact.ToArray() });
            }
            else
            {
                Connection.SendAsync(new ChatMessage { Login = "toolsystem", Message = "Invalid toolid provided. Can not use the tool" });
            }
        }

        public override void Equip(EntityEquipmentMessage entityEquipmentMessage)
        {

        }

        private ContainedSlot _itemTaken;

        private bool TakeItem(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            #region Take from world
            if (itemTransferMessage.SourceContainerEntityId == 0 && itemTransferMessage.DestinationContainerEntityId == playerCharacter.EntityId)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    ServerChunk chunk;
                    if ((chunk = _server.LandscapeManager.SurroundChunks(playerCharacter.Position).First(c => c.Entities.ContainsId(itemTransferMessage.ItemEntityId))) != null)
                    {
                        IStaticEntity entity;
                        chunk.Entities.RemoveById(itemTransferMessage.ItemEntityId, playerCharacter.EntityId, out entity);

                        _itemTaken = new ContainedSlot { Item = (IItem)entity };
                        return true;
                    }
                }
            }
            #endregion
            
            // detect the container
            SlotContainer<ContainedSlot> container = null;

            var position = itemTransferMessage.SourceContainerSlot;

            if (playerCharacter.EntityId == itemTransferMessage.SourceContainerEntityId)
            {
                if (itemTransferMessage.SourceContainerSlot.X == -1)
                {
                    container = playerCharacter.Equipment;
                    position.X = 0;
                }
                else
                    container = playerCharacter.Inventory;
            }
            
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
                var playerCharacter = (PlayerCharacter)DynamicEntity;
                var position = itm.SourceContainerSlot;
                SlotContainer<ContainedSlot> container = null;
                if (playerCharacter.EntityId == itm.SourceContainerEntityId)
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
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            #region Throw to world
            if (itemTransferMessage.SourceContainerEntityId == playerCharacter.EntityId && itemTransferMessage.DestinationContainerEntityId == 0)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    // check if entity have this item

                    var chunk = _server.LandscapeManager.GetChunk(playerCharacter.Position);

                    var containedSlot = new ContainedSlot { ItemsCount = itemTransferMessage.ItemsCount, GridPosition = itemTransferMessage.SourceContainerSlot };

                    var itemType = playerCharacter.Inventory.PeekSlot(containedSlot.GridPosition);

                    if (playerCharacter.Inventory.TakeItem(containedSlot.GridPosition, containedSlot.ItemsCount))
                    {
                        // check if we have correct entityId
                        if (itemType.Item.StaticId == itemTransferMessage.ItemEntityId)
                        {
                            // repeat for entities count
                            for (int i = 0; i < itemTransferMessage.ItemsCount; i++)
                            {
                                // throw it
                                chunk.Entities.Add(itemType.Item, playerCharacter.EntityId);
                            }
                            // ok
                            return true;
                        }

                        // return item to inventory
                        playerCharacter.Inventory.PutItem(itemType.Item, containedSlot.GridPosition, containedSlot.ItemsCount);
                        
                    }
                }

                return false;
            }
            #endregion

            // detect the container
            SlotContainer<ContainedSlot> container = null;

            var position = itemTransferMessage.DestinationContainerSlot;

            if (itemTransferMessage.DestinationContainerEntityId == playerCharacter.EntityId)
            {
                if (position.X == -1)
                {
                    container = playerCharacter.Equipment;
                    position.X = 0;
                }
                else
                {
                    container = playerCharacter.Inventory;
                }
            }

            if (container == null)
                return false;

            return container.PutItem(_itemTaken.Item, position, _itemTaken.ItemsCount);
        }

        public SlotContainer<ContainedSlot> FindContainer(uint entityId, Vector2I position, out Vector2I newPosition)
        {
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            newPosition = position;

            if (entityId == playerCharacter.EntityId)
            {
                if (position.X == -1)
                {
                    newPosition.X = 0;
                    return playerCharacter.Equipment;
                }
                return playerCharacter.Inventory;
            }
            return null;
        }


        public override void ItemTransfer(ItemTransferMessage itm)
        {
            #region Switch
            if (itm.IsSwitch)
            {
                var srcPosition = itm.SourceContainerSlot;
                var dstPosition = itm.DestinationContainerSlot;

                var srcContainer = FindContainer(itm.SourceContainerEntityId, srcPosition, out srcPosition);
                var dstContainer = FindContainer(itm.DestinationContainerEntityId, dstPosition, out dstPosition);

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

                if(!srcContainer.TakeItem(srcSlot.GridPosition, srcSlot.ItemsCount))
                {
                    ItemError();
                    return;
                }
                if(!dstContainer.TakeItem(dstSlot.GridPosition, dstSlot.ItemsCount))
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
                var playerCharacter = (PlayerCharacter)DynamicEntity;

                playerCharacter.Toolbar[itm.SourceContainerSlot.Y] = itm.ItemEntityId;
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

        private void ItemError()
        {
            Connection.SendAsync(new ChatMessage { Login = "inventory", Message = "Invalid transfer operation" });
        }

    }
}
