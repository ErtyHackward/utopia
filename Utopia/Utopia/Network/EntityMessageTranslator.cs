using System;
using System.Linq;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;

namespace Utopia.Network
{
    /// <summary>
    /// Translates messages from the entity object model to the server and back
    /// </summary>
    public class EntityMessageTranslator : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerComponent _server;
        private readonly IVisualDynamicEntityManager _dynamicEntityManager;
        private readonly IChunkEntityImpactManager _landscapeManager;
        private readonly SyncManager _syncManager;
        private ICharacterEntity _playerEntity;
        private bool _handlingUseMessage;

        private int _useToken;
        
        /// <summary>
        /// Gets or sets main player entity. All its events will be translated to the server
        /// </summary>
        public ICharacterEntity PlayerEntity
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
                        _playerEntity.Health.ValueChanged -= Health_ValueChanged;
                        _playerEntity.HealthStateChanged -= _playerEntity_HealthStateChanged;
                        _playerEntity.AfflictionStateChanged -= _playerEntity_AfflictionStateChanged;
                    }

                    _playerEntity = value;

                    if (_playerEntity != null)
                    {
                        _playerEntity.PositionChanged += PlayerEntityPositionChanged;
                        _playerEntity.ViewChanged += PlayerEntityViewChanged;
                        _playerEntity.Use += PlayerEntityUse;
                        _playerEntity.Health.ValueChanged += Health_ValueChanged;
                        _playerEntity.HealthStateChanged += _playerEntity_HealthStateChanged;
                        _playerEntity.AfflictionStateChanged += _playerEntity_AfflictionStateChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Creates new instance of EntityMessageTranslator
        /// </summary>
        public EntityMessageTranslator(
                ServerComponent server, 
                PlayerEntityManager playerEntityManager, 
                IVisualDynamicEntityManager dynamicEntityManager, 
                IChunkEntityImpactManager landscapeManager,
                SyncManager syncManager)
        {
            _server = server;

            //Handle Entity received Message from Server
            _server.MessageEntityIn += ServerMessageEntityIn;
            _server.MessageEntityOut += ServerMessageEntityOut;
            _server.MessagePosition += ServerMessagePosition; 
            _server.MessageDirection += ServerMessageDirection;
            _server.MessageEntityLock += ServerMessageEntityLock;
            _server.MessageEntityUse += _server_MessageEntityUse;
            _server.MessageEntityEquipment += _server_MessageEntityEquipment;
            _server.MessageUseFeedback += _server_MessageUseFeedback;
            _server.MessageItemTransfer += _server_MessageItemTransfer;
            _server.MessageEntityVoxelModel += ServerOnMessageEntityVoxelModel;
            _server.MessageEntityHealth += _server_MessageEntityHealth;
            _server.MessageEntityHealthState += _server_MessageEntityHealthState;
            _server.MessageEntityAfflictionState += _server_MessageEntityAfflictionState;

            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");
            if (landscapeManager == null) throw new ArgumentNullException("landscapeManager");
            if (syncManager == null) throw new ArgumentNullException("syncManager");
            if (playerEntityManager == null) throw new ArgumentNullException("playerEntityManager");

            _dynamicEntityManager = dynamicEntityManager;
            _landscapeManager = landscapeManager;
            _syncManager = syncManager;
            PlayerEntity = playerEntityManager.PlayerCharacter;
            playerEntityManager.PlayerEntityChanged += (sender, args) => { PlayerEntity = args.PlayerCharacter; };
        }


        public void Dispose()
        {
            PlayerEntity = null;  

            _server.MessageEntityIn -= ServerMessageEntityIn;
            _server.MessageEntityOut -= ServerMessageEntityOut;
            _server.MessagePosition -= ServerMessagePosition;
            _server.MessageDirection -= ServerMessageDirection;
            _server.MessageEntityLock -= ServerMessageEntityLock;
            _server.MessageEntityUse -= _server_MessageEntityUse;
            _server.MessageEntityEquipment -= _server_MessageEntityEquipment;
            _server.MessageUseFeedback -= _server_MessageUseFeedback;
            _server.MessageItemTransfer -= _server_MessageItemTransfer;
            _server.MessageEntityVoxelModel -= ServerOnMessageEntityVoxelModel;
            _server.MessageEntityHealth -= _server_MessageEntityHealth;
            _server.MessageEntityHealthState -= _server_MessageEntityHealthState;
            _server.MessageEntityAfflictionState -= _server_MessageEntityAfflictionState;
            
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
            if (e.Message.Link.IsDynamic)
            {
                _dynamicEntityManager.RemoveEntityById(e.Message.EntityId);
            }
            else
            {
                if (e.Message.TakerEntityId != PlayerEntity.DynamicId)
                {
                    _landscapeManager.ProcessMessageEntityOut(e);
                }
            }
        }

        void ServerMessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            if (e.Message.Link.IsDynamic)
            {
                _dynamicEntityManager.AddEntity((ICharacterEntity)e.Message.Entity, true);
            }
            else
            {
                // skip the message if the source is our entity (because we already have added the entity)
                if (e.Message.SourceEntityId != PlayerEntity.DynamicId)
                {
                    _landscapeManager.ProcessMessageEntityIn(e);
                }
            }
        }

        void ServerMessageEntityLock(object sender, ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            var link = e.Message.EntityLink;

            if (link.IsStatic)
            {
                var entity = e.Message.EntityLink.ResolveStatic(_landscapeManager);
                if (entity != null)
                    entity.Locked = e.Message.Lock;
            }
            else
            {
                var entity = _dynamicEntityManager.GetEntityById(link.DynamicEntityId);
                entity.Locked = e.Message.Lock;
            }
        }

        void _server_MessageEntityUse(object sender, ProtocolMessageEventArgs<EntityUseMessage> e)
        {
            var entity = (PlayerCharacter)_dynamicEntityManager.GetEntityById(e.Message.DynamicEntityId);
            
            if (entity != null)
            {
                entity.EntityState = e.Message.State;

                _handlingUseMessage = true;
                var impact = entity.ReplayUse(e.Message);
                _handlingUseMessage = false;

                if (impact.Dropped == false)
                {
                    // register other entity use message only if message was not dropped
                    _syncManager.RegisterUseMessage(e.Message, impact);
                }
                else
                {
                    logger.Debug("Impact dropped");
                }
            }
        }

        void _server_MessageItemTransfer(object sender, ProtocolMessageEventArgs<ItemTransferMessage> e)
        {
            var entity = (PlayerCharacter)_dynamicEntityManager.GetEntityById(e.Message.SourceEntityId);

            if (entity != null && entity != PlayerEntity)
            {
                // todo: handle desync on false
                entity.ReplayTransfer(e.Message);
            }
        }

        private void ServerOnMessageEntityVoxelModel(object sender, ProtocolMessageEventArgs<EntityVoxelModelMessage> e)
        {
            var entity = (PlayerCharacter)_dynamicEntityManager.GetEntityById(e.Message.EntityLink.DynamicEntityId);

            //Is the concerned entity being redered (Can be the player itself when in 3th person view or other dynamic entities
            if (entity != null)
            {
                var charClass = entity.EntityFactory.Config.CharacterClasses.FirstOrDefault(c => c.ClassName == e.Message.ClassName);
                if (charClass != null)
                {
                    _dynamicEntityManager.UpdateEntityVoxelBody(entity.DynamicId, charClass.ModelName);
                }
            }

            //Change the local Player default ModelName
            if (entity == null && e.Message.EntityLink.DynamicEntityId == PlayerEntity.DynamicId)
            {
                entity = (PlayerCharacter)PlayerEntity;
                var charClass = entity.EntityFactory.Config.CharacterClasses.FirstOrDefault(c => c.ClassName == e.Message.ClassName);
                if (charClass != null)
                {
                    entity.ModelName = charClass.ModelName;
                }
            }
        }

        void _server_MessageUseFeedback(object sender, ProtocolMessageEventArgs<UseFeedbackMessage> e)
        {
            _syncManager.RegisterFeedback(e.Message);
        }

        void _server_MessageEntityEquipment(object sender, ProtocolMessageEventArgs<EntityEquipmentMessage> e)
        {
            _dynamicEntityManager.UpdateEntity((ICharacterEntity)e.Message.Entity);
        }

        void _server_MessageEntityAfflictionState(object sender, ProtocolMessageEventArgs<EntityAfflictionStateMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
            {
                //update the health of the entity
                entity.Afflictions = e.Message.AfflictionState;
            }
        }

        void _server_MessageEntityHealthState(object sender, ProtocolMessageEventArgs<EntityHealthStateMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
            {
                //update the health of the entity
                entity.HealthState = e.Message.HealthState;
            }
        }

        void _server_MessageEntityHealth(object sender, ProtocolMessageEventArgs<EntityHealthMessage> e)
        {
            var entity = _dynamicEntityManager.GetEntityById(e.Message.EntityId);
            if (entity != null)
            {
                //update the health of the entity
                entity.Health.MaxValue = e.Message.Health.MaxValue;
                entity.HealthImpact(e.Message.Change);

                // sync, most of the time will do nothing
                //entity.Health.CurrentValue = e.Message.Health.CurrentValue;
            }
        }
        
        //OUT Going Server Data concering current player =================================================================
        //These are player event subscribing
        private void PlayerEntityUse(object sender, EntityUseEventArgs e)
        {
            var useMessage = new EntityUseMessage(e) { 
                Token = ++_useToken 
            };

            _syncManager.RegisterUseMessage(useMessage, e.Impact);
            _server.ServerConnection.Send(useMessage);
        }

        private void PlayerEntityViewChanged(object sender, EntityViewEventArgs e)
        {
            _server.ServerConnection.Send(new EntityHeadDirectionMessage
            {
                Rotation = e.Entity.HeadRotation,
                EntityId = e.Entity.DynamicId
            });
        }

        private void PlayerEntityPositionChanged(object sender, EntityMoveEventArgs e)
        {
            _server.ServerConnection.Send(new EntityPositionMessage
            {
                Position = e.Entity.Position,
                EntityId = e.Entity.DynamicId
            });
        }

        private void Health_ValueChanged(object sender, EnergyChangedEventArgs e)
        {
            // we don't need to send local player health caused by a tool use, only if health has been changed but another way (falling, drowning,...)
            if (_handlingUseMessage) 
                return;

            _server.ServerConnection.Send(new EntityHealthMessage
            {
                Health = e.EnergyChanged,
                Change = e.ValueChangedAmount,
                EntityId = e.EntityOwner
            });
        }

        private void _playerEntity_AfflictionStateChanged(object sender, EntityAfflicationStateChangeEventArgs e)
        {
            _server.ServerConnection.Send(new EntityAfflictionStateMessage
            {
                EntityId = e.DynamicEntity.DynamicId,
                AfflictionState = e.NewState
            });
        }

        private void _playerEntity_HealthStateChanged(object sender, EntityHealthStateChangeEventArgs e)
        {
            _server.ServerConnection.Send(new EntityHealthStateMessage
            {
                EntityId = e.DynamicEntity.DynamicId,
                HealthState = e.NewState
            });
        }
    }
}
