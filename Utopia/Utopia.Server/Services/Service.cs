using System;

namespace Utopia.Server.Services
{
    /// <summary>
    /// Represents a game service. It can be anything game logic
    /// </summary>
    public abstract class Service : IDisposable
    {
        /// <summary>
        /// Gets service name
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// Stops the service and releases all resources
        /// </summary>
        public virtual void Dispose()
        {
            
        }

        public abstract void Initialize(Server server);
    }
}
