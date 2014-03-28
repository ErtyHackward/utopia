using System;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Commands
{
    public class DefaultRoleCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "defaultrole"; }
        }

        public override string Description
        {
            get { return "Sets starting role of the new user. Example: /defaultrole member"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            try
            {
                if (arguments == null || arguments.Length != 1)
                    return;

                var roleStr = arguments[0].ToLower();

                UserRole role;

                if (Enum.TryParse(roleStr, true, out role))
                {
                    server.CustomStorage.SetVariable("DefaultRole", role);
                    server.UsersStorage.DefaultRole = role;
                    connection.SendChat("Default role is updated");
                }
                else
                {
                    connection.SendChat("Error: Unknown role" );
                }
            }
            catch (Exception x)
            {
                connection.SendChat("Error: " + x.Message);
            }
        }
    }
}