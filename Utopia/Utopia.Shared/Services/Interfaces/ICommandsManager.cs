using System;

namespace Utopia.Shared.Services.Interfaces
{
    public interface ICommandsManager
    {
        /// <summary>
        /// Occurs when users sends a command
        /// </summary>
        event EventHandler<PlayerCommandEventArgs> PlayerCommand;

        void RegisterCommand(IServerCommand command);
    }
}