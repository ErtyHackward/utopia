namespace Utopia.Server.Commands
{
    /// <summary>
    /// A structure for server commands
    /// </summary>
    public abstract class ServerCommand
    {
        /// <summary>
        /// Gets or sets command id text
        /// </summary>
        public abstract string Command { get; }

        /// <summary>
        /// Gets or sets command description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="server"></param>
        public abstract void Execute(Server server); 
    }
}
