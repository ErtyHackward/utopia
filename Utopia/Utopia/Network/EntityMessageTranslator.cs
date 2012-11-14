using System;
using System.Diagnostics;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks;
using System.Linq;

namespace Utopia.Network
{
    /// <summary>
    /// Translates messages from the entity object model to the server and back
    /// </summary>
    public class EntityMessageTranslator : IDisposable
    {
        private readonly ServerComponent _server;
        private readonly IDynamicEntityManager _dynamicEntityManager;
        private readonly IWorldChunks _chunkManager;
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
        /// <param name="chunkManager"></param>
        public EntityMessageTranslator(ServerComponent server, IDynamicEntity playerEntity, IDynamicEntityManager dynamicEntityManager, IWorldChunks chunkManager)
        {
            _server = server;

            //Handle Entity received Message from Server
            _server.MessageEntityIn += ConnectionMessageEntityIn;
            _server.MessageEntityOut += ConnectionMessageEntityOut;
            _server.MessagePosition += ConnectionMessagePosition;
            _server.MessageDirection += ConnectionMessageDirection;

            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");
            _dynamicEntityManager = dynamicEntityManager;

            if (chunkManager == null) throw new ArgumentNullException("chunkManager");
            _chunkManager = chunkManager;

            if (playerEntity == null) throw new ArgumentNullException("playerEntity");
            PlayerEntity = playerEntity;
        }

        public void Dispose()
        {
            PlayerEntity = null;  

            _server.MessageEntityIn -= ConnectionMessageEntityIn;
            _server.MessageEntityOut -= ConnectionMessageEntityOut;
            _server.MessagePosition -= ConnectionMessagePosition;
            _server.MessageDirection -= ConnectionMessageDirection;
        }

        void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityHeadDirectionMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
                entity.HeadRotation = e.Message.Rotation;
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
            switch (e.Message.EntityType)
            {
                case EntityType.Gear:
                    break;
                case EntityType.Block:
                    break;
                case EntityType.Static:
                    var cpos = e.Message.Link.ChunkPosition;
                    var chunk = _chunkManager.GetChunk(cpos.X * AbstractChunk.ChunkSize.X, cpos.Y * AbstractChunk.ChunkSize.Z);
                    chunk.Entities.RemoveById(e.Message.EntityId);
                    break;
                case EntityType.Dynamic:
                    _dynamicEntityManager.RemoveEntityById(e.Message.EntityId);
                    break;
                default:
                    break;
            }
        }

        void ConnectionMessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            switch (e.Message.Entity.Type)
            {
                case EntityType.Gear:
                    break;
                case EntityType.Block:
                    break;
                case EntityType.Static:
                    var cpos = e.Message.Link.ChunkPosition;
                    var chunk = _chunkManager.GetChunk(cpos.X * AbstractChunk.ChunkSize.X, cpos.Y * AbstractChunk.ChunkSize.Z);
                    
                    // skip the message if the source is our entity (because we already have added the entity)
                    if (e.Message.SourceEntityId != PlayerEntity.DynamicId)
                    {
                        chunk.Entities.Add((IStaticEntity)e.Message.Entity, e.Message.SourceEntityId);
                    }

                    break;
                case EntityType.Dynamic:
                    _dynamicEntityManager.AddEntity((IDynamicEntity) e.Message.Entity);
                    break;
            }
        }

        private void PlayerEntityUse(object sender, EntityUseEventArgs e)
        {
            _server.ServerConnection.SendAsync(new EntityUseMessage
            {
                IsEntityPicked = e.IsEntityPicked,
                IsBlockPicked = e.IsBlockPicked,
                DynamicEntityId = _playerEntity.DynamicId,
                NewBlockPosition = e.NewBlockPosition,
                PickedBlockPosition = e.PickedBlockPosition,
                PickedEntityId = e.PickedEntityLink,
                ToolId = e.Tool == null ? 0 : e.Tool.StaticId,
                UseMode = e.UseMode
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
