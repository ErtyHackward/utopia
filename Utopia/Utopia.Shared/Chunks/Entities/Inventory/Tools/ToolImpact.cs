using System;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    /// <summary>
    /// Basic tool impact
    /// </summary>
    public class ToolImpact : IToolImpact
    {
        /// <summary>
        /// Describes why tool can not be used
        /// </summary>
        public String Message { get; set; }
        
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        public bool Success { get; set; }
    }
}
