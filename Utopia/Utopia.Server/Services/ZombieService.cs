using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Entities;
using Utopia.Shared.Chunks.Entities;

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

            string[] names = new[] { "Bob", "Ivan", "Steve", "Said", "Chuck", "Matvey", "Mattias", "George", "Master Yoda", "Homer" };

            Random r = new Random();

            foreach (var name in names)
            {
                var move = new Vector2(r.Next(-100, 100) / 100f, r.Next(-100, 100) / 100f);
                move.Normalize();
                //// need to find a place to put entity to
                //server.AreaManager.AddEntity(new ServerZombie(server, name)
                //{
                //    MoveVector = move,
                //    EntityId = EntityFactory.Instance.GetUniqueEntityId(),
                //    Position = new DVector3(40, 72, -60),
                //    Blocks = new[, ,] { { { (byte)15 } } },
                //    Size = new SharpDX.Vector3(0.5f, 1.9f, 0.5f)
                //});

                // need to find a place to put entity to
                var zomby = new ServerZombie(server, name)
                {
                    MoveVector = move,
                    EntityId = EntityFactory.Instance.GetUniqueEntityId(),
                    Position = new DVector3(40, 72, -60),
                    Size = new SharpDX.Vector3(2f, 3f, 2f)
                };

                zomby.Blocks = new byte[2, 3, 2];// { { { (byte)15 } } },
                zomby.Blocks[0, 0, 0] = (byte)15;
                zomby.Blocks[1, 0, 0] = (byte)0;
                zomby.Blocks[0, 0, 1] = (byte)15;
                zomby.Blocks[1, 0, 1] = (byte)0;
                zomby.Blocks[0, 1, 0] = (byte)15;
                zomby.Blocks[1, 1, 0] = (byte)0;
                zomby.Blocks[0, 1, 1] = (byte)15;
                zomby.Blocks[1, 1, 1] = (byte)0;
                zomby.Blocks[0, 2, 0] = (byte)14;
                zomby.Blocks[1, 2, 0] = (byte)15;
                zomby.Blocks[0, 2, 1] = (byte)14;
                zomby.Blocks[1, 2, 1] = (byte)15;

                server.AreaManager.AddEntity(zomby);
            }
        }
    }
}
