using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public abstract class Tool : Item
    {
        /// <summary>
        /// Tries to use the tool. The tool should decide is it possible to use and return toolImpact.
        /// </summary>
        /// <returns></returns>
        public IToolImpact Use()
        {
            return ToolLogic.Use(this);
        }

        /// <summary>
        /// Gets or sets tool business logic object (separate for client and server)
        /// This object is not stored with tool instance
        /// </summary>
        public IToolLogic ToolLogic { get; set; }

        /// <summary>
        /// The tool owner, impossible to use the tool without owner
        /// This object is not stored with tool instance
        /// </summary>
        public DynamicEntity Parent { get; set; }

        protected Tool()
        {
            UniqueName = this.GetType().Name;
        }

    }
}
