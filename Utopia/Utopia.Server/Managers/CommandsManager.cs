using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Server.Commands;
using Utopia.Server.Events;
using Utopia.Server.Interfaces;
using Utopia.Server.Structs;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Managers
{
    public class CommandsManager
    {
        private readonly Server _server;

        private readonly Dictionary<string, IServerCommand> _commands = new Dictionary<string, IServerCommand>();

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

            RegisterCommand(new HelpCommand());
            RegisterCommand(new StatusCommand());
            RegisterCommand(new SaveCommand());
            RegisterCommand(new ServicesCommand());
            RegisterCommand(new SettimeCommand());
        }

        public void RegisterCommand(IServerCommand command)
        {
            if (_commands.ContainsKey(command.Id))
                throw new InvalidOperationException("Command with such id is already registered");

            _commands.Add(command.Id, command);
        }

        public bool TryExecute(ClientConnection connection, string msg)
        {
            if (msg[0] == '/')
            {
                // extract command and parameters
                var spaceIndex = msg.IndexOf(' ');
                string[] pars = null;
                string cmd;
                if (spaceIndex != -1)
                {
                    cmd = msg.Substring(1, spaceIndex - 1).ToLower();
                    pars = msg.Substring(spaceIndex + 1).Split(' ');

                    if (pars.Length == 0)
                        pars = null;
                }
                else
                {
                    cmd = msg.Substring(1).ToLower();
                }

                // extract the command instance
                IServerCommand command;
                if (!_commands.TryGetValue(cmd, out command))
                {
                    connection.SendAsync(new ChatMessage { Login = "server", Message = "Sorry, no such command." });
                    return true;
                }

                // check access
                if (command is IRoleRestrictedCommand)
                {
                    if (!(command as IRoleRestrictedCommand).HasAccess(connection.UserRole))
                    {
                        connection.SendAsync(new ChatMessage { Login = "server", Message = "Sorry, access denied." });
                        return true;
                    }
                }

                #region Help command
                if (command is HelpCommand)
                {
                    if (pars == null)
                    {
                        // enumerate all available commands
                        var result = (from p in _commands
                                      where !(p.Value is IRoleRestrictedCommand) || (p.Value as IRoleRestrictedCommand).HasAccess(connection.UserRole)
                                      select p.Value.Id).ToArray();

                        var commandsList = string.Join(", ", result);

                        connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Available commands: {0}.\nUse help [command_name] to get additioanl information about command", commandsList) });
                        
                        return true;
                    }

                    // show details about command
                    var cmdName = pars[0].ToLower();
                    IServerCommand c;
                    if (_commands.TryGetValue(cmdName, out c))
                    {
                        connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("{0} - {1}", cmdName, c.Description) });
                    }
                    else connection.SendAsync(new ChatMessage { Login = "server", Message = "No such command" });
                    
                    return true;
                }
                #endregion
                #region Status command
                if (command is StatusCommand)
                {
                    var sb = new StringBuilder();

                    sb.AppendFormat("Server is up for {0}\n", _server.PerformanceManager.UpTime.ToString(@"dd\.hh\:mm\:ss"));
                    sb.AppendFormat("Chunks in memory: {0}\n", _server.LandscapeManager.ChunksInMemory);
                    sb.AppendFormat("Entities count: {0}\n", _server.AreaManager.EntitiesCount);
                    sb.AppendFormat("Perfomance: CPU usage {1}%, Free RAM {2}Mb, DynamicUpdate {0} msec", _server.PerformanceManager.UpdateAverageTime, _server.PerformanceManager.CpuUsage, _server.PerformanceManager.FreeRAM);

                    connection.SendAsync(new ChatMessage { Login = "server", Message = sb.ToString() });
                    return true;
                }
                #endregion
                #region Save command
                if (command is SaveCommand)
                {
                    _server.LandscapeManager.SaveChunks();
                    connection.SendAsync(new ChatMessage { Login = "server", Message = string.Format("Saved {1} chunks. Time: {0} ms", _server.LandscapeManager.SaveTime, _server.LandscapeManager.ChunksSaved) });
                    return true;
                }
                #endregion
                #region Services command
                if (command is ServicesCommand)
                {
                    connection.SendAsync(new ChatMessage { Login = "server", Message = "Currenty active services: " + string.Join(", ", (from s in _server.Services select s.ServiceName)) });
                    
                    return true;
                }
                #endregion
                #region Settime command
                if (command is SettimeCommand)
                {
                    try
                    {
                        if (pars == null || pars.Length == 0)
                            return false;

                        var time = TimeSpan.Parse(pars[0]);

                        _server.Clock.SetCurrentTimeOfDay(time);
                        _server.ConnectionManager.Broadcast(new DateTimeMessage { DateTime = _server.Clock.Now, TimeFactor = _server.Clock.TimeFactor });
                        _server.ChatManager.Broadcast("Time updated by " + connection.Login );
                    }
                    catch (Exception ex)
                    {
                        if(ex is OverflowException || ex is FormatException)
                            connection.SendAsync(new ChatMessage { Login = "server", Message = "wrong time value, try 9:00 or 21:00" });
                        else throw;
                    }
                    return true;
                }
                #endregion

                OnPlayerCommand(new PlayerCommandEventArgs { Connection = connection, Command = command, Params = pars });
                return true;
            }
            return false;
        }
    }
}
