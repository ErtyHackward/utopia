namespace Utopia.Shared.Services.Interfaces
{
    public interface IChatCommand
    {
        /// <summary>
        /// Command id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Description of the command that will be displayed by "help {id}"
        /// </summary>
        string Description { get; }
    }
}
