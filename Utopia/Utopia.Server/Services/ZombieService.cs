using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Entities;
using Utopia.Server.Events;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.ClassExt;

namespace Utopia.Server.Services
{
    public class ZombieService : Service
    {
        private Server _server;
        private string[] _names = new[] { "Bob", "Ivan", "Steve", "Sayid", "Chuck", "Matvey", "Mattias", "George", "Master Yoda", "Homer" };

        private List<ServerZombie> _aliveZombies = new List<ServerZombie>();

        public override string ServiceName
        {
            get { return "Zombie"; }
        }

        /// <summary>
        /// Creates new zombie and puts it at location specified 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ServerZombie CreateZombie(string name, DVector3 position)
        {
            var z = new Zombie { CharacterName = name, EntityId = EntityFactory.Instance.GetUniqueEntityId() };

            var zombie = new ServerZombie(_server, z);
            
            zombie.DynamicEntity.Position = position;
            zombie.DynamicEntity.Size = new SharpDX.Vector3(2f, 3f, 2f);

            zombie.DynamicEntity.Model = new VoxelModel();
            zombie.DynamicEntity.Model.Blocks = new byte[1, 1, 1];// { { { (byte)15 } } },
            zombie.DynamicEntity.Model.Blocks[0, 0, 0] = (byte)27;
            //zombie.Blocks[1, 0, 0] = (byte)0;
            //zombie.Blocks[0, 0, 1] = (byte)15;
            //zombie.Blocks[1, 0, 1] = (byte)0;
            //zombie.Blocks[0, 1, 0] = (byte)15;
            //zombie.Blocks[1, 1, 0] = (byte)0;
            //zombie.Blocks[0, 1, 1] = (byte)15;
            //zombie.Blocks[1, 1, 1] = (byte)0;
            //zombie.Blocks[0, 2, 0] = (byte)14;
            //zombie.Blocks[1, 2, 0] = (byte)15;
            //zombie.Blocks[0, 2, 1] = (byte)14;
            //zombie.Blocks[1, 2, 1] = (byte)15;
            _aliveZombies.Add(zombie);
            _server.AreaManager.AddEntity(zombie);
            return zombie;
        }

        public override void Initialize(Server server)
        {
            _server = server;

            _server.PlayerCommand += ServerPlayerCommand;
            var r = new Random();
            for (int i = 0; i < 400; i++)
            {
                var z = CreateZombie(r.Next(_names), new DVector3(r.Next(-200, 200), 125, r.Next(-200, 200))); //new DVector3(40, 72, -60));
                z.MoveVector = new Vector2(r.Next(-100, 100) / 100f, r.Next(-100, 100) / 100f);
                z.Seed = r.Next(0, 100000);
            }
        }

        public override void Dispose()
        {
            _server.PlayerCommand -= ServerPlayerCommand;
        }

        void ServerPlayerCommand(object sender, PlayerCommandEventArgs e)
        {
            var cmd = e.Command.ToLower();
            var r = new Random(DateTime.Now.Millisecond);
            if (cmd == "addzombie")
            {
                var z = CreateZombie(r.Next(_names), e.Connection.ServerEntity.DynamicEntity.Position);
                _server.SendChatMessage(string.Format("Zombie {0} added {1}", z.DynamicEntity.DisplayName, _aliveZombies.Count));
            }

            if (cmd == "removezombies")
            {
                for (int i = _aliveZombies.Count - 1; i >= 0; i--)
                {
                    _server.AreaManager.RemoveEntity(_aliveZombies[i]);
                }
                _aliveZombies.Clear();

                _server.SendChatMessage("All zombies removed");
            }

            if (cmd.StartsWith("addzombies"))
            {
                var splt = cmd.Split(' ');
                int count = 0;
                if (splt.Length == 2) int.TryParse(splt[1], out count);

                for (int i = 0; i < count; i++)
                {
                    var z = CreateZombie(r.Next(_names), e.Connection.ServerEntity.DynamicEntity.Position);
                    z.Seed = r.Next(0, 100000);
                }
                _server.SendChatMessage(string.Format("{0} zombies added ({1} total)", count, _aliveZombies.Count));
            }
        }
    }
}
