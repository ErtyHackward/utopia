using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using S33M3Resources.Structs;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.GUI;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;
using Utopia.Worlds.Chunks;

namespace Utopia.Network
{
    /// <summary>
    /// Allows to prevent desynchronizations in the game
    /// Desync can be detected by the server or the client
    /// 
    /// Server can detect it when it failed to perform ItemTransferOperation
    /// in such case ErrorMessage with ErrorCode == ErrorCodes.DesyncDetected will be sent
    /// 
    /// Client can detect the desync by caching UseMessages with local IToolImpcat and 
    /// comparing them to server responces
    /// 
    /// Local caching is done in RegisterUseMessage, validation is performed in RegisterFeedback
    /// 
    /// </summary>
    public class SyncManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IVisualDynamicEntityManager _dynamicEntityManager;
        private readonly ServerComponent _server;
        private readonly IWorldChunks _chunks;
        private readonly ChatComponent _chat;
        private readonly List<SyncItem> _syncItems = new List<SyncItem>();

        /// <summary>
        /// Occurs when we have desynchronization with the server
        /// </summary>
        public event EventHandler<DesyncEventArgs> DesyncDetected;

        protected virtual void OnDesyncDetected(DesyncEventArgs e)
        {
            PrepareEvent(e);

#if DEBUG
            _chat.AddMessage("Desync detected, fixing...");
#endif

            logger.Info("Desync detected, fixing...");

            foreach (var chunkPos in e.ChunksToSynchronize)
            {
                _chunks.ResyncChunk(chunkPos, true);
            }

            foreach (var entityId in e.EntitiesToSynchronize)
            {
                _server.ServerConnection.Send(new GetEntityMessage{ DynamicEntityId = entityId });
            }

            var handler = DesyncDetected;
            if (handler != null) handler(this, e);
        }
        
        [Inject]
        public PlayerEntityManager PlayerManager { get; set; }

        public SyncManager(IVisualDynamicEntityManager dynamicEntityManager,
                           ServerComponent server,
                           IWorldChunks chunks,
                           ChatComponent chat)
        {
            _dynamicEntityManager = dynamicEntityManager;
            _server = server;
            _chunks = chunks;
            _chat = chat;
            _server.MessageError += _server_MessageError;
            _server.MessageEntityData += server_MessageEntityData;
        }

        void server_MessageEntityData(object sender, Shared.Net.Connections.ProtocolMessageEventArgs<EntityDataMessage> e)
        {
            // update the entity
            if (e.Message.Entity != null)
            {
                var dynEntity = (ICharacterEntity)e.Message.Entity;

                _dynamicEntityManager.UpdateEntity(dynEntity);

                if (dynEntity.DynamicId == PlayerManager.PlayerCharacter.DynamicId)
                {
                    //These are properties not synchronized with server, need to keep the local one !
                    var prevDisplacementMode = PlayerManager.PlayerCharacter.DisplacementMode;
                    var staminaBckp = new Energy(PlayerManager.PlayerCharacter.Stamina);
                    var oxygenBckp = new Energy(PlayerManager.PlayerCharacter.Oxygen);

                    var playerChar = (PlayerCharacter)dynEntity;
                    playerChar.DisplacementMode = prevDisplacementMode;
                    playerChar.Stamina = staminaBckp;
                    playerChar.Oxygen = oxygenBckp;

                    PlayerManager.PlayerCharacter = playerChar;
                }
            }
        }

        void _server_MessageError(object sender, Shared.Net.Connections.ProtocolMessageEventArgs<ErrorMessage> e)
        {
            if (e.Message.ErrorCode == ErrorCodes.DesyncDetected)
            {
                OnDesyncDetected(new DesyncEventArgs());
            }
        }

        public void RegisterFeedback(UseFeedbackMessage message)
        {
            var item = _syncItems.FirstOrDefault(s => s.UseMessage.DynamicEntityId == message.OwnerDynamicId && s.UseMessage.Token == message.Token);

            if (item.UseMessage == null) 
                return;

            if (!item.ToolImpact.Equals(message.Impact))
            {
                OnDesyncDetected(new DesyncEventArgs { 
                    UseMessage = item.UseMessage,
                    EntitiesToSynchronize = new List<uint> { item.UseMessage.DynamicEntityId }
                });
            }

            _syncItems.Remove(item);
        }

        public void RegisterUseMessage(EntityUseMessage useMessage, IToolImpact localImpact)
        {
            _syncItems.Add(new SyncItem { 
                Added = DateTime.Now, 
                UseMessage = useMessage, 
                ToolImpact = localImpact
            });
        }

        public void Clear()
        {
            _syncItems.Clear();
        }

        private void PrepareEvent(DesyncEventArgs e)
        {
            if (e.ChunksToSynchronize == null)
                e.ChunksToSynchronize = new List<Vector3I>();

            if (e.EntitiesToSynchronize == null)
                e.EntitiesToSynchronize = new List<uint>();

            // we will resync our entity only if the source entity is close enough
            if (!e.EntitiesToSynchronize.Contains(PlayerManager.PlayerCharacter.DynamicId))
            {
                if (e.EntitiesToSynchronize.Count > 0)
                {
                    var entity = _dynamicEntityManager.GetEntityById(e.EntitiesToSynchronize[0]);

                    if (Vector3D.DistanceSquared(entity.Position, PlayerManager.PlayerCharacter.Position) < 1024) // (16*2)^2 = 1024 (2 chunks range)
                    {
                        e.EntitiesToSynchronize.Add(PlayerManager.PlayerCharacter.DynamicId);
                    }
                }
                else
                {
                    e.EntitiesToSynchronize.Add(PlayerManager.PlayerCharacter.DynamicId);
                }
            }

            // find chunks to resync
            foreach (var entityId in e.EntitiesToSynchronize)
            {
                var entity = entityId == PlayerManager.PlayerCharacter.DynamicId ? 
                    PlayerManager.PlayerCharacter : 
                    _dynamicEntityManager.FindEntity(new EntityLink(entityId));
                
                if (entity != null)
                {
                    var rootPos = BlockHelper.EntityToChunkPosition(entity.Position);

                    e.ChunksToSynchronize.Add(rootPos);

                    for (int x = -1; x < 2; x++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            if (x == 0 && z == 0)
                                continue;

                            e.ChunksToSynchronize.Add(rootPos + new Vector3I(x, 0, z));
                        }
                    }
                }
            }

            e.ChunksToSynchronize = e.ChunksToSynchronize.Distinct().ToList();
        }
    }

    public class DesyncEventArgs : EventArgs
    {
        public List<Vector3I> ChunksToSynchronize { get; set; }

        public List<uint> EntitiesToSynchronize { get; set; }

        public EntityUseMessage UseMessage { get; set; }
    }

    public struct SyncItem
    {
        /// <summary>
        /// Original message
        /// </summary>
        public EntityUseMessage UseMessage { get; set; }

        /// <summary>
        /// Local cached result
        /// </summary>
        public IToolImpact ToolImpact { get; set; }

        public DateTime Added { get; set; }
    }
}