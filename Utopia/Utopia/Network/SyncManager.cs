using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;

namespace Utopia.Network
{
    public class SyncManager
    {
        private readonly List<SyncItem> _syncItems = new List<SyncItem>();

        /// <summary>
        /// Occurs when we have desynchronization with the server
        /// </summary>
        public event EventHandler<DesyncEventArgs> DesyncDetected;

        protected virtual void OnDesyncDetected(DesyncEventArgs e)
        {
            var handler = DesyncDetected;
            if (handler != null) handler(this, e);
        }

        public void RegisterFeedback(UseFeedbackMessage message)
        {
            var item = _syncItems.FirstOrDefault(s => s.UseMessage.DynamicEntityId == message.OwnerDynamicId && s.UseMessage.Token == message.Token);

            if (item.UseMessage == null) 
                return;

            if (!item.ToolImpact.Equals(message.Impact))
            {
                OnDesyncDetected(new DesyncEventArgs { UseMessage = item.UseMessage });
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