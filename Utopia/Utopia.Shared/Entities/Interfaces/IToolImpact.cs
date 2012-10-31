using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an tool effect from tool using
    /// </summary>
    public interface IToolImpact : IBinaryStorable
    {
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        bool Success { get; set; }
    }
}
