﻿using System;
using System.Collections.Generic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    public class EntityManager
    {
        private readonly Server _server;
        private readonly Dictionary<uint, uint> _lockedDynamicEntities = new Dictionary<uint, uint>();
        private readonly Dictionary<EntityLink, uint> _lockedStaticEntities = new Dictionary<EntityLink, uint>();

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

        public EntityManager(Server server)
        {
            _server = server;
            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;
        }

        void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessagePosition -= ConnectionMessagePosition;
            e.Connection.MessageDirection -= ConnectionMessageDirection;
            e.Connection.MessageEntityUse -= ConnectionMessageEntityUse;
            e.Connection.MessageItemTransfer -= ConnectionMessageItemTransfer;
            e.Connection.MessageEntityEquipment -= ConnectionMessageEntityEquipment;
            e.Connection.MessageEntityLock -= ConnectionMessageEntityLock;

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

        void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessagePosition        += ConnectionMessagePosition;
            e.Connection.MessageDirection       += ConnectionMessageDirection;
            e.Connection.MessageEntityUse       += ConnectionMessageEntityUse;
            e.Connection.MessageItemTransfer    += ConnectionMessageItemTransfer;
            e.Connection.MessageEntityEquipment += ConnectionMessageEntityEquipment;
            e.Connection.MessageEntityLock      += ConnectionMessageEntityLock;
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
        }

        private void ConnectionMessageEntityUse(object sender, ProtocolMessageEventArgs<EntityUseMessage> e)
        {
            // incoming use message by the player
            // handling entity using (tool or just use)

            var connection = (ClientConnection)sender;
            connection.ServerEntity.Use(e.Message);
        }

        private void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityHeadDirectionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.ServerEntity.DynamicEntity.DynamicId)
            {
                connection.ServerEntity.DynamicEntity.HeadRotation = e.Message.Rotation;
            }
        }

        private void ConnectionMessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.ServerEntity.DynamicEntity.DynamicId)
            {
                connection.ServerEntity.DynamicEntity.Position = e.Message.Position;
            }
        }

        private void ConnectionMessageEntityLock(object sender, ProtocolMessageEventArgs<EntityLockMessage> e)
        {
            var connection = (ClientConnection) sender;
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
    }
}
