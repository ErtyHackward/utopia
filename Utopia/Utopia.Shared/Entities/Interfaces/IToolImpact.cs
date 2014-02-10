using ProtoBuf;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an tool effect from tool using
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(ToolImpact))]
    public interface IToolImpact
    {
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        bool Success { get; set; }
    }
}
