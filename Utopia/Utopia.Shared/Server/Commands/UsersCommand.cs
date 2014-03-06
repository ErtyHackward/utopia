using System.Linq;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Commands
{
    public class UsersCommand : IServerChatCommand
    {
        public string Id
        {
            get { return "users"; }
        }

        public string Description
        {
            get { return "Displays a list of online users"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            connection.SendChat(string.Format("Total {0} users", Enumerable.Count<ClientConnection>(server.ConnectionManager.Connections(), c => c.Authorized)));

            foreach (var group in Enumerable.Where<ClientConnection>(server.ConnectionManager.Connections(), c => c.Authorized).GroupBy(c => c.UserRole).OrderByDescending(g => g.Key))
            {
                if (!group.Any())
                    continue;

                switch (group.Key)
                {
                    case UserRole.Guest:
                        connection.SendChat("Guests:");
                        break;
                    case UserRole.Member:
                        connection.SendChat("Members:");
                        break;
                    case UserRole.Moderator:
                        connection.SendChat("Moderators:");
                        break;
                    case UserRole.Administrator:
                        connection.SendChat("Administrators:");
                        break;
                    default:
                        connection.SendChat("Other:");
                        break;
                }

                connection.SendChat(string.Join(", ", group.Select(c => c.DisplayName)));
            }
        }
    }
}