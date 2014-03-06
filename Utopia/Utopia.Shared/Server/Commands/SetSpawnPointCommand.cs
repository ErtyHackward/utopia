using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
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

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            server.CustomStorage.SetVariable("SpawnPosition", connection.ServerEntity.DynamicEntity.Position);
            connection.SendChat("Spawn point is set");
        }
    }
}