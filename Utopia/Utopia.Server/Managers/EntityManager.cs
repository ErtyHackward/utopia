using System.Collections.Generic;
using Utopia.Shared.Entities;
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
        private readonly Dictionary<StaticId, uint> _lockedStaticEntities = new Dictionary<StaticId, uint>();

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
                            var sid = new StaticId(e.Connection.ServerEntity.StaticEntityChunk, staticEntity.StaticId);
                            _lockedStaticEntities.Remove(sid);
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
            e.Connection.MessagePosition += ConnectionMessagePosition;
            e.Connection.MessageDirection += ConnectionMessageDirection;
            e.Connection.MessageEntityUse += ConnectionMessageEntityUse;
            e.Connection.MessageItemTransfer += ConnectionMessageItemTransfer;
            e.Connection.MessageEntityEquipment += ConnectionMessageEntityEquipment;
            e.Connection.MessageEntityLock += ConnectionMessageEntityLock;
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

        private void ConnectionMessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            var connection = sender as ClientConnection;
            if (connection != null && e.Message.EntityId == connection.ServerEntity.DynamicEntity.DynamicId)
            {
                connection.ServerEntity.DynamicEntity.Rotation = e.Message.Direction;
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
            
            if (e.Message.Lock)
            {
                // locking

                if (e.Message.IsStatic)
                {
                    #region Lock static entity
                    var chunk = _server.LandscapeManager.GetChunk(e.Message.ChunkPosition);
                    IStaticEntity staticEntity;
                    chunk.Entities.ContainsId(e.Message.EntityId, out staticEntity);

                    if (staticEntity == null)
                    {
                        connection.SendAsync(new EntityLockResultMessage
                        {
                            IsStatic = true,
                            ChunkPosition = e.Message.ChunkPosition,
                            EntityId = e.Message.EntityId,
                            LockResult = LockResult.NoSuchEntity
                        });
                        return;
                    }


                    var sid = new StaticId(chunk.Position, e.Message.EntityId);

                    lock (_lockedStaticEntities)
                    {
                        if (_lockedStaticEntities.ContainsKey(sid))
                        {
                            connection.SendAsync(new EntityLockResultMessage
                                                     {
                                                         IsStatic = true,
                                                         ChunkPosition = e.Message.ChunkPosition,
                                                         EntityId = e.Message.EntityId,
                                                         LockResult = LockResult.FailAlreadyLocked
                                                     });
                            return;
                        }

                        _lockedStaticEntities.Add(sid, connection.ServerEntity.DynamicEntity.DynamicId);

                        connection.ServerEntity.StaticEntityChunk = e.Message.ChunkPosition;
                        connection.ServerEntity.LockedEntity = staticEntity;
                        connection.SendAsync(new EntityLockResultMessage
                        {
                            IsStatic = true,
                            ChunkPosition = e.Message.ChunkPosition,
                            EntityId = e.Message.EntityId,
                            LockResult = LockResult.SuccessLocked
                        });
                        
                    }
                    #endregion
                }
                else
                {
                    #region Lock dynamic entity

                    lock (_lockedDynamicEntities)
                    {
                        if (_lockedDynamicEntities.ContainsKey(e.Message.EntityId))
                        {
                            connection.SendAsync(new EntityLockResultMessage
                                                     {
                                                         EntityId = e.Message.EntityId,
                                                         LockResult = LockResult.FailAlreadyLocked
                                                     });
                            return;
                        }

                        var dynEntity = _server.AreaManager.Find(e.Message.EntityId);

                        if (dynEntity != null)
                        {
                            _lockedDynamicEntities.Add(e.Message.EntityId,
                                                       connection.ServerEntity.DynamicEntity.DynamicId);

                            IEntity lockEntity = (Entity) dynEntity.DynamicEntity;

                            connection.ServerEntity.LockedEntity = lockEntity;
                            connection.SendAsync(new EntityLockResultMessage
                                                     {
                                                         EntityId = e.Message.EntityId,
                                                         LockResult = LockResult.SuccessLocked
                                                     });
                        }
                        else
                        {
                            connection.SendAsync(new EntityLockResultMessage
                                                     {
                                                         EntityId = e.Message.EntityId,
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

                if (e.Message.IsStatic)
                {
                    var chunk = _server.LandscapeManager.GetChunk(e.Message.ChunkPosition);
                    IStaticEntity staticEntity;
                    chunk.Entities.ContainsId(e.Message.EntityId, out staticEntity);
                    
                    if (staticEntity == null)
                        return;

                    var sid = new StaticId(chunk.Position, e.Message.EntityId);

                    lock (_lockedStaticEntities)
                    {
                        _lockedStaticEntities.Remove(sid);
                    }

                    connection.ServerEntity.LockedEntity = null;
                }
                else
                {
                    lock (_lockedDynamicEntities)
                    {
                        uint lockOwner;
                        if (_lockedDynamicEntities.TryGetValue(e.Message.EntityId, out lockOwner))
                        {
                            if (lockOwner == connection.ServerEntity.DynamicEntity.DynamicId)
                            {
                                _lockedDynamicEntities.Remove(e.Message.EntityId);
                                connection.ServerEntity.LockedEntity = null;
                            }
                        }
                    }
                }
            }

        }
    }
}
