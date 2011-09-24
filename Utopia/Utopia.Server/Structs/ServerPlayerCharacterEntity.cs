using System;
using System.IO;
using Utopia.Net.Messages;
using Utopia.Server.Events;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerCharacterEntity : ServerDynamicEntity
    {
        
        public ClientConnection Connection { get; private set; }

        /// <summary>
        /// Creates new instance of Server player entity that translates Entity Object Model events to player via network
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        public ServerPlayerCharacterEntity(ClientConnection connection, DynamicEntity entity) : base(entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (entity == null) throw new ArgumentNullException("entity");
            Connection = connection;
        }

        public override void AddArea(MapArea area)
        {
            area.EntityView += AreaEntityView;
            area.EntityMoved += AreaEntityMoved;
            area.EntityUse += AreaEntityUse;
            area.BlocksChanged += AreaBlocksChanged;
            area.EntityModelChanged += AreaEntityModelChanged;

            foreach (var serverEntity in area.Enumerate())
            {
                if (serverEntity != this)
                {
                    //Console.WriteLine("TO: {0}, entity {1} in", Connection.Entity.EntityId, dynamicEntity.EntityId);
                    Connection.SendAsync(new EntityInMessage { Entity = serverEntity.DynamicEntity });
                }
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
                Connection.SendAsync(new EntityInMessage { Entity = e.Entity.DynamicEntity });
            }
        }

        public override void RemoveArea(MapArea area)
        {
            area.EntityView -= AreaEntityView;
            area.EntityMoved -= AreaEntityMoved;
            area.EntityUse -= AreaEntityUse;
            area.BlocksChanged -= AreaBlocksChanged;

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

    }
}
