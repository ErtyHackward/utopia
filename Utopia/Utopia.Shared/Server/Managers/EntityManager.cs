using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Events;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Server.Utils;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Managers
{
    public class EntityManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _server;
        private readonly Dictionary<uint, uint> _lockedDynamicEntities = new Dictionary<uint, uint>();
        private readonly Dictionary<EntityLink, uint> _lockedStaticEntities = new Dictionary<EntityLink, uint>();
        private readonly Dictionary<uint, ServerNpc> _npcs = new Dictionary<uint, ServerNpc>();
        private readonly Queue<ServerNpc> _npcToSave = new Queue<ServerNpc>();

        /// <summary>
        /// Occurs on success lock/unlock of an entity
        /// </summary>
        public event EventHandler<ProtocolMessageEventArgs<EntityLockMessage>> EntityLockChanged;

        public void OnEntityLockChanged(ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            if (e.Message.EntityLink.IsDynamic)
            {
                _server.ConnectionManager.Broadcast(e.Message);
            }

            var handler = EntityLockChanged;
            if (handler != null) handler(this, e);
        }

        public EntityManager(ServerCore server)
        {
            _server = server;
            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            _server.Scheduler.AddPeriodic(TimeSpan.FromSeconds(10), SaveEntities);
        }

        private void SaveEntities()
        {
            lock (_npcToSave)
            {
                using (new PerfLimit("NPC Save", 200))
                while (_npcToSave.Count > 0)
                {
                    _npcToSave.Dequeue().Save();
                }
            }
        }

        void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessagePosition              -= ConnectionMessagePosition;
            e.Connection.MessageDirection             -= ConnectionMessageDirection;
            e.Connection.MessageEntityUse             -= ConnectionMessageEntityUse;
            e.Connection.MessageItemTransfer          -= ConnectionMessageItemTransfer;
            e.Connection.MessageEntityEquipment       -= ConnectionMessageEntityEquipment;
            e.Connection.MessageEntityLock            -= ConnectionMessageEntityLock;
            e.Connection.MessageRequestDateTimeSync   -= ConnectionOnMessageRequestDateTimeSync;
            e.Connection.MessageGetEntity             -= ConnectionOnMessageGetEntity;
            e.Connection.MessageEntityVoxelModel      -= ConnectionOnMessageEntityVoxelModel;
            e.Connection.MessageEntityHealth          -= ConnectionOnMessageEntityHealth;
            e.Connection.MessageEntityHealthState     -= Connection_MessageEntityHealthState;
            e.Connection.MessageEntityAfflictionState -= Connection_MessageEntityAfflictionState;

            if (e.Connection.Authorized)
            {
                // unlocking entities that was locked
                if (e.Connection.ServerEntity.LockedEntity != null)
                {
                    if (e.Connection.ServerEntity.LockedEntity is IStaticEntity)
                    {
                        var staticEntity = e.Connection.ServerEntity.LockedEntity as IStaticEntity;
                        lock (_lockedStaticEntities)
                        {
                            _lockedStaticEntities.Remove(staticEntity.GetLink());
                        }
                    }
                    if (e.Connection.ServerEntity.LockedEntity is IDynamicEntity)
                    {
                        var dynamicEntity = e.Connection.ServerEntity.LockedEntity as IDynamicEntity;
                        lock (_lockedDynamicEntities)
                        {
                            _lockedDynamicEntities.Remove(dynamicEntity.DynamicId);
                        }
                    }


                    e.Connection.ServerEntity.LockedEntity = null;
                }
            }
        }

        private void ConnectionOnMessageEntityVoxelModel(object sender, ProtocolMessageEventArgs<EntityVoxelModelMessage> e)
        {
            var connection = (ClientConnection)sender;
            if (e.Message.EntityLink.DynamicEntityId != connection.ServerEntity.DynamicEntity.DynamicId)
                return;

            _server.AreaManager.GetArea(connection.ServerEntity.DynamicEntity.Position).OnEntityVoxelModel(e);
        }

        void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessagePosition              += ConnectionMessagePosition;
            e.Connection.MessageDirection             += ConnectionMessageDirection;
            e.Connection.MessageEntityUse             += ConnectionMessageEntityUse;
            e.Connection.MessageItemTransfer          += ConnectionMessageItemTransfer;
            e.Connection.MessageEntityEquipment       += ConnectionMessageEntityEquipment;
            e.Connection.MessageEntityLock            += ConnectionMessageEntityLock;
            e.Connection.MessageRequestDateTimeSync   += ConnectionOnMessageRequestDateTimeSync;
            e.Connection.MessageGetEntity             += ConnectionOnMessageGetEntity;
            e.Connection.MessageEntityVoxelModel      += ConnectionOnMessageEntityVoxelModel;
            e.Connection.MessageEntityHealth          += ConnectionOnMessageEntityHealth;
            e.Connection.MessageEntityHealthState     += Connection_MessageEntityHealthState;
            e.Connection.MessageEntityAfflictionState += Connection_MessageEntityAfflictionState;
        }

        private void Connection_MessageEntityAfflictionState(object sender, ProtocolMessageEventArgs<EntityAfflictionStateMessage> e)
        {
            var connection = (ClientConnection)sender;
            e.Message.EntityId = connection.ServerEntity.DynamicEntity.DynamicId;
            connection.ServerEntity.RetranslateMessage(e.Message);
        }

        private void Connection_MessageEntityHealthState(object sender, ProtocolMessageEventArgs<EntityHealthStateMessage> e)
        {
            var connection = (ClientConnection)sender;
            e.Message.EntityId = connection.ServerEntity.DynamicEntity.DynamicId;
            connection.ServerEntity.RetranslateMessage(e.Message);
        }

        private void ConnectionOnMessageEntityHealth(object sender, ProtocolMessageEventArgs<EntityHealthMessage> e)
        {
            var connection = (ClientConnection)sender;
            e.Message.EntityId = connection.ServerEntity.DynamicEntity.DynamicId;
            connection.ServerEntity.RetranslateMessage(e.Message);
        }

        private void ConnectionOnMessageGetEntity(object sender, ProtocolMessageEventArgs<GetEntityMessage> e)
        {
            var connection = (ClientConnection)sender;

            ServerDynamicEntity entity;
            
            if (_server.AreaManager.TryFind(e.Message.DynamicEntityId, out entity))
            {
                connection.Send(new EntityDataMessage { 
                    Entity = entity.DynamicEntity,
                    DynamicId = e.Message.DynamicEntityId
                });
            }
            else
            {
                connection.Send(new EntityDataMessage { 
                    Entity = null,
                    DynamicId = e.Message.DynamicEntityId
                });
            }
        }

        private void ConnectionOnMessageRequestDateTimeSync(object sender, ProtocolMessageEventArgs<RequestDateTimeSyncMessage> protocolMessageEventArgs)
        {
            var connection = (ClientConnection)sender;
            connection.Send(new DateTimeMessage { 
                DateTime = _server.Clock.Now, 
                TimeFactor = _server.Clock.TimeFactor 
            });
        }

        private void ConnectionMessageEntityEquipment(object sender, ProtocolMessageEventArgs<EntityEquipmentMessage> e)
        {
            var connection = (ClientConnection)sender;
            connection.ServerEntity.Equip(e.Message);
        }

        private void ConnectionMessageItemTransfer(object sender, ProtocolMessageEventArgs<ItemTransferMessage> e)
        {
            var connection = (ClientConnection)sender;
            connection.ServerEntity.ItemTransfer(e.Message);

            e.Message.SourceEntityId = connection.ServerEntity.DynamicEntity.DynamicId;

            // retranslate
            connection.ServerEntity.RetranslateMessage(e.Message);
        }

        private void ConnectionMessageEntityUse(object sender, ProtocolMessageEventArgs<EntityUseMessage> e)
        {
            // incoming use message by the player
            // handling entity using (tool or just use)

            var connection = (ClientConnection)sender;
            e.Message.DynamicEntityId = connection.ServerEntity.DynamicEntity.DynamicId;
            
            connection.ServerEntity.RetranslateMessage(e.Message);
            
            connection.ServerEntity.Use(e.Message);
        }

        private void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityHeadDirectionMessage> e)
        {
            var connection = (ClientConnection)sender;
            if (e.Message.EntityId == connection.ServerEntity.DynamicEntity.DynamicId)
            {
                connection.ServerEntity.DynamicEntity.HeadRotation = e.Message.Rotation;
            }
        }

        private void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var connection = (ClientConnection)sender;
            if (e.Message.EntityId == connection.ServerEntity.DynamicEntity.DynamicId)
            {
                connection.ServerEntity.DynamicEntity.Position = e.Message.Position;
            }
        }

        private void ConnectionMessageEntityLock(object sender, ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            var connection = (ClientConnection)sender;
            bool success = false;

            if (e.Message.Lock)
            {
                // locking
                if (e.Message.EntityLink.IsStatic)
                {
                    #region Lock static entity

                    var staticEntity = e.Message.EntityLink.ResolveStatic(_server.LandscapeManager);

                    if (staticEntity == null)
                    {
                        connection.Send(new EntityLockResultMessage
                        {
                            EntityLink = e.Message.EntityLink,
                            LockResult = LockResult.NoSuchEntity
                        });
                        return;
                    }

                    lock (_lockedStaticEntities)
                    {
                        if (_lockedStaticEntities.ContainsKey(e.Message.EntityLink))
                        {
                            connection.Send(new EntityLockResultMessage
                                                     {
                                                         EntityLink = e.Message.EntityLink,
                                                         LockResult = LockResult.FailAlreadyLocked
                                                     });
                            return;
                        }

                        _lockedStaticEntities.Add(e.Message.EntityLink, connection.ServerEntity.DynamicEntity.DynamicId);
                    }
                    
                    connection.ServerEntity.LockedEntity = staticEntity;
                    connection.Send(new EntityLockResultMessage
                    {
                        EntityLink = e.Message.EntityLink,
                        LockResult = LockResult.SuccessLocked
                    });
                    success = true;
                    #endregion
                }
                else
                {
                    #region Lock dynamic entity

                    lock (_lockedDynamicEntities)
                    {
                        if (_lockedDynamicEntities.ContainsKey(e.Message.EntityLink.DynamicEntityId))
                        {
                            connection.Send(new EntityLockResultMessage
                                                     {
                                                         EntityLink = e.Message.EntityLink,
                                                         LockResult = LockResult.FailAlreadyLocked
                                                     });
                            return;
                        }

                        var dynEntity = _server.AreaManager.Find(e.Message.EntityLink.DynamicEntityId);

                        if (dynEntity != null)
                        {
                            _lockedDynamicEntities.Add(e.Message.EntityLink.DynamicEntityId,
                                                       connection.ServerEntity.DynamicEntity.DynamicId);

                            var lockEntity = (IEntity)dynEntity.DynamicEntity;

                            connection.ServerEntity.LockedEntity = lockEntity;
                            connection.Send(new EntityLockResultMessage
                                                     {
                                                         EntityLink = e.Message.EntityLink,
                                                         LockResult = LockResult.SuccessLocked
                                                     });
                            
                            success = true;
                        }
                        else
                        {
                            connection.Send(new EntityLockResultMessage
                                                     {
                                                         EntityLink = e.Message.EntityLink,
                                                         LockResult = LockResult.NoSuchEntity
                                                     });
                        }

                    }

                    #endregion
                }
            }
            else
            {
                // unlocking    
                if (e.Message.EntityLink.IsStatic)
                {
                    #region unlock static
                    var staticEntity = e.Message.EntityLink.ResolveStatic(_server.LandscapeManager);
                    
                    if (staticEntity == null)
                        return;
                    
                    lock (_lockedStaticEntities)
                    {
                        uint lockOwner;
                        if (_lockedStaticEntities.TryGetValue(e.Message.EntityLink, out lockOwner))
                        {
                            if (lockOwner == connection.ServerEntity.DynamicEntity.DynamicId)
                            {
                                _lockedStaticEntities.Remove(e.Message.EntityLink);
                                connection.ServerEntity.LockedEntity = null;
                                success = true;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region unlock dynamic
                    lock (_lockedDynamicEntities)
                    {
                        uint lockOwner;
                        if (_lockedDynamicEntities.TryGetValue(e.Message.EntityLink.DynamicEntityId, out lockOwner))
                        {
                            if (lockOwner == connection.ServerEntity.DynamicEntity.DynamicId)
                            {
                                _lockedDynamicEntities.Remove(e.Message.EntityLink.DynamicEntityId);
                                connection.ServerEntity.LockedEntity = null;
                                success = true;
                            }
                        }
                    }
                    #endregion
                }
            }

            // retranslate success locks
            if (success)
                OnEntityLockChanged(e);
        }

        public ServerNpc AddNpc(CharacterEntity charEntity)
        {
            var npc = new ServerNpc(_server, charEntity);
            var id = DynamicIdHelper.GetNextUniqueId();
            npc.DynamicEntity.DynamicId = id;
            _server.AreaManager.AddEntity(npc);
            _npcs.Add(id, npc);

            charEntity.HealthStateChanged += charEntity_HealthStateChanged;
            charEntity.NeedSave += charEntity_NeedSave;

            return npc;
        }

        void charEntity_NeedSave(object sender, EventArgs e)
        {
            var npc = (CharacterEntity)sender;
            
            ServerDynamicEntity entity;
            if (_server.AreaManager.TryFind(npc.DynamicId, out entity))
            {
                lock (_npcToSave)
                {
                    _npcToSave.Enqueue((ServerNpc)entity);
                }
            }
            
        }

        void charEntity_HealthStateChanged(object sender, Entities.Events.EntityHealthStateChangeEventArgs e)
        {
            if (e.NewState == DynamicEntityHealthState.Dead)
            {
                var charEntity = (CharacterEntity)sender;

                charEntity.HealthStateChanged -= charEntity_HealthStateChanged;
                charEntity.NeedSave -= charEntity_NeedSave;
                _npcs.Remove(charEntity.DynamicId);
                _server.AreaManager.RemoveNpc(charEntity.DynamicId);
                _server.EntityStorage.RemoveEntity(charEntity.DynamicId);
            }
        }

        public void SaveAll()
        {
            logger.Info("Saving all npcs...");
            lock (_npcToSave)
            {
                foreach (var serverNpc in _npcs.Values)
                {
                    serverNpc.Save();
                }
            }
        }

        public void LoadNpcs()
        {
            logger.Info("Loading npcs...");

            foreach (var npc in _server.EntityStorage.AllEntities().OfType<Npc>())
            {
                AddNpc(npc);
            }
        }

        public void Dispose()
        {
            SaveAll();
        }
    }
}
