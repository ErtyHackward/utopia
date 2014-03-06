using Utopia.Shared.Net.Connections;
using Utopia.Shared.Services.Interfaces;

namespace Utopia.Shared.Server.Interfaces
{
    public interface IServerChatCommand : IChatCommand
    {
        void Execute(ServerCore server, ClientConnection connection, string[] arguments);
    }
}
