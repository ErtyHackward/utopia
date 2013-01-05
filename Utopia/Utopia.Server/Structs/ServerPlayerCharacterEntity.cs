using System;
using System.IO;
using System.Linq;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerCharacterEntity : ServerDynamicEntity
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            area.EntityEquipment += AreaEntityEquipment;
            area.StaticEntityAdded += AreaStaticEntityAdded;
            area.StaticEntityRemoved += AreaStaticEntityRemoved;
            area.EntityLockChanged += AreaEntityLockChanged;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != this)
                {
                    //Console.WriteLine("TO: {0}, entity {1} in", Connection.Entity.EntityId, dynamicEntity.EntityId);
                    Connection.Send(new EntityInMessage { Entity = (Entity)serverEntity.DynamicEntity, Link = serverEntity.DynamicEntity.GetLink() });
                }
            }
        }

        public override void RemoveArea(MapArea area)
        {
            area.EntityView -= AreaEntityView;
            area.EntityMoved -= AreaEntityMoved;
            area.EntityUse -= AreaEntityUse;
            area.BlocksChanged -= AreaBlocksChanged;
            area.EntityEquipment -= AreaEntityEquipment;
            area.StaticEntityAdded -= AreaStaticEntityAdded;
            area.StaticEntityRemoved -= AreaStaticEntityRemoved;
            area.EntityLockChanged -= AreaEntityLockChanged;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != DynamicEntity)
                {
                    //Console.WriteLine("TO: {0}, entity {1} out (remove)", Connection.Entity.EntityId, dynamicEntity.EntityId);
                    Connection.Send(new EntityOutMessage { EntityId = serverEntity.DynamicEntity.DynamicId, EntityType = EntityType.Dynamic, Link = serverEntity.DynamicEntity.GetLink() });
                }
            }
        }

        void AreaEntityLockChanged(object sender, Shared.Net.Connections.ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            Connection.Send(e.Message);
        }
        
        void AreaStaticEntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            Connection.Send(new EntityOutMessage { EntityId = e.Entity.StaticId, TakerEntityId = e.SourceDynamicEntityId, EntityType = EntityType.Static, Link = e.Entity.GetLink() });
        }

        void AreaStaticEntityAdded(object sender, EntityCollectionEventArgs e)
        {
            Connection.Send(new EntityInMessage { Entity = e.Entity, SourceEntityId = e.SourceDynamicEntityId, Link = e.Entity.GetLink() });
        }

        void AreaEntityEquipment(object sender, CharacterEquipmentEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityEquipmentMessage { Items = new[] { new EquipmentItem(e.Slot, e.EquippedItem.Item) } });
            }
        }

        protected override void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                //Console.WriteLine("TO: {0},  {1} entity out of view", Connection.Entity.EntityId, e.Entity.EntityId);
                Connection.Send(new EntityOutMessage { EntityId = e.Entity.DynamicEntity.DynamicId, EntityType = EntityType.Dynamic, Link = e.Entity.DynamicEntity.GetLink() });
            }
        }

        protected override void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                //Console.WriteLine("TO: {0},  {1} entity in view", Connection.Entity.EntityId, e.Entity.EntityId);
                Connection.Send(new EntityInMessage { Entity = (Entity)e.Entity.DynamicEntity, Link = e.Entity.DynamicEntity.GetLink() });
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
                Connection.Send(new EntityUseMessage (e));
            }
        }

        void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityPositionMessage { EntityId = e.Entity.DynamicId, Position = e.Entity.Position });
            }
        }

        void AreaEntityView(object sender, EntityViewEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityHeadDirectionMessage { EntityId = e.Entity.DynamicId, Rotation = e.Entity.HeadRotation });
            }
        }

        void AreaBlocksChanged(object sender, BlocksChangedEventArgs e)
        {
            Connection.Send(new BlocksChangedMessage { BlockValues = e.BlockValues, BlockPositions = e.GlobalLocations, Tags = e.Tags });
        }

        public override void Use(EntityUseMessage entityUseMessage)
        {
            // update entity state
            base.Use(entityUseMessage);
            
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            // detect use type, if 0 then it is entity use, otherwise it is tool use
            if (entityUseMessage.ToolId != 0)
            {
                // find tool
                var tool = playerCharacter.FindToolById(entityUseMessage.ToolId);

                if (tool != null)
                {
                    if (entityUseMessage.UseType == UseType.Use)
                    {
                        var toolImpact = tool.Use(playerCharacter, true);
                        // returning tool feedback
                        Connection.Send(new UseFeedbackMessage
                            {
                                Token = entityUseMessage.Token,
                                EntityImpactBytes = toolImpact.Serialize()
                            });
                    }
                    if (entityUseMessage.UseType == UseType.Put)
                    {
                        var toolImpact = tool.Put(playerCharacter);
                        // returning tool feedback
                        Connection.Send(new UseFeedbackMessage
                            {
                                Token = entityUseMessage.Token,
                                EntityImpactBytes = toolImpact.Serialize()
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

                var link = entityUseMessage.PickedEntityLink;
                var entity = link.ResolveStatic(_server.LandscapeManager); 

                if (entity is IUsableEntity)
                {
                    var usableEntity = (IUsableEntity)entity;
                    usableEntity.Use();
                }
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
            if (itemTransferMessage.SourceContainerEntityLink.IsEmpty && itemTransferMessage.DestinationContainerEntityLink.IsPointsTo(playerCharacter))
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    ServerChunk chunk;
                    if ((chunk = _server.LandscapeManager.SurroundChunks(playerCharacter.Position).First(c => c.Entities.ContainsId(itemTransferMessage.ItemEntityId))) != null)
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
                var playerCharacter = (PlayerCharacter)DynamicEntity;
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
            var playerCharacter = (PlayerCharacter)DynamicEntity;

            #region Throw to world
            if (itemTransferMessage.SourceContainerEntityLink.IsPointsTo(playerCharacter) && itemTransferMessage.DestinationContainerEntityLink.IsEmpty)
            {
                if (itemTransferMessage.ItemEntityId != 0)
                {
                    // check if entity have this item
                    var chunk = _server.LandscapeManager.GetChunk(playerCharacter.Position);
                    
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
            var playerCharacter = (PlayerCharacter) DynamicEntity;

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
                var entity = link.ResolveStatic(_server.LandscapeManager);
                return (entity as Container).Content;
            }
            return null;
        }


        public override void ItemTransfer(ItemTransferMessage itm)
        {
            logger.Info("Transfer " + itm.ToString());

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
            Connection.Send(new ChatMessage { DisplayName = "inventory", Message = "Invalid transfer operation" });
        }

    }
}
