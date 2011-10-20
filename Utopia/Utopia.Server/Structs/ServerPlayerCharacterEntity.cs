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
            Connection.SendAsync(new EntityOutMessage { EntityId = e.Entity.EntityId, TakerEntityId = e.ParentEntityId });
        }

        void AreaStaticEntityAdded(object sender, EntityCollectionEventArgs e)
        {
            Connection.SendAsync(new EntityInMessage { Entity = e.Entity, ParentEntityId = e.ParentEntityId });
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
                Connection.SendAsync(new EntityOutMessage { EntityId = e.Entity.DynamicEntity.EntityId });
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
                    Connection.SendAsync(new EntityOutMessage { EntityId = serverEntity.DynamicEntity.EntityId });
                }
            }
        }

        public override void Update(Shared.Structs.DynamicUpdateState gameTime)
        {
            // no need to update something on real player
        }

        void AreaEntityUse(object sender, EntityUseEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityUseMessage 
                { 
                    EntityId = e.Entity.EntityId, 
                    NewBlockPosition = e.NewBlockPosition, 
                    PickedBlockPosition = e.PickedBlockPosition,
                    PickedEntityPosition = e.PickedEntityPosition,
                    ToolId = e.Tool.EntityId
                });
            }
        }

        void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityPositionMessage { EntityId = e.Entity.EntityId, Position = e.Entity.Position });
            }
        }

        void AreaEntityView(object sender, EntityViewEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.SendAsync(new EntityDirectionMessage { EntityId = e.Entity.EntityId, Direction = e.Entity.Rotation });
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
                var toolImpact = tool.Use(playerCharacter);

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
            // first check has the entity this item it want to equip
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            foreach (var equipmentItem in entityEquipmentMessage.Items)
            {
                ContainedSlot itemSlot;
                if ((itemSlot = playerCharacter.Inventory.Find(equipmentItem.Entity)) != null)
                {
                    // take item from inventory
                    playerCharacter.Inventory.TakeItem(itemSlot.GridPosition, itemSlot.ItemsCount);

                    var oldItem = playerCharacter.Equipment.WearItem(new ContainedSlot { Item = (IItem)equipmentItem.Entity }, equipmentItem.Slot);

                    if (oldItem != null)
                    {
                        itemSlot.Item = oldItem;
                        playerCharacter.Inventory.PutItem(itemSlot.Item, itemSlot.GridPosition, itemSlot.ItemsCount);
                    }

                }
                else
                {
                    // impossible to equip
                    Connection.SendAsync(new ChatMessage { Login = "inventory", Message = "Unable to equip this item, it should be inside the inventory" });
                }
            }
        }

        private ContainedSlot _itemTaken;

        private bool TakeItem(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            // first find the container

            if (playerCharacter.EntityId == itemTransferMessage.SourceContainerEntityId)
            {
                if (itemTransferMessage.SourceSlotType != EquipmentSlotType.None)
                {
                    // equipment take
                    _itemTaken = playerCharacter.Equipment.UnWearItem(itemTransferMessage.SourceSlotType);
                    if (_itemTaken != null)
                        return true;
                    return false;
                }

            }

            return false;
        }

        private bool PutItem(ItemTransferMessage itemTransferMessage)
        {

            return false;
        }

        public override void ItemTransfer(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            if (itemTransferMessage.IsSwitch)
            {
                //this.LockedEntity




                return;
            }


            // internal inventory transfer?
            if (playerCharacter.EntityId == itemTransferMessage.SourceContainerEntityId && itemTransferMessage.SourceContainerEntityId == itemTransferMessage.DestinationContainerEntityId)
            {
                
                var slot = new ContainedSlot
                               {
                                   GridPosition = itemTransferMessage.SourceContainerSlot,
                                   ItemsCount = itemTransferMessage.ItemsCount
                               };

                var itemType = playerCharacter.Inventory.PeekSlot(slot.GridPosition);

                // check if we allow transfer
                if (playerCharacter.Inventory.TakeItem(slot.GridPosition, slot.ItemsCount))
                {
                    if (playerCharacter.Inventory.PutItem(itemType.Item, itemTransferMessage.DestinationContainerSlot, slot.ItemsCount ))
                    {
                        // ok
                        return;
                    }
                    else
                    {
                        // return back
                        slot.GridPosition = itemTransferMessage.SourceContainerSlot;
                        playerCharacter.Inventory.PutItem(itemType.Item, slot.GridPosition, slot.ItemsCount);
                    }
                }

            }

            // take from world?
            if (itemTransferMessage.SourceContainerEntityId == 0 && itemTransferMessage.DestinationContainerEntityId == playerCharacter.EntityId)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    ServerChunk chunk;
                    if ( (chunk = _server.LandscapeManager.SurroundChunks(playerCharacter.Position).First(c => c.Entities.ContainsId(itemTransferMessage.ItemEntityId))) != null)
                    {
                        Entity entity;
                        chunk.Entities.RemoveById(itemTransferMessage.ItemEntityId, playerCharacter.EntityId, out entity);
                        if (entity != null)
                        {
                            if (playerCharacter.Inventory.PutItem((IItem)entity))
                                return; // ok
                        }
                    }
                }
            }

            //throw item to world
            if (itemTransferMessage.SourceContainerEntityId == playerCharacter.EntityId && itemTransferMessage.DestinationContainerEntityId == 0)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    // check if entity have this item

                    var chunk = _server.LandscapeManager.GetChunk(playerCharacter.Position);

                    var containedSlot = new ContainedSlot{ ItemsCount = itemTransferMessage.ItemsCount, GridPosition = itemTransferMessage.SourceContainerSlot };

                    var itemType = playerCharacter.Inventory.PeekSlot(containedSlot.GridPosition);

                    if (playerCharacter.Inventory.TakeItem(containedSlot.GridPosition, containedSlot.ItemsCount))
                    {
                        // check if we have correct entityId
                        if (itemType.Item.EntityId == itemTransferMessage.ItemEntityId)
                        {
                            // repeat for entities count
                            for (int i = 0; i < itemTransferMessage.ItemsCount; i++)
                            {
                                // throw it
                                chunk.Entities.Add((Entity)itemType.Item, playerCharacter.EntityId);    
                            }
                            // ok
                            return;
                        }
                        else
                        {
                            // return item to inventory
                            playerCharacter.Inventory.PutItem(itemType.Item, containedSlot.GridPosition, containedSlot.ItemsCount);
                        }


                    }



                }
            }

            // impossible to transfer
            Connection.SendAsync(new ChatMessage { Login = "inventory", Message = "Invalid transfer operation" });
        }

    }
}
