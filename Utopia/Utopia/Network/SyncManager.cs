using System;
using System.Collections.Generic;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Network
{
    public class SyncManager
    {
        private readonly PlayerCharacter _player;
        private readonly IDynamicEntityManager _dynamicEntityManager;
        private readonly List<SyncItem> _syncItems = new List<SyncItem>();

        /// <summary>
        /// Occurs when we have desynchronization with the server
        /// </summary>
        public event EventHandler<DesyncEventArgs> DesyncDetected;

        protected virtual void OnDesyncDetected(DesyncEventArgs e)
        {
            if (e.ChunksToSynchronize == null)
                e.ChunksToSynchronize = new List<Vector3I>();

            if (e.EntitiesToSynchronize == null)
                e.EntitiesToSynchronize = new List<uint>();

            // we always need to resync our entity
            if (!e.EntitiesToSynchronize.Contains(_player.DynamicId))
                e.EntitiesToSynchronize.Add(_player.DynamicId);

            // find chunks to resync
            foreach (var entityId in e.EntitiesToSynchronize)
            {
                var entity = _dynamicEntityManager.FindEntity(new EntityLink(entityId));

                if (entity != null)
                {
                    var rootPos = BlockHelper.EntityToChunkPosition(entity.Position);

                    for (int x = -1; x < 2; x++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            e.ChunksToSynchronize.Add(rootPos + new Vector3I(x, 0, z));
                        }
                    }
                }
            }

            e.ChunksToSynchronize = e.ChunksToSynchronize.Distinct().ToList();

            var handler = DesyncDetected;
            if (handler != null) handler(this, e);
        }

        public SyncManager(PlayerCharacter player,
                           IDynamicEntityManager dynamicEntityManager,
                           ServerComponent server)
        {
            _player = player;
            _dynamicEntityManager = dynamicEntityManager;
            server.MessageError += _server_MessageError;
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