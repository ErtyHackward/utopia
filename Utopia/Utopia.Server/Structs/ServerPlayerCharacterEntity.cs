using System;
using System.IO;
using System.Linq;
using Utopia.Net.Messages;
using Utopia.Server.Events;
using Utopia.Server.Managers;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;

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
                Connection.SendAsync(new EntityEquipmentMessage { Items = new[] { new EquipmentItem(e.Slot, e.Item) } });
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
                    PickedEntityId = e.PickedEntityId, 
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
                tool.Use();
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
                    playerCharacter.Inventory.TakeItem(itemSlot);

                    var oldItem = playerCharacter.Equipment.WearItem((Item)equipmentItem.Entity, equipmentItem.Slot);

                    if (oldItem != null)
                    {
                        itemSlot.Item = oldItem;
                        playerCharacter.Inventory.PutItem(itemSlot);
                    }

                }
                else
                {
                    // impossible to equip
                    Connection.SendAsync(new ChatMessage { Login = "inventory", Message = "Unable to equip this item, it should be inside the inventory" });
                }
            }
        }

        public override void ItemTransfer(ItemTransferMessage itemTransferMessage)
        {
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            // internal inventory transfer?
            if (playerCharacter.EntityId == itemTransferMessage.SourceContainerEntityId && itemTransferMessage.SourceContainerEntityId == itemTransferMessage.DestinationContainerEntityId)
            {
                var slot = new ContainedSlot { GridPosition = itemTransferMessage.SourceContainerSlot, ItemsCount = itemTransferMessage.ItemsCount };
                // check if we allow transfer
                slot = playerCharacter.Inventory.TakeSlot(slot);

                if (slot != null)
                {
                    slot.GridPosition = itemTransferMessage.DestinationContainerSlot;
                    if (playerCharacter.Inventory.PutItem(slot))
                    {
                        // ok
                        return;
                    }
                    else
                    {
                        // return back
                        slot.GridPosition = itemTransferMessage.SourceContainerSlot;
                        playerCharacter.Inventory.PutItem(slot);
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
                            if (playerCharacter.Inventory.PutItem((Item)entity))
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
                    
                    var slot = playerCharacter.Inventory.TakeSlot(containedSlot);

                    if (slot != null)
                    {
                        // check if we have correct entityId
                        if (slot.Item.EntityId == itemTransferMessage.ItemEntityId)
                        {
                            // repeat for entities count
                            for (int i = 0; i < itemTransferMessage.ItemsCount; i++)
                            {
                                // throw it
                                chunk.Entities.Add(slot.Item, playerCharacter.EntityId);    
                            }
                            // ok
                            return;
                        }
                        else
                        {
                            // return item to inventory
                            playerCharacter.Inventory.PutItem(slot);
                        }


                    }



                }
            }

            // impossible to transfer
            Connection.SendAsync(new ChatMessage { Login = "inventory", Message = "Invalid transfer operation" });
        }

    }
}
