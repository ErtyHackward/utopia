using System;
using System.Linq;
using System.Text;
using Utopia.Server.Events;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Managers
{
    public class CommandsManager
    {
        private readonly Server _server;

        /// <summary>
        /// Occurs when users sends a command
        /// </summary>
        public event EventHandler<PlayerCommandEventArgs> PlayerCommand;

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            var handler = PlayerCommand;
            if (handler != null) handler(this, e);
        }

        public CommandsManager(Server server)
        {
            _server = server;
        }

        public bool TryExecute(ClientConnection connection, string msg)
        {
            if (msg[0] == '/')
            {
                if (msg == "/status")
                {
                    var sb = new StringBuilder();

                    sb.AppendFormat("Server is up for {0}\n", _server.PerformanceManager.UpTime.ToString(@"dd\.hh\:mm\:ss"));
                    sb.AppendFormat("Chunks in memory: {0}\n", _server.LandscapeManager.ChunksInMemory);
                    sb.AppendFormat("Entities count: {0}\n", _server.AreaManager.EntitiesCount);
                    sb.AppendFormat("Perfomance: CPU usage {1}%, Free RAM {2}Mb, DynamicUpdate {0} msec", _server.PerformanceManager.UpdateAverageTime, _server.PerformanceManager.CpuUsage, _server.PerformanceManager.FreeRAM);

                    connection.SendAsync(new ChatMessage { Login = "server", Message = sb.ToString() });
                    return true;
                }

                if (msg == "/save")
                {
                    _server.LandscapeManager.SaveChunks();
                    connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Saved {1} chunks. Time: {0} ms", _server.LandscapeManager.SaveTime, _server.LandscapeManager.ChunksSaved) });
                    return true;
                }

                if (msg.StartsWith("/services"))
                {
                    connection.SendAsync(new ChatMessage { Login = "server", Message = "Currenty active services: " + string.Join(", ", (from s in _server.Services select s.ServiceName)) });
                    
                    return true;
                }

                if (msg.StartsWith("/settime") && msg.Length > 9)
                {
                    try
                    {
                        var time = TimeSpan.Parse(msg.Remove(0, 9));

                        _server.Clock.SetCurrentTimeOfDay(time);
                        _server.ConnectionManager.Broadcast(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });
                        _server.ChatManager.Broadcast("Time updated by "+ connection.Login );
                    }
                    catch (OverflowException)
                    {
                        connection.SendAsync(new ChatMessage { Login = "server", Message = "wrong time value, try 9:00 or 21:00" });
                    }
                    return true;
                }

                OnPlayerCommand(new PlayerCommandEventArgs { Connection = connection, Command = msg.Remove(0, 1) });
                return true;
            }
            return false;
        }
    }
}
