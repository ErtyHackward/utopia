using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Server.Commands
{
    public class WipeChunkCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "wipechunk"; }
        }

        public override string Description
        {
            get { return "Restores current chunk to its original state. All changes will be lost."; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            var chunkPos = BlockHelper.EntityToChunkPosition(connection.ServerEntity.DynamicEntity.Position);
            server.LandscapeManager.WipeChunk(chunkPos);
            connection.SendChat("Chunk is wiped");
        }
    }
}
