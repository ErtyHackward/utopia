using Utopia.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Server.Commands
{
    public class SetSpawnPointCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "setspawn"; }
        }

        public override string Description
        {
            get { return "Sets current player position as default start position for new players"; }
        }

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            server.CustomStorage.SetVariable("SpawnPosition", connection.ServerEntity.DynamicEntity.Position);
            connection.SendChat("Spawn point is set");
        }
    }
}