using System;
using System.Linq;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Server.Commands
{
    public class AdditemCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "additem"; }
        }

        public override string Description
        {
            get { return "Adds items to the inventory. Format: \"additem <blueprintid> [count=1]\" Example: additem 59 10"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            try
            {
                if (arguments == null || arguments.Length == 0)
                    return;

                ushort blueprintId;

                if (!ushort.TryParse(arguments[0], out blueprintId))
                {
                    var blockProfile =
                        server.EntityFactory.Config.BlockProfiles.FirstOrDefault(
                            bp => bp.Name.Equals(arguments[0], StringComparison.CurrentCultureIgnoreCase));

                    if (blockProfile != null)
                    {
                        blueprintId = blockProfile.Id;
                    }
                    else
                    {
                        var entity =
                            server.EntityFactory.Config.BluePrints.Values.FirstOrDefault(
                                v => string.Equals(v.Name.Replace(" ", ""), arguments[0],
                                    StringComparison.CurrentCultureIgnoreCase));

                        if (entity == null)
                        {
                            connection.SendChat("There is no such item.");
                            return;
                        }
                        blueprintId = entity.BluePrintId;
                    }
                }

                var count = arguments.Length == 2 ? int.Parse(arguments[1]) : 1;

                var charEntity = (CharacterEntity)connection.ServerEntity.DynamicEntity;

                var spawnEntity = server.EntityFactory.CreateFromBluePrint(blueprintId);

                var item = spawnEntity as IItem;

                if (item != null)
                {
                    charEntity.Inventory.PutItem(item, count);
                    connection.SendChat(string.Format("Item {0} was added to the inventory", item));
                }

                var npc = spawnEntity as Npc;

                if (npc != null)
                {
                    if (connection.ServerEntity.DynamicEntity.EntityState.IsBlockPicked)
                    {
                        npc.Position = new Vector3D(connection.ServerEntity.DynamicEntity.EntityState.PickPoint);
                        server.EntityManager.AddNpc(npc);
                        connection.SendChat(string.Format("Npc {0} was created", item));
                    }
                    else
                    {
                        connection.SendChat("Please pick a point to place the npc.");
                    }
                }

            }
            catch (Exception x)
            {
                connection.SendChat("Error: " + x.Message);
            }
        }
    }
}