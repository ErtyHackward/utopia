using System;
using System.Linq;
using Utopia.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Server.Commands
{
    public class SetroleCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "setrole"; }
        }

        public override string Description
        {
            get { return "Changes access level of the user. Format: \"setrole <usernickname> <role>\".\n Possible roles: admin, moderator, member "; }
        }

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            if (arguments == null || arguments.Length != 2)
                return;

            var login = server.ConnectionManager.Connections().Where(c => c.DisplayName.Replace(" ","").Equals(arguments[0], StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Login).FirstOrDefault() ?? arguments[0];

            var newRole = UserRole.Guest;

            switch (arguments[1])
            {
                case "admin":
                    newRole = UserRole.Administrator;
                    break;
                case "moderator":
                    newRole = UserRole.Moderator;
                    break;
                case "member":
                    newRole = UserRole.Member;
                    break;
                case "guest":
                    newRole = UserRole.Guest;
                    break;
            }

            var ownRole = server.UsersStorage.GetRole(connection.Login);

            if (newRole > ownRole)
            {
                connection.SendChat("You cannot set this role");
                return;
            }

            if (connection.UserRole == UserRole.Moderator)
            {
                var userRole = server.UsersStorage.GetRole(login);

                if (userRole > connection.UserRole)
                {
                    connection.SendChat("You cannot downgrade administrators");
                    return;
                }
            }

            var success = server.UsersStorage.SetRole(login, newRole);

            if (success)
            {
                connection.SendChat("User access level is updated");
            }
            else
            {
                connection.SendChat("Unable to update the user, check the login or role name");
            }
        }
    }
}