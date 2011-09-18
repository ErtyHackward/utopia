using System;
using System.Collections.Generic;
using Utopia.Server.Services;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Performs server services management
    /// </summary>
    public class ServiceManager : IEnumerable<Service>
    {
        private readonly Server _parentServer;
        private readonly List<Service> _services = new List<Service>();

        public ServiceManager(Server parentServer)
        {
            if (parentServer == null) throw new ArgumentNullException("parentServer");
            _parentServer = parentServer;
        }

        public void Add(Service s)
        {
            _services.Add(s);
            s.Initialize(_parentServer);
        }

        public void Remove(Service s)
        {
            _services.Remove(s);
            s.Dispose();
        }


        public IEnumerator<Service> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _services.GetEnumerator();
        }
    }
}
