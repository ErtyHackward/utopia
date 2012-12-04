using System;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;

namespace Utopia.Network
{
    /// <summary>
    /// Translates messages from the entity object model to the server and back
    /// </summary>
    public class EntityMessageTranslator : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerComponent _server;
        private readonly IDynamicEntityManager _dynamicEntityManager;
        private readonly ILandscapeManager2D _landscapeManager;
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
        /// <param name="server"></param>
        /// <param name="playerEntity"></param>
        /// <param name="dynamicEntityManager"></param>
        /// <param name="landscapeManager"> </param>
        public EntityMessageTranslator(ServerComponent server, IDynamicEntity playerEntity, IDynamicEntityManager dynamicEntityManager, ILandscapeManager2D landscapeManager)
        {
            _server = server;

            //Handle Entity received Message from Server
            _server.MessageEntityIn += ServerMessageEntityIn;
            _server.MessageEntityOut += ServerMessageEntityOut;
            _server.MessagePosition += ServerMessagePosition; 
            _server.MessageDirection += ServerMessageDirection;
            _server.MessageEntityLock += ServerMessageEntityLock;

            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");
            if (landscapeManager == null) throw new ArgumentNullException("landscapeManager");
            if (playerEntity == null) throw new ArgumentNullException("playerEntity");

            _dynamicEntityManager = dynamicEntityManager;
            _landscapeManager = landscapeManager;
            PlayerEntity = playerEntity;
        }


        public void Dispose()
        {
            PlayerEntity = null;  

            _server.MessageEntityIn -= ServerMessageEntityIn;
            _server.MessageEntityOut -= ServerMessageEntityOut;
            _server.MessagePosition -= ServerMessagePosition;
            _server.MessageDirection -= ServerMessageDirection;
            _server.MessageEntityLock -= ServerMessageEntityLock;
        }

        //IN Going Server Data concering entities =================================================================
        void ServerMessageDirection(object sender, ProtocolMessageEventArgs<EntityHeadDirectionMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
                entity.HeadRotation = e.Message.Rotation;
            else
            {
                logger.Error("Unable to update direction of an entity");
            }
        }

        void ServerMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
                entity.Position = e.Message.Position;
            else
            {
                logger.Error("Unable to update Position of an entity");
            }
        }

        void ServerMessageEntityOut(object sender, ProtocolMessageEventArgs<EntityOutMessage> e)
        {
            switch (e.Message.EntityType)
            {
                case EntityType.Gear:
                    break;
                case EntityType.Block:
                    break;
                case EntityType.Static:
                    var cpos = e.Message.Link.ChunkPosition;
                    var chunk = _landscapeManager.GetChunk(cpos);
                    chunk.Entities.RemoveById(e.Message.EntityId);
                    break;
                case EntityType.Dynamic:
                    _dynamicEntityManager.RemoveEntityById(e.Message.EntityId);
                    break;
            }
        }

        void ServerMessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            switch (e.Message.Entity.Type)
            {
                case EntityType.Gear:
                    break;
                case EntityType.Block:
                    break;
                case EntityType.Static:
                    var cpos = e.Message.Link.ChunkPosition;
                    var chunk = _landscapeManager.GetChunk(cpos);
                    
                    // skip the message if the source is our entity (because we already have added the entity)
                    if (e.Message.SourceEntityId != PlayerEntity.DynamicId)
                    {
                        chunk.Entities.Add((StaticEntity)e.Message.Entity, e.Message.SourceEntityId);
                    }

                    break;
                case EntityType.Dynamic:
                    _dynamicEntityManager.AddEntity((IDynamicEntity) e.Message.Entity, true);
                    break;
            }
        }

        void ServerMessageEntityLock(object sender, ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            var link = e.Message.EntityLink;

            if (link.IsStatic)
            {
                var entity = e.Message.EntityLink.ResolveStatic(_landscapeManager);
                entity.Locked = e.Message.Lock;
            }
            else
            {
                var entity = _dynamicEntityManager.GetEntityById(link.DynamicEntityId);
                entity.Locked = e.Message.Lock;
            }
        }
        
        //OUT Going Server Data concering current player =================================================================
        //These are player event subscribing
        private void PlayerEntityUse(object sender, EntityUseEventArgs e)
        {
            _server.ServerConnection.SendAsync(new EntityUseMessage
            {
                IsEntityPicked = e.IsEntityPicked,
                IsBlockPicked = e.IsBlockPicked,
                DynamicEntityId = _playerEntity.DynamicId,
                NewBlockPosition = e.NewBlockPosition,
                PickedBlockPosition = e.PickedBlockPosition,
                PickedEntityLink = e.PickedEntityLink,
                PickedBlockFaceOffset = e.PickedBlockFaceOffset,
                ToolId = e.Tool == null ? 0 : e.Tool.StaticId
            });
        }

        private void PlayerEntityViewChanged(object sender, EntityViewEventArgs e)
        {
            _server.ServerConnection.SendAsync(new EntityHeadDirectionMessage
            {
                Rotation = e.Entity.HeadRotation,
                EntityId = e.Entity.DynamicId
            });
        }

        private void PlayerEntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            _server.ServerConnection.SendAsync(new EntityPositionMessage
            {
                Position = e.Entity.Position,
                EntityId = e.Entity.DynamicId
            });
        }
    }
}
