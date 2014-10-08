using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class WipeNpcsCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "wipenpcs"; }
        }

        public override string Description
        {
            get { return "Completely removes all npcs from the world"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            foreach (var mapArea in server.AreaManager.Areas())
            {
                foreach (var serverDynamicEntity in mapArea.Enumerate())
                {
                    server.EntityManager.RemoveNpc((CharacterEntity)serverDynamicEntity.DynamicEntity);
                }
            }
            connection.SendChat("Npcs were removed");
        }
    }
}