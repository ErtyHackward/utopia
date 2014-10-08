using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class SaveCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "save"; }
        }

        public override string Description
        {
            get { return "Saves all modified chunks to the database"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            server.LandscapeManager.SaveChunks();
            connection.SendChat(string.Format("Saved {1} chunks. Time: {0} ms", server.LandscapeManager.SaveTime, server.LandscapeManager.ChunksSaved));
        }
    }
}