using System.Collections.Generic;
using System.Linq;
using Utopia.Server.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Managers
{
    public class EntityManager
    {
        private readonly Server _server;
        private readonly Dictionary<uint, uint> _lockedEntities = new Dictionary<uint, uint>();

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
                    lock (_lockedEntities)
                    {
                        _lockedEntities.Remove(e.Connection.ServerEntity.LockedEntity.EntityId);
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
            var connection = (ClientConnection)sender;
            lock (_lockedEntities)
            {
                if (e.Message.Lock)
                {
                    if (_lockedEntities.ContainsKey(e.Message.EntityId))
                    {
                        connection.SendAsync(new EntityLockResultMessage { EntityId = e.Message.EntityId, LockResult = LockResult.FailAlreadyLocked });
                        return;
                    }

                    IEntity lockEntity = null;
                    IStaticEntity lockStatic = null;
                    if (!_server.LandscapeManager.SurroundChunks(connection.ServerEntity.DynamicEntity.Position).Any(chunk => chunk.Entities.ContainsId(e.Message.EntityId, out lockStatic)))
                    {
                        ServerDynamicEntity dynEntity;
                        if (_server.AreaManager.TryFind(e.Message.EntityId, out dynEntity))
                            lockEntity = (Entity)dynEntity.DynamicEntity;
                    }
                    else
                    {
                        lockEntity = lockStatic;
                    }

                    _lockedEntities.Add(e.Message.EntityId, connection.ServerEntity.DynamicEntity.DynamicId);
                    connection.ServerEntity.LockedEntity = lockEntity;
                    connection.SendAsync(new EntityLockResultMessage { EntityId = e.Message.EntityId, LockResult = LockResult.SuccessLocked });
                }
                else
                {
                    uint lockOwner;
                    if (_lockedEntities.TryGetValue(e.Message.EntityId, out lockOwner))
                    {
                        if (lockOwner == connection.ServerEntity.DynamicEntity.DynamicId)
                        {
                            _lockedEntities.Remove(e.Message.EntityId);
                            connection.ServerEntity.LockedEntity = null;
                        }
                    }
                }
            }
        }
    }
}
