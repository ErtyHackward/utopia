using System.Linq;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services.Interfaces;

namespace Utopia.Shared.Server.Commands
{
    public class HelpCommand : IServerChatCommand
    {
        public string Id
        {
            get { return "help"; }
        }

        public string Description
        {
            get { return "Provides command list and help information about commands. Use help {command_name} to get details about the command"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (arguments == null)
            {
                // enumerate all available commands
                var result = Enumerable.ToArray<string>((from p in server.CommandsManager.Commands
                                  where !(p is IRoleRestrictedCommand) || (p as IRoleRestrictedCommand).HasAccess(connection.UserRole)
                                  select p.Id));

                var commandsList = string.Join(", ", result);

                connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = string.Format("Available commands: {0}.\nUse help [command_name] to get additioanl information about command", commandsList) });

                return;
            }

            // show details about command
            var cmdName = arguments[0].ToLower();
            IChatCommand c = Enumerable.FirstOrDefault<IChatCommand>(server.CommandsManager.Commands, cmd => cmd.Id == cmdName);
            if (c != null)
            {
                connection.SendChat(string.Format("{0} - {1}", cmdName, c.Description));
            }
            else connection.SendChat("No such command");
        }
    }

}
