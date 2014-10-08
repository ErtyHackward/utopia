namespace Utopia.Shared.Entities.Events
{
    public enum UseType
    {
        Use,
        Put,
        Craft,
        /// <summary>
        /// Provides entity state to execute a command
        /// </summary>
        Command
    }
}