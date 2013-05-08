using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerEntity : ServerDynamicEntity
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected readonly Server _server;
        
        public ClientConnection Connection { get; private set; }

        /// <summary>
        /// Creates new instance of Server player entity that translates Entity Object Model events to player via network
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="server"></param>
        public ServerPlayerEntity(ClientConnection connection, DynamicEntity entity, Server server) : base(entity)
        {
            if (connection == null) 
                throw new ArgumentNullException("connection");
            if (entity == null) 
                throw new ArgumentNullException("entity");

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
            Connection.Send(new EntityOutMessage { EntityId = e.Entity.StaticId, TakerEntityId = e.SourceDynamicEntityId, EntityType = e.Entity.Type, Link = e.Entity.GetLink() });
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
    }
}
