using System;
using System.Collections.Generic;
using Utopia.Server.Commands;
using Utopia.Server.Entities;
using Utopia.Server.Events;
using Utopia.Server.Services;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities.Concrete;
using S33M3Resources.Structs;

namespace Utopia.Server.Sample
{
    /// <summary>
    /// Provides sample service to performs test
    /// </summary>
    public class TestNpcService : Service
    {
        private Server _server;
        private string[] _names = new[] { "Bob", "Ivan", "Steve", "Sayid", "Chuck", "Matvey", "Mattias", "George", "Master Yoda", "Homer" };

        private List<TestNpc> _aliveNpc = new List<TestNpc>();

        public override string ServiceName
        {
            get { return "Test NPC"; }
        }

        /// <summary>
        /// Creates new zombie and puts it at location specified 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public TestNpc CreateZombie(string name, Vector3D position)
        {
            var z = new Zombie { CharacterName = name };

            var zombie = new TestNpc(_server, z);
            
            zombie.DynamicEntity.Position = position;
            zombie.DynamicEntity.Size = new SharpDX.Vector3(1f, 1f, 1f);

            _aliveNpc.Add(zombie);
            _server.AreaManager.AddEntity(zombie);
            return zombie;
        }

        public override void Initialize(Server server)
        {
            _server = server;

            _server.CommandsManager.RegisterCommand(new AddTestNpcCommand());
            _server.CommandsManager.RegisterCommand(new RemoveTestNpcCommand());
            _server.CommandsManager.RegisterCommand(new ComeHereCommand());

            _server.CommandsManager.PlayerCommand += ServerPlayerCommand;
            var r = new Random();
            for (int i = 0; i < 1; i++)
            {
                var z = CreateZombie(r.Next(_names), new Vector3D(40, 72, -60)); //  new DVector3(r.Next(-200, 200), 125, r.Next(-200, 200));
                //z.MoveVector = new Vector2(r.Next(-100, 100) / 100f, r.Next(-100, 100) / 100f);
                z.Seed = r.Next(0, 100000);
            }
        }

        public override void Dispose()
        {
            _server.CommandsManager.PlayerCommand -= ServerPlayerCommand;
        }

        void ServerPlayerCommand(object sender, PlayerCommandEventArgs e)
        {
            
            var r = new Random(DateTime.Now.Millisecond);
            if (e.Command is AddTestNpcCommand)
            {
                if (e.HaveParameters)
                {
                    int count;
                    int.TryParse(e.Params[0], out count);

                    for (int i = 0; i < count; i++)
                    {
                        var z = CreateZombie(r.Next(_names), e.Connection.ServerEntity.DynamicEntity.Position);
                        z.Seed = r.Next(0, 100000);
                    }
                    _server.ChatManager.Broadcast(string.Format("{0} test NPC added ({1} total)", count, _aliveNpc.Count));
                }
                else
                {
                    var z = CreateZombie(r.Next(_names), e.Connection.ServerEntity.DynamicEntity.Position);
                    _server.ChatManager.Broadcast(string.Format("Test NPC {0} added {1}", z.DynamicEntity.DisplayName,
                                                                _aliveNpc.Count));
                }
            }

            if (e.Command is RemoveTestNpcCommand)
            {
                for (int i = _aliveNpc.Count - 1; i >= 0; i--)
                {
                    _server.AreaManager.RemoveEntity(_aliveNpc[i]);
                }
                _aliveNpc.Clear();

                _server.ChatManager.Broadcast("All test npc removed");
            }

            if (e.Command is ComeHereCommand)
            {
                var blockPos = _server.LandscapeManager.GetCursor(e.Connection.ServerEntity.DynamicEntity.Position);
                
                if (!blockPos.IsSolid() && blockPos.Down().IsSolid())
                {
                    foreach (var serverZombie in _aliveNpc)
                    {
                        serverZombie.Goto(blockPos.GlobalPosition);
                    }
                }
                else
                {
                    _server.ChatManager.Broadcast(string.Format("Error: you need to stay on solid block to use this command"));
                }
            }

        }
    }

    public class AddTestNpcCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "addtestnpc"; }
        }

        public override string Description
        {
            get { return "Adds new test NPC to current player position"; }
        }
    }

    public class RemoveTestNpcCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "removetestnpc"; }
        }

        public override string Description
        {
            get { return "Removes all test NPC from the world"; }
        }
    }

    public class ComeHereCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "comehere"; }
        }

        public override string Description
        {
            get { return "Tells every test npc to go to current player position"; }
        }
    }
}
