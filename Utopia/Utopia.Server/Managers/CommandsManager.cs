using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Services;
using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    public class CommandsManager : ICommandsManager
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
            RegisterCommand(new AdditemCommand());
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
                    connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "Sorry, no such command." });
                    return true;
                }

                // check access
                if (command is IRoleRestrictedCommand && connection.UserRole != UserRole.Administrator)
                {
                    if (!(command as IRoleRestrictedCommand).HasAccess(connection.UserRole))
                    {
                        connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "Sorry, access denied." });
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

                        connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = string.Format("Available commands: {0}.\nUse help [command_name] to get additioanl information about command", commandsList) });
                        
                        return true;
                    }

                    // show details about command
                    var cmdName = pars[0].ToLower();
                    IServerCommand c;
                    if (_commands.TryGetValue(cmdName, out c))
                    {
                        connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = string.Format("{0} - {1}", cmdName, c.Description) });
                    }
                    else connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "No such command" });
                    
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

                    connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = sb.ToString() });
                    return true;
                }
                #endregion
                #region Save command
                if (command is SaveCommand)
                {
                    _server.LandscapeManager.SaveChunks();
                    connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = string.Format("Saved {1} chunks. Time: {0} ms", _server.LandscapeManager.SaveTime, _server.LandscapeManager.ChunksSaved) });
                    return true;
                }
                #endregion
                #region Services command
                if (command is ServicesCommand)
                {
                    connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "Currenty active services: " + string.Join(", ", (from s in _server.Services select s.GetType().Name)) });
                    
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
                        _server.ChatManager.Broadcast("Time updated by " + connection.DisplayName );
                    }
                    catch (Exception ex)
                    {
                        if(ex is OverflowException || ex is FormatException)
                            connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "wrong time value, try 9:00 or 21:00" });
                        else throw;
                    }
                    return true;
                }
                #endregion
                #region Additem command
                if (command is AdditemCommand)
                {
                    try
                    {
                        if (pars == null || pars.Length == 0)
                            return false;

                        ushort blueprintId ;

                        if (!ushort.TryParse(pars[0], out blueprintId))
                        {
                            var entity = _server.EntityFactory.Config.BluePrints.Values.FirstOrDefault(v => string.Equals(v.Name.Replace(" ", ""), pars[0], StringComparison.CurrentCultureIgnoreCase));

                            if (entity == null)
                            {
                                connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "There is no such item." });
                                return false;
                            }
                            blueprintId = entity.BluePrintId;
                        }

                        var count = pars.Length == 2 ? int.Parse(pars[1]) : 1;

                        var charEntity = (CharacterEntity)connection.ServerEntity.DynamicEntity;

                        var item = (IItem)_server.EntityFactory.CreateFromBluePrint(blueprintId);

                        charEntity.Inventory.PutItem(item, count);
                        connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = string.Format("Item {0} was added to the inventory", item) });
                    }
                    catch (Exception x)
                    {
                        connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "Error: " + x.Message });
                    }
                    return true;
                }
                #endregion
                #region Setrole command
                if (command is SetroleCommand)
                {
                    if (pars == null || pars.Length != 2)
                        return false;

                    bool success = false;

                    switch (pars[2])
                    {
                        case "admin":
                            success = _server.UsersStorage.SetRole(pars[0], UserRole.Administrator);
                            break;
                        case "op":
                            success = _server.UsersStorage.SetRole(pars[0], UserRole.Moderator);
                            break;
                        case "normal":
                            success = _server.UsersStorage.SetRole(pars[0], UserRole.Normal);
                            break;
                        default:
                            break;
                    }

                    if (success)
                    {
                        connection.Send(new ChatMessage { 
                            IsServerMessage = true, 
                            DisplayName = "server", 
                            Message = "User access level is updated" 
                        });
                    }
                    else
                    {
                        connection.Send(new ChatMessage { 
                            IsServerMessage = true, 
                            DisplayName = "server", 
                            Message = "Unable to update the user, check the login or role name" 
                        });
                    }
                    return true;
                }
                #endregion

                OnPlayerCommand(new PlayerCommandEventArgs { 
                    Command = command, 
                    Params = pars, 
                    PlayerEntity = connection.ServerEntity.DynamicEntity 
                });
                return true;
            }
            return false;
        }
    }
}
