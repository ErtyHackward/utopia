using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using System.Diagnostics;
using Nuclex.UserInterface.Input;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using Utopia.Shared.Chunks.Entities.Inventory;


namespace Utopia.GUI.D3D.Inventory
{
    public class InventoryCell : Control, IDropTarget
    {

 
        public InventorySlot InventorySlot { get; set; }


        public bool MouseHovering
        {
            get
            {
                MouseState ms = Mouse.GetState();
                int x = ms.X;
                int y = ms.Y;
                return this.GetAbsoluteBounds().Contains(x, y);
            }

            set { }
        }
        public bool IsLink { get; set; }
        public Item Item { get; set; }


        public InventoryCell( InventorySlot slot = InventorySlot.Bags)
            : base()
        {
            InventorySlot = slot;

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
                this.MouseHovering = false;

            }
        }

        protected override void OnMouseEntered()
        {
            // Debug.WriteLine("hovering over " + this.Name);
            this.MouseHovering = true;
        }

        protected override void OnMouseLeft()
        {
            this.MouseHovering = false;
        }

        public void Link(Item itemToLink) {
            this.Item = itemToLink;
        }
    }
}
