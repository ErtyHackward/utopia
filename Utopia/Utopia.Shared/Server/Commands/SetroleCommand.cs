using System;
using System.Linq;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Commands
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

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (arguments == null || arguments.Length != 2)
                return;

            var login = Enumerable.Where<ClientConnection>(server.ConnectionManager.Connections(), c => c.DisplayName.Replace(" ","").Equals(arguments[0], StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Login).FirstOrDefault() ?? arguments[0];

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
                var con = Enumerable.FirstOrDefault<ClientConnection>(server.ConnectionManager.Connections(), c => c.Login == login);

                if (con != null)
                {
                    con.SendChat(string.Format("Your access level is updated by {1} to {0}.", newRole, connection.DisplayName));
                    con.UserRole = newRole;

                    var currentReadOnly = con.ServerEntity.DynamicEntity.IsReadOnly;
                    var newReadOnly = newRole == UserRole.Guest;

                    var currentFly = con.ServerEntity.DynamicEntity.CanFly;
                    var newFly = newRole == UserRole.Administrator;

                    var updateEntity = false;

                    if (currentReadOnly != newReadOnly)
                    {
                        con.ServerEntity.DynamicEntity.IsReadOnly = newReadOnly;
                        updateEntity = true;
                    }

                    if (currentFly != newFly)
                    {
                        con.ServerEntity.DynamicEntity.CanFly = newFly;
                        updateEntity = true;
                    }

                    if (updateEntity)
                    {
                        server.AreaManager.RemoveEntity(con.ServerEntity);
                        server.AreaManager.AddEntity(con.ServerEntity);

                        con.Send(new EntityDataMessage { Entity = con.ServerEntity.DynamicEntity });
                    }
                }

                connection.SendChat("User access level is updated");
            }
            else
            {
                connection.SendChat("Unable to update the user, check the login or role name");
            }
        }
    }
}