using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Interfaces
{
    public interface IConnectionManager
    {
        /// <summary>
        /// Sends a message to every connected and authorized client
        /// </summary>
        /// <param name="message">a message to send</param>
        void Broadcast(IBinaryMessage message);
    }
}