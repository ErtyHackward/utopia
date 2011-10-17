using Nuclex.UserInterface.Controls;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// This control will display current slot that is dragging now
    /// </summary>
    public class DragControl : Control
    {
        /// <summary>
        /// Slot that is currently attached to player hand
        /// </summary>
        public ContainedSlot Slot { get; set; }

        /// <summary>
        /// Indicates if user currently have something attached to his mouse pointer
        /// </summary>
        public bool IsDragging { get; set; }
    }
}
