using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Server.Managers
{
    public class WeatherManager
    {
        private readonly Server _server;

        public WeatherManager(Server server)
        {
            _server = server;
        }
    }
}
