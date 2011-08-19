using System;

namespace Utopia.Server.Services
{
    /// <summary>
    /// Represents a game service
    /// </summary>
    public abstract class Service : IDisposable
    {
        /// <summary>
        /// Parent server
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        /// Gets service name
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// Creates new instance of the game service
        /// </summary>
        /// <param name="server"></param>
        protected Service(Server server)
        {
            Server = server;
        }

        /// <summary>
        /// Stops the service and releases all resources
        /// </summary>
        public virtual void Dispose()
        {
            
        }
    }
}
