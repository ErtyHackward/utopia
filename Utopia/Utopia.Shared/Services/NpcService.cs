using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Server;
using Container = Utopia.Shared.Entities.Concrete.Container;

namespace Utopia.Shared.Services
{
    /// <summary>
    /// Provides sample service to performs test
    /// </summary>
    [ProtoContract]
    public class NpcService : Service
    {
        private ServerCore _server;
        private string[] _names = new[] { "Bob", "Ivan", "Steve", "Sayid", "Chuck", "Matvey", "Mattias", "George", "Master Yoda", "Homer" };
        //private string[] _names = new[] { "Katia", "Sveta", "Lena", "Dasha" };

        private List<INpc> _aliveNpc = new List<INpc>();

        [ProtoMember(1, OverwriteList = true)]
        [Description("Initial start stuff")]
        public List<InitSlot> StartItems { get; set; }

        public NpcService()
        {
            StartItems = new List<InitSlot>();
        }

        /// <summary>
        /// Creates new zombie and puts it at location specified 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public INpc CreateNpc(Npc npc, Vector3D position)
        {
            var collector = _server.WorldParameters.Configuration.BluePrints.Select(p => p.Value).FirstOrDefault(t => t is BasicCollector);

            if (collector != null)
            {
                var item = (BasicCollector)collector.Clone();
                _server.EntityFactory.PrepareEntity(item);
                npc.Inventory.PutItem(item);
            }

            npc.Position = position;
            var srvNpc = _server.EntityManager.AddNpc(npc);
            _aliveNpc.Add(srvNpc);
            return srvNpc;
        }

        public override void Initialize(ServerCore server)
        {
            _server = server;

            _server.CommandsManager.RegisterCommand(new AddTestNpcCommand());
            _server.CommandsManager.RegisterCommand(new RemoveTestNpcCommand());

            _server.CommandsManager.PlayerCommand += ServerPlayerCommand;

            Faction faction;

            if (_server.GlobalStateManager.GlobalState.Factions.Count == 0)
            {
                faction = new Faction { Name = "Player faction", FactionId = 1 };
                _server.GlobalStateManager.GlobalState.Factions.Add(faction);
            }
            else
            {
                faction = _server.GlobalStateManager.GlobalState.Factions[1];
            }

            int move = 0;
            var r = new Random();

            foreach (var startItem in StartItems)
            {
                for (int i = 0; i < startItem.Count; i++)
                {
                    var item = _server.EntityFactory.CreateFromBluePrint(startItem.BlueprintId);

                    if (!string.IsNullOrEmpty(startItem.SetName))
                    {
                        var container = (Container)item;
                        _server.EntityFactory.FillContainer(startItem.SetName, container.Content);
                    }

                    if (item is Npc)
                    {
                        var n = (Npc)item;

                        if (!(item is Animal))
                        {
                            n.Name = r.Next(_names);
                        }
                        
                        var npc = CreateNpc(n, _server.LandscapeManager.GetHighestPoint(new Vector3D(-50 + move, 72, 30)));
                        npc.Faction = faction;
                    }
                    else if (item is StaticEntity)
                    {
                        var staticEntity = (StaticEntity)item;
                        staticEntity.FactionId = faction.FactionId;

                        var pos = _server.LandscapeManager.GetHighestPoint(new Vector3D(-50 + move, 72, 30));

                        var cursor = _server.LandscapeManager.GetCursor(pos);

                        var chunk = _server.LandscapeManager.GetChunkFromBlock(new Vector3I(-50 + move, 72, 30));

                        if (!chunk.Entities.EnumerateFast().Any(e => e.Position == pos))
                        {
                            staticEntity.Position = pos + new Vector3D(0.5f, 0, 0.5f);

                            var ble = staticEntity as IBlockLinkedEntity;

                            if (ble != null)
                            {
                                ble.LinkedCube = (Vector3I)staticEntity.Position + new Vector3I(0, -1, 0);
                            }

                            cursor.AddEntity(staticEntity);
                            faction.Stuff.Add(staticEntity.GetLink());
                        }
                    }

                    move++;
                }
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
                        //var z = CreateNpc(r.Next(_names), e.PlayerEntity.Position);
                    }
                    _server.ChatManager.Broadcast(string.Format("{0} test NPC added ({1} total)", count, _aliveNpc.Count));
                }
                else
                {
                    //var z = CreateNpc(r.Next(_names), e.PlayerEntity.Position);
                    //_server.ChatManager.Broadcast(string.Format("Test NPC {0} added {1}", z.Character.Name, _aliveNpc.Count));
                }
            }

            if (e.Command is RemoveTestNpcCommand)
            {
                for (int i = _aliveNpc.Count - 1; i >= 0; i--)
                {
                    _server.AreaManager.RemoveNpc(_aliveNpc[i]);
                }
                _aliveNpc.Clear();

                _server.ChatManager.Broadcast("All test npc removed");
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
}
