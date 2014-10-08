using System;
using System.Linq;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class KickCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "kick"; }
        }

        public override string Description
        {
            get { return "Kicks user from the server. Example: kick userName"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
                return;

            var con = Enumerable.FirstOrDefault<ClientConnection>(server.ConnectionManager.Connections(), c => c.DisplayName.Replace(" ", "")
                .Equals(arguments[0], StringComparison.CurrentCultureIgnoreCase));

            if (con == null)
            {
                connection.SendChat("Can't find such user");
                return;
            }

            if (connection.UserRole < con.UserRole)
            {
                connection.SendChat("Can't kick user with higher access level");
                return;
            }

            con.Send(new ErrorMessage { ErrorCode = ErrorCodes.Kicked, Message = "You were kicked off the game" });
            con.Disconnect();
            server.ChatManager.Broadcast(arguments[0] + " was kicked by " + connection.DisplayName);
        }
    }
}