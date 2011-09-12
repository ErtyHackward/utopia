using System;
using Utopia.Net.Connections;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Entities;

namespace Utopia.Network
{
    /// <summary>
    /// Translates message from entity object model to the server and back
    /// </summary>
    public class EntityMessageTranslator
    {
        private readonly ServerConnection _connection;
        private readonly IEntityManager _entityManager;
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
        /// <param name="clientEntityManager"></param>
        public EntityMessageTranslator(Server connection, IDynamicEntity playerEntity, IEntityManager clientEntityManager)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _connection = connection.ServerConnection;

            //Handle Entity received Message from Server
            _connection.MessageEntityIn += ConnectionMessageEntityIn;
            _connection.MessageEntityOut += ConnectionMessageEntityOut;
            _connection.MessagePosition += ConnectionMessagePosition;
            _connection.MessageDirection += ConnectionMessageDirection;

            if (clientEntityManager == null) throw new ArgumentNullException("clientEntityManager");
            _entityManager = clientEntityManager;

            if (playerEntity == null) throw new ArgumentNullException("playerEntity");
            PlayerEntity = playerEntity;
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            _entityManager.GetEntityById(e.Message.EntityId).Rotation = e.Message.Direction;
        }

        void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
                _entityManager.GetEntityById(e.Message.EntityId).Position = e.Message.Position;
        }

        void ConnectionMessageEntityOut(object sender, ProtocolMessageEventArgs<EntityOutMessage> e)
        {
            _entityManager.RemoveEntityById(e.Message.EntityId);
        }

        void ConnectionMessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            _entityManager.AddEntity(e.Message.Entity);
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
