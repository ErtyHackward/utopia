using System;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Input;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    public class InventoryCell : Control, IDropTarget
    {
      
        public ContainedSlot Slot;

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