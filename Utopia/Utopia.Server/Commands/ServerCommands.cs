using Utopia.Server.Structs;

namespace Utopia.Server.Commands
{
    /// <summary>
    /// Server command for status information
    /// </summary>
    public class StatusCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "status"; }
        }

        public override string Description
        {
            get { return "Returns server overall statistics."; }
        }
    }

    public class SaveCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "save"; }
        }

        public override string Description
        {
            get { return "Saves all modified chunks to the database"; }
        }
    }

    public class ServicesCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "services"; }
        }

        public override string Description
        {
            get { return "Lists all active server services"; }
        }
    }

    public class SettimeCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "settime"; }
        }

        public override string Description
        {
            get { return "Changes current game time. Examples: settime 9:00, settime 23:00"; }
        }
    }

    public class HelpCommand : IServerCommand
    {
        public string Id
        {
            get { return "help"; }
        }

        public string Description
        {
            get { return "Provides command list and help information about commands. Use help {command_name} to get details about the command"; }
        }
    }

}
