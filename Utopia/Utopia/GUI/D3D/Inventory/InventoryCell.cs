using System;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Input;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Represents inventory cell
    /// </summary>
    public class InventoryCell : Control, IDropTarget
    {
        private readonly SlotContainer<ContainedSlot> _container;

        /// <summary>
        /// Gets current cell grid position
        /// </summary>
        public Vector2I InventoryPosition { get; private set; }

        /// <summary>
        /// Gets or sets current slot
        /// </summary>
        public ContainedSlot Slot
        {
            get;
            set;
        }

        /// <summary>
        /// Creates new inventory cell and links it with some container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="position"></param>
        public InventoryCell(SlotContainer<ContainedSlot> container, Vector2I position)
        {
            _container = container;
            InventoryPosition = position;
        }

        public bool MouseHovering
        {
            get
            {
                MouseState ms = Mouse.GetState();
                int x = ms.X;
                int y = ms.Y;
                return GetAbsoluteBounds().Contains(x, y);
            }

            set { }
        }

        protected override void OnMouseReleased(MouseButtons button)
        {
        }

        protected override void OnMouseMoved(float x, float y)
        {
        }

        protected override void OnMousePressed(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                MouseHovering = false;
            }
        }

        protected override void OnMouseEntered()
        {
            // Debug.WriteLine("hovering over " + this.Name);
            MouseHovering = true;
        }

        protected override void OnMouseLeft()
        {
            MouseHovering = false;
        }
    }
}