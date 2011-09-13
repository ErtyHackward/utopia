using System;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Management;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// This class sends all events from entity model to player
    /// </summary>
    public class ServerPlayerCharacterEntity : PlayerCharacter
    {
        public ClientConnection Connection { get; private set; }

        public override MapArea CurrentArea
        {
            get
            {
                return base.CurrentArea;
            }
            set
            {
                if (base.CurrentArea != value)
                {
                    if (base.CurrentArea != null)
                    {
                        base.CurrentArea.EntityInViewRange -= AreaEntityInViewRange;
                        base.CurrentArea.EntityOutOfViewRange -= AreaEntityOutOfViewRange;
                    }

                    base.CurrentArea = value;

                    if (base.CurrentArea != null)
                    {
                        base.CurrentArea.EntityInViewRange += AreaEntityInViewRange;
                        base.CurrentArea.EntityOutOfViewRange += AreaEntityOutOfViewRange;
                    }
                }
            }
        }

        /// <summary>
        /// Creates new instance of Server player entity that translates Entity Object Model events to player via network
        /// </summary>
        /// <param name="connection"></param>
        public ServerPlayerCharacterEntity(ClientConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            Connection = connection;
        }

        public override void AddArea(MapArea area)
        {
            area.EntityView += AreaEntityView;
            area.EntityMoved += AreaEntityMoved;
            area.EntityRemoved += AreaEntityRemoved;
            area.EntityAdded += AreaEntityAdded;
            area.EntityUse += AreaEntityUse;
            area.BlocksChanged += area_BlocksChanged;

            foreach (var entity in area.Enumerate())
            {
                Connection.Send(new EntityInMessage { Entity = entity });
            }
        }

        void AreaEntityOutOfViewRange(object sender, DynamicEntityEventArgs e)
        {
            if (e.Entity != this)
            {
                Connection.Send(new EntityOutMessage { EntityId = e.Entity.EntityId });
            }
        }

        void AreaEntityInViewRange(object sender, DynamicEntityEventArgs e)
        {
            if (e.Entity != this)
            {
                Connection.Send(new EntityInMessage { Entity = e.Entity });
            }
        }

        public override void RemoveArea(MapArea area)
        {
            area.EntityView -= AreaEntityView;
            area.EntityMoved -= AreaEntityMoved;
            area.EntityRemoved -= AreaEntityRemoved;
            area.EntityAdded -= AreaEntityAdded;
            area.EntityUse -= AreaEntityUse;
            area.BlocksChanged -= area_BlocksChanged;

            foreach (var dynamicEntity in area.Enumerate())
            {
                Connection.Send(new EntityOutMessage { EntityId = dynamicEntity.EntityId});
            }
        }

        void AreaEntityUse(object sender, EntityUseEventArgs e)
        {
            if (e.Entity != this)
            {
                Connection.Send(new EntityUseMessage 
                { 
                    EntityId = e.Entity.EntityId, 
                    NewBlockPosition = e.NewBlockPosition, 
                    PickedBlockPosition = e.PickedBlockPosition, 
                    PickedEntityId = e.PickedEntityId, 
                    SpaceVector = e.SpaceVector, 
                    ToolId = e.Tool.EntityId
                });
            }
        }

        void AreaEntityAdded(object sender, DynamicEntityEventArgs e)
        {

        }

        void AreaEntityRemoved(object sender, DynamicEntityEventArgs e)
        {

        }

        void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (e.Entity != this)
            {
                Connection.Send(new EntityPositionMessage { EntityId = e.Entity.EntityId, Position = e.Entity.Position});
            }
        }

        void AreaEntityView(object sender, EntityViewEventArgs e)
        {
            if (e.Entity != this)
            {
                Connection.Send(new EntityDirectionMessage { EntityId = e.Entity.EntityId, Direction = e.Entity.Rotation});
            }
        }

        void area_BlocksChanged(object sender, BlocksChangedEventArgs e)
        {
            Connection.Send(new BlocksChangedMessage { ChunkPosition = e.ChunkPosition, BlockValues = e.BlockValues, BlockPositions = e.Locations });
        }

    }
}
