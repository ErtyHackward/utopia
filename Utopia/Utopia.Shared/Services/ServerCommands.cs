using Utopia.Shared.Services.Interfaces;

namespace Utopia.Shared.Services
{
    /// <summary>
    /// Server command for status information
    /// </summary>
    public class StatusCommand : ModeratorCommand
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

    public class SaveCommand : ModeratorCommand
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

    public class ServicesCommand : ModeratorCommand
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

    public class SettimeCommand : ModeratorCommand
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

    public class AdditemCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "additem"; }
        }

        public override string Description
        {
            get { return "Adds items to the inventory. Format: \"additem <blueprintid> [count=1]\" Example: additem 59 10"; }
        }
    }

    public class SetroleCommand : AdministratorCommand
    {
        public override string Id
        {
            get { return "setrole"; }
        }

        public override string Description
        {
            get { return "Changes access level of the user. Format: \"setrole <usernickname> <role>\".\n Possible roles: op, normal "; }
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
