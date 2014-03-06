using System.Text;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
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

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            var sb = new StringBuilder();

            sb.AppendFormat((string)"Server is up for {0}\n", (object)server.PerformanceManager.UpTime.ToString(@"dd\.hh\:mm\:ss"));
            sb.AppendFormat((string)"Chunks in memory: {0}\n", (object)server.LandscapeManager.ChunksInMemory);
            sb.AppendFormat((string)"Entities count: {0}\n", (object)server.AreaManager.EntitiesCount);
            sb.AppendFormat("Perfomance: CPU usage {1}%, Free RAM {2}Mb, DynamicUpdate {0} msec", server.PerformanceManager.UpdateAverageTime, server.PerformanceManager.CpuUsage, server.PerformanceManager.FreeRAM);

            connection.SendChat(sb.ToString());
        }
    }
}