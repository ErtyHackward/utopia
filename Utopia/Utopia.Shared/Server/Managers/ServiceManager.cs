using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Managers
{
    /// <summary>
    /// Performs server services management
    /// </summary>
    public class ServiceManager : IEnumerable<Service>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _parentServer;
        private readonly List<Service> _services = new List<Service>();

        public ServiceManager(ServerCore parentServer)
        {
            if (parentServer == null) 
                throw new ArgumentNullException("parentServer");
            _parentServer = parentServer;
        }

        public void Initialize()
        {
            foreach (var service in _parentServer.WorldParameters.Configuration.Services)
            {
                Add(service);
            }
        }

        public void Add(Service s)
        {
            logger.Info("Activating {0} service...", s.GetType().Name);
            _services.Add(s);
            s.Initialize(_parentServer);
        }

        public void Remove(Service s)
        {
            _services.Remove(s);
            s.Dispose();
        }

        public T GetService<T>()  where T : Service
        {
            return _services.OfType<T>().FirstOrDefault();
        }

        public IEnumerator<Service> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        public void Dispose()
        {
            foreach (var service in _services)
            {
                service.Dispose();
            }
        }
    }
}
