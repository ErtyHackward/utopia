using System;
using System.Linq;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Server.Commands
{
    public class BanCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "ban"; }
        }

        public override string Description
        {
            get { return "Prevents users to join to the server. Example: ban userNickname 1d"; }
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
                connection.SendChat("Can't ban user with higher access level");
                return;
            }

            TimeSpan span;
            
            if (TimeSpanHelper.Parse(arguments[1], out span))
            {
                server.UsersStorage.AddBan(con.Login, span);
                con.Send(new ErrorMessage { ErrorCode = ErrorCodes.Banned, Message = "You were banned and kicked off the game" });
                con.Disconnect();
                server.ChatManager.Broadcast(arguments[0] + " was banned by " + connection.DisplayName);
            }
            else
            {
                connection.SendChat("Specify ban time. Example: ban userName 1d");
            }
        }
    }
}