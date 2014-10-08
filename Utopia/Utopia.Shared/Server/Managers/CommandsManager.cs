using System;
using System.Collections.Generic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Commands;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Managers
{
    public class CommandsManager
    {
        private readonly ServerCore _server;

        private readonly Dictionary<string, IChatCommand> _commands = new Dictionary<string, IChatCommand>();

        /// <summary>
        /// Occurs when users sends a command
        /// </summary>
        public event EventHandler<PlayerCommandEventArgs> PlayerCommand;

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            var handler = PlayerCommand;
            if (handler != null) handler(this, e);
        }

        public IEnumerable<IChatCommand> Commands {
            get { return _commands.Values; }
        }

        public CommandsManager(ServerCore server)
        {
            _server = server;

            RegisterCommand(new HelpCommand());
            RegisterCommand(new StatusCommand());
            RegisterCommand(new SaveCommand());
            RegisterCommand(new ServicesCommand());
            RegisterCommand(new SettimeCommand());
            RegisterCommand(new AdditemCommand());
            RegisterCommand(new SetroleCommand());
            RegisterCommand(new UsersCommand());
            RegisterCommand(new KickCommand());
            RegisterCommand(new BanCommand());
            RegisterCommand(new SetSpawnPointCommand());
            RegisterCommand(new MotdCommand());
            RegisterCommand(new DefaultRoleCommand());
            RegisterCommand(new WipeChunkCommand());
            RegisterCommand(new NpcSelectCommand());
            RegisterCommand(new NpcAddActivityCommand());
            RegisterCommand(new NpcAddPointCommand());
            RegisterCommand(new NpcComehereCommand());
            RegisterCommand(new WipeNpcsCommand());
            RegisterCommand(new NpcSpawnCommand());
        }

        public void RegisterCommand(IChatCommand command)
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
                IChatCommand command;
                if (!_commands.TryGetValue(cmd, out command))
                {
                    connection.SendChat("Sorry, no such command.");
                    return true;
                }

                // check access
                if (command is IRoleRestrictedCommand && connection.UserRole != UserRole.Administrator)
                {
                    if (!(command as IRoleRestrictedCommand).HasAccess(connection.UserRole))
                    {
                        connection.SendChat("Sorry, access denied.");
                        return true;
                    }
                }

                var serverCommand = command as IServerChatCommand;

                if (serverCommand != null)
                {
                    serverCommand.Execute(_server, connection, pars);
                    return true;
                }

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
