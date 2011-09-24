namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Represents an tool effect from tool using
    /// </summary>
    public interface IToolImpact
    {
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        bool Success { get; set; }
    }
}
