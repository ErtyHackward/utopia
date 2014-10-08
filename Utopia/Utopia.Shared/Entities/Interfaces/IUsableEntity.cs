namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an entity that can be used without a tool
    /// </summary>
    public interface IUsableEntity
    {
        /// <summary>
        /// Executes entity specific logic, (handtool use)
        /// </summary>
        void Use();
    }
}
