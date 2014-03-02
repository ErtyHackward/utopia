using Utopia.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Server.Commands
{
    public class MotdCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "motd"; }
        }

        public override string Description
        {
            get { return "Sets the message everyone receive when joining the server"; }
        }

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            server.SettingsManager.Settings.MessageOfTheDay = string.Join(" ", arguments);
            server.SettingsManager.Save();
            connection.SendChat("The message is updated");
        }
    }
}
