using System.Linq;
using Utopia.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Server.Commands
{
    public class ServicesCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "services"; }
        }

        public override string Description
        {
            get { return "Lists all active server services"; }
        }

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            connection.SendChat("Currenty active services: " + string.Join(", ", (from s in server.Services select s.GetType().Name)));
        }
    }
}