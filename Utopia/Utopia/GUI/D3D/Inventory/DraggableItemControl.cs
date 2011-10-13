using System;
using System.Diagnostics;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Input;
using Utopia.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    public class DraggableItemControl : Control
    {
        public static UniRectangle ReferenceBounds = new UniRectangle(0, 0, 64, 64);
        private readonly SlotContainer<ContainedSlot> _inventory;
        public bool BeingDragged;
        protected UniRectangle PickupBounds;

        /// <summary>X coordinate at which the control was picked up</summary>
        protected float PickupX;

        /// <summary>Y coordinate at which the control was picked up</summary>
        protected float PickupY;

        public DraggableItemControl(IconFactory iconFactory, SlotContainer<ContainedSlot> inventory)
        {
            IconFactory = iconFactory;
            _inventory = inventory;
        }

        public IconFactory IconFactory { get; private set; }
        public IItem Item { get; set; }

        protected override void OnMouseReleased(MouseButtons button)
        {
            BeingDragged = false;

            IDropTarget dropTarget = findDropTarget(Screen.Desktop);

            ContainedSlot sourceSlot = ((InventoryCell) Parent).Slot;

            if (dropTarget == Parent)
            {
                Debug.WriteLine("droptarget = parent");
                Bounds = ReferenceBounds;
                return;
            }
            else if (dropTarget != null)
            {
                if (dropTarget is ToolbarButtonControl)
                {
                    //copy item reference and restore the dragged item to its original position
                    ((ToolbarButtonControl) dropTarget).Link(sourceSlot);
                    Bounds = PickupBounds;
                }
                else if (dropTarget is InventoryCell)
                {
                    var destination = dropTarget as InventoryCell;

                    _inventory.DropOn(ref sourceSlot, destination.Slot);

                    //restore the draggablecontrols
                    Bounds = ReferenceBounds;
                    Item = sourceSlot.Item;

                    var dragDest = (DraggableItemControl) destination.Children.First();
                    dragDest.Item = destination.Slot.Item;
                }
            }
            else
            {
                //restore inital position
                Bounds = PickupBounds;
            }
        }

        //recursively search the control tree for a control with the droptarget marker interface that has the mouse hovering
        //returns null if no match
        private IDropTarget findDropTarget(Control parent)
        {
            if (parent is ToolBarUi)
            {
            }

            if (MouseOverControl is IDropTarget)
            {
                Debug.WriteLine("mouse over ctrl");
                return (IDropTarget) MouseOverControl;
            }

            foreach (Control control in parent.Children)
            {
                //int x = Mouse.GetState().X;
                // int y = Mouse.GetState().Y;

                if (control != this && control is IDropTarget && ((IDropTarget) control).MouseHovering
                    // && control.GetAbsoluteBounds().Contains(x, y)//avoid bugs with multiple selected cells
                    )
                {
                    return (IDropTarget) control;
                }
                else
                {
                    //XXX can be optimized by marking the containers of droptargets (like toolbar) with an IDroptargetContainer
                    IDropTarget found = findDropTarget(control);
                    if (found != null) return found;
                }
            }
            return null; //not found
        }


        protected override void OnMouseMoved(float x, float y)
        {
            if (BeingDragged)
            {
                BringToFront();
                // Adjust the control's position within the container
                Bounds.Location.X.Offset += x - PickupX;
                Bounds.Location.Y.Offset += y - PickupY;
                //this.Bounds.Size.X = 32;
                // this.Bounds.Size.Y = 32;
            }
            else
            {
                // Remember the current mouse position so we know where the user picked
                // up the control when a drag operation begins
                PickupX = x;
                PickupY = y;
                PickupBounds = Bounds;
            }
            base.OnMouseMoved(x, y);
        }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        protected override void OnMousePressed(MouseButtons button)
        {
            if (Item == null) return; //dont drag empty cells

            if (button == MouseButtons.Left)
            {
                BeingDragged = true;
                ((IDropTarget) Parent).MouseHovering = false;
            }
        }

        protected override void OnMouseEntered()
        {
        }

        protected override void OnMouseLeft()
        {
        }
    }
}