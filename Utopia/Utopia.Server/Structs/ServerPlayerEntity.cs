using System;
using System.Linq;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerEntity : ServerDynamicEntity
    {
        public ClientConnection Connection { get; private set; }

        /// <summary>
        /// Creates new instance of Server player entity that translates Entity Object Model events to player via network
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="server"></param>
        public ServerPlayerEntity(ClientConnection connection, DynamicEntity entity, Server server) : base(server, entity)
        {
            if (connection == null) 
                throw new ArgumentNullException("connection");
            if (entity == null) 
                throw new ArgumentNullException("entity");

            Connection = connection;
        }

        public override void AddArea(MapArea area)
        {
            area.EntityView          += AreaEntityView;
            area.EntityMoved         += AreaEntityMoved;
            area.EntityUse           += AreaEntityUse;
            area.EntityUseFeedback   += AreaEntityUseFeedback;
            area.BlocksChanged       += AreaBlocksChanged;
            area.StaticEntityAdded   += AreaStaticEntityAdded;
            area.StaticEntityRemoved += AreaStaticEntityRemoved;
            area.EntityLockChanged   += AreaEntityLockChanged;
            area.TransferMessage     += AreaTransferMessage;
            area.VoxelModelChanged   += AreaOnVoxelModelChanged;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != this)
                {
                    Connection.Send(new EntityInMessage { 
                        Entity = (Entity)serverEntity.DynamicEntity, 
                        Link = serverEntity.DynamicEntity.GetLink() 
                    });
                }
            }
        }

        public override void RemoveArea(MapArea area)
        {
            area.EntityView          -= AreaEntityView;
            area.EntityMoved         -= AreaEntityMoved;
            area.EntityUse           -= AreaEntityUse;
            area.EntityUseFeedback   -= AreaEntityUseFeedback;
            area.BlocksChanged       -= AreaBlocksChanged;
            area.StaticEntityAdded   -= AreaStaticEntityAdded;
            area.StaticEntityRemoved -= AreaStaticEntityRemoved;
            area.EntityLockChanged   -= AreaEntityLockChanged;
            area.TransferMessage     -= AreaTransferMessage;
            area.VoxelModelChanged   -= AreaOnVoxelModelChanged;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity.DynamicEntity != DynamicEntity)
                {
                    Connection.Send(new EntityOutMessage { 
                        EntityId = serverEntity.DynamicEntity.DynamicId, 
                        Link = serverEntity.DynamicEntity.GetLink() 
                    });
                }
            }
        }

        void AreaEntityLockChanged(object sender, Shared.Net.Connections.ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            Connection.Send(e.Message);
        }
        
        void AreaStaticEntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            Connection.Send(new EntityOutMessage { 
                EntityId = e.Entity.StaticId, 
                TakerEntityId = e.SourceDynamicEntityId, 
                Link = e.Entity.GetLink() 
            });
        }

        void AreaStaticEntityAdded(object sender, EntityCollectionEventArgs e)
        {
            Connection.Send(new EntityInMessage { 
                Entity = e.Entity, 
                SourceEntityId = e.SourceDynamicEntityId, 
                Link = e.Entity.GetLink() 
            });
        }

        protected override void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity.DynamicEntity != DynamicEntity)
            {
                Connection.Send(new EntityOutMessage { 
                    EntityId = e.Entity.DynamicEntity.DynamicId, 
                    Link = e.Entity.DynamicEntity.GetLink() 
                });
            }
        }

        protected override void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e)
        {
            if (e.Entity.DynamicEntity != DynamicEntity)
            {
                Connection.Send(new EntityInMessage { 
                    Entity = e.Entity.DynamicEntity, 
                    Link = e.Entity.DynamicEntity.GetLink() 
                });
            }
        }

        private void AreaEntityUse(object sender, EntityUseEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityUseMessage(e));
            }
        }

        void AreaTransferMessage(object sender, ProtocolMessageEventArgs<ItemTransferMessage> e)
        {
            if (e.Message.SourceEntityId != DynamicEntity.DynamicId)
            {
                Connection.Send(e.Message);
            }
        }

        private void AreaOnVoxelModelChanged(object sender, ProtocolMessageEventArgs<EntityVoxelModelMessage> e)
        {
            if (e.Message.EntityLink.DynamicEntityId == DynamicEntity.DynamicId)
            {
                var charClass = _server.EntityFactory.Config.CharacterClasses.FirstOrDefault( c=> c.ClassName == e.Message.ClassName);

                if (charClass != null)
                    DynamicEntity.ModelName = charClass.ModelName;
            }

            Connection.Send(e.Message);
        }

        private void AreaEntityUseFeedback(object sender, EntityUseFeedbackEventArgs e)
        {
            // to be able to detect desync we need to send our toolimpact back and compare with local version
            Connection.Send(e.Message);
        }

        private void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityPositionMessage { 
                    EntityId = e.Entity.DynamicId, 
                    Position = e.Entity.Position 
                });
            }
        }

        private void AreaEntityView(object sender, EntityViewEventArgs e)
        {
            if (e.Entity != DynamicEntity)
            {
                Connection.Send(new EntityHeadDirectionMessage { 
                    EntityId = e.Entity.DynamicId, 
                    Rotation = e.Entity.HeadRotation 
                });
            }
        }

        private void AreaBlocksChanged(object sender, BlocksChangedEventArgs e)
        {
            // we no need to send message caused by the entity, because it is responsibility of the entity tool to update the world
            // the message will only be sent if change was done by 3rd party objects (services)

            if (e.SourceEntityId == 0)
            {
                Connection.Send(new BlocksChangedMessage
                {
                    BlockValues = e.BlockValues,
                    BlockPositions = e.GlobalLocations,
                    Tags = e.Tags
                });
            }
        }        
    }
}
