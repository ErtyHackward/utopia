using Utopia.Shared.Services.Interfaces;

namespace Utopia.Server.Interfaces
{
    public interface IServerChatCommand : IChatCommand
    {
        void Execute(Server server, ClientConnection connection, string[] arguments);
    }
}
