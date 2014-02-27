using System;
using System.Linq;
using Utopia.Server.Interfaces;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Server.Commands
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

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            try
            {
                if (arguments == null || arguments.Length == 0)
                    return;

                ushort blueprintId;

                if (!ushort.TryParse(arguments[0], out blueprintId))
                {
                    var blockProfile = server.EntityFactory.Config.BlockProfiles.FirstOrDefault(
                        bp => bp.Name.Equals(arguments[0], StringComparison.CurrentCultureIgnoreCase)
                    );

                    if (blockProfile != null)
                    {
                        blueprintId = blockProfile.Id;
                    }
                    else
                    {
                        var entity =
                            server.EntityFactory.Config.BluePrints.Values.FirstOrDefault(
                                v => string.Equals(v.Name.Replace(" ", ""), arguments[0],
                                    StringComparison.CurrentCultureIgnoreCase)
                                );

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

                var item = (IItem)server.EntityFactory.CreateFromBluePrint(blueprintId);

                charEntity.Inventory.PutItem(item, count);
                connection.SendChat(string.Format("Item {0} was added to the inventory", item));
            }
            catch (Exception x)
            {
                connection.SendChat("Error: " + x.Message);
            }
        }
    }
}