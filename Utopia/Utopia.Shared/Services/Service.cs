using System;
using ProtoBuf;
using Utopia.Shared.Server;

namespace Utopia.Shared.Services
{
    /// <summary>
    /// Represents a game service. It can be anything game logic
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(NpcService))]
    [ProtoInclude(101, typeof(WaterDynamicService))]
    [ProtoInclude(102, typeof(WeatherService))]
    public abstract class Service : IDisposable
    {
        /// <summary>
        /// Stops the service and releases all resources
        /// </summary>
        public virtual void Dispose()
        {
            
        }

        public abstract void Initialize(ServerCore server);
    }
}
