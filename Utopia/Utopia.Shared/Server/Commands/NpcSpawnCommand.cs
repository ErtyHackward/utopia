using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class NpcSpawnCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "npcspawn"; }
        }

        public override string Description
        {
            get { return "allows to enable/disable npc spawn (true/false)"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (arguments.Length == 1)
            {
                bool value;
                if (!bool.TryParse(arguments[0], out value))
                {
                    connection.SendChat("Invalid argument, try 'true' or 'false'");
                    return;
                }
                else
                {
                    server.EntitySpawningManager.DisableNPCSpawn = !value;
                }
            }

            connection.SendChat(string.Format("Npc spawn is {0}", server.EntitySpawningManager.DisableNPCSpawn ? "disabled" : "enabled"));
        }
    }
}