using System.Text;
using Utopia.Server.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Services;

namespace Utopia.Server.Commands
{
    /// <summary>
    /// Server command for status information
    /// </summary>
    public class StatusCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "status"; }
        }

        public override string Description
        {
            get { return "Returns server overall statistics."; }
        }

        public void Execute(Server server, ClientConnection connection, string[] arguments)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Server is up for {0}\n", server.PerformanceManager.UpTime.ToString(@"dd\.hh\:mm\:ss"));
            sb.AppendFormat("Chunks in memory: {0}\n", server.LandscapeManager.ChunksInMemory);
            sb.AppendFormat("Entities count: {0}\n", server.AreaManager.EntitiesCount);
            sb.AppendFormat("Perfomance: CPU usage {1}%, Free RAM {2}Mb, DynamicUpdate {0} msec", server.PerformanceManager.UpdateAverageTime, server.PerformanceManager.CpuUsage, server.PerformanceManager.FreeRAM);

            connection.SendChat(sb.ToString());
        }
    }
}