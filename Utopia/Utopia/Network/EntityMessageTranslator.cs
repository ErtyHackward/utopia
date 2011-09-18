using System;
using System.Diagnostics;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Entities;
using Utopia.Entities.Managers;

namespace Utopia.Network
{
    /// <summary>
    /// Translates message from entity object model to the server and back
    /// </summary>
    public class EntityMessageTranslator
    {
        private readonly ServerConnection _connection;
        private readonly IDynamicEntityManager _dynamicEntityManager;
        private IDynamicEntity _playerEntity;

        /// <summary>
        /// Gets or sets main player entity. All its events will be translated to the server
        /// </summary>
        public IDynamicEntity PlayerEntity
        {
            get { return _playerEntity; }
            set {
                if (_playerEntity != value)
                {
                    if (_playerEntity != null)
                    {
                        _playerEntity.PositionChanged -= PlayerEntityPositionChanged;
                        _playerEntity.ViewChanged -= PlayerEntityViewChanged;
                        _playerEntity.Use -= PlayerEntityUse;
                    }

                    _playerEntity = value;

                    if (_playerEntity != null)
                    {
                        _playerEntity.PositionChanged += PlayerEntityPositionChanged;
                        _playerEntity.ViewChanged += PlayerEntityViewChanged;
                        _playerEntity.Use += PlayerEntityUse;
                    }
                }
            }
        }

        /// <summary>
        /// Creates new instance of EntityMessageTranslator
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="playerEntity"></param>
        /// <param name="dynamicEntityManager"></param>
        public EntityMessageTranslator(Server connection, IDynamicEntity playerEntity, IDynamicEntityManager dynamicEntityManager)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _connection = connection.ServerConnection;

            //Handle Entity received Message from Server
            _connection.MessageEntityIn += ConnectionMessageEntityIn;
            _connection.MessageEntityOut += ConnectionMessageEntityOut;
            _connection.MessagePosition += ConnectionMessagePosition;
            _connection.MessageDirection += ConnectionMessageDirection;

            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");
            _dynamicEntityManager = dynamicEntityManager;

            if (playerEntity == null) throw new ArgumentNullException("playerEntity");
            PlayerEntity = playerEntity;
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
                entity.Rotation = e.Message.Direction;
            else
            {
                Debug.WriteLine("Unable to update direction of an entity");
            }
        }

        void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
                entity.Position = e.Message.Position;
            else
            {
                Debug.WriteLine("Unable to update position of an entity");
            }
        }

        void ConnectionMessageEntityOut(object sender, ProtocolMessageEventArgs<EntityOutMessage> e)
        {
            _dynamicEntityManager.RemoveEntityById(e.Message.EntityId);
        }

        void ConnectionMessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            _dynamicEntityManager.AddEntity(e.Message.Entity);
        }

        private void PlayerEntityUse(object sender, EntityUseEventArgs e)
        {
            _connection.SendAsync(new EntityUseMessage 
            { 
                EntityId = _playerEntity.EntityId,
                NewBlockPosition = e.NewBlockPosition, 
                PickedBlockPosition = e.PickedBlockPosition, 
                PickedEntityId = e.PickedEntityId, 
                SpaceVector = e.SpaceVector, 
                ToolId = e.Tool == null ? 0 : e.Tool.EntityId 
            });
        }

        private void PlayerEntityViewChanged(object sender, EntityViewEventArgs e)
        {
            _connection.SendAsync(new EntityDirectionMessage 
            { 
                Direction = e.Entity.Rotation, 
                EntityId = e.Entity.EntityId 
            });
        }

        private void PlayerEntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            _connection.SendAsync(new EntityPositionMessage
            {
                Position = e.Entity.Position,
                EntityId = e.Entity.EntityId
            });
        }

    }
}
