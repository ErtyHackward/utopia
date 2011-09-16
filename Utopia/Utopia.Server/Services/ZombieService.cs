using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using Utopia.Server.Entities;

namespace Utopia.Server.Services
{
    public class ZombieService : Service
    {
        private Server _server;

        public override string ServiceName
        {
            get { return "Zombie"; }
        }

        public override void Initialize(Server server)
        {
            _server = server;
            // need to find a place to put entity to
            server.AreaManager.AddEntity(new ServerZombie(server, "Bob") 
            { 
                Position = new DVector3(40, 72, -60), 
                Blocks = new [,,] { { {(byte)15}} }, 
                Size = new SharpDX.Vector3(0.5f, 1.9f, 0.5f)
            });
        }
    }
}
