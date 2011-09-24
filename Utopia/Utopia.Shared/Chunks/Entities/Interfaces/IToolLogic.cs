using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Describes tool business logic object that perform actual use 
    /// </summary>
    public interface IToolLogic
    {
        /// <summary>
        /// Performs tool business logic
        /// </summary>
        /// <param name="callerTool"></param>
        /// <returns></returns>
        IToolImpact Use(Tool callerTool);
    }
}
