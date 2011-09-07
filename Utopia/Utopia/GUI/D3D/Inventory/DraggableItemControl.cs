﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using System.Diagnostics;
using Nuclex.UserInterface.Input;
using Utopia.Shared.Chunks.Entities.Inventory;
using S33M3Engines.InputHandler;

namespace Utopia.GUI.D3D.Inventory
{
    public class DraggableItemControl : Control
    {
        public static UniRectangle referenceBounds = new UniRectangle(0, 0, Item.IconSize, Item.IconSize);
       public bool beingDragged;

        public Item Item { get; set; }

        /// <summary>X coordinate at which the control was picked up</summary>
        protected float pickupX;
        /// <summary>Y coordinate at which the control was picked up</summary>
        protected float pickupY;

        protected UniRectangle pickupBounds;

        public DraggableItemControl()
            : base()
        {
            
        }



        protected override void OnMouseReleased(MouseButtons button)
        {
            beingDragged = false;

            IDropTarget dropTarget = findDropTarget(this.Screen.Desktop);

            if (dropTarget == this.Parent)
            {
                Debug.WriteLine("droptarget = parent");
                this.Bounds = referenceBounds;
                return;
            }
            else if (dropTarget != null)
            {
                if (dropTarget.IsLink)
                {
                    //copy item reference and restore the dragged item to its original position
                    dropTarget.Link(Item);
                    this.Bounds = pickupBounds;
                }
                else
                {
                    //swap childs
                    Control dest = ((Control)dropTarget).Children.First();

                    Control thisParent = this.Parent;

                    this.RemoveFromParent();
                    dest.RemoveFromParent();

                    thisParent.Children.Add(dest);
                    ((Control)dropTarget).Children.Add(this);


                    dest.Bounds = referenceBounds;
                    this.Bounds = referenceBounds;

                }
            }
            else
            {
                //restore inital position
                this.Bounds = pickupBounds;
            }
        }

        //recursively search the control tree for a control with the droptarget marker interface that has the mouse hovering
        //returns null if no match
        private IDropTarget findDropTarget(Control parent)
        {
            if (parent is ToolBarUi)
            {
                Console.WriteLine();
            }

            if (this.MouseOverControl is IDropTarget)
            {
                Debug.WriteLine("mouse over ctrl");
                return (IDropTarget)MouseOverControl;
            }

            foreach (Control control in parent.Children)
            {
                //int x = Mouse.GetState().X;
               // int y = Mouse.GetState().Y;

                if (control != this && control is IDropTarget && ((IDropTarget)control).MouseHovering
                   // && control.GetAbsoluteBounds().Contains(x, y)//avoid bugs with multiple selected cells
                    )
                {
                    if (this.Item!=null && this.Item.AllowedSlots.HasFlag(((IDropTarget)control).InventorySlot))
                    {
                        return (IDropTarget)control;
                    }
                    else
                    {
                        Debug.WriteLine("{0} not allowed, only {1}", ((IDropTarget)control).InventorySlot, this.Item.AllowedSlots);
                    }

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
            if (this.beingDragged)
            {
                this.BringToFront();
                // Adjust the control's position within the container
                this.Bounds.Location.X.Offset += x - this.pickupX;
                this.Bounds.Location.Y.Offset += y - this.pickupY;
                //this.Bounds.Size.X = 32;
                // this.Bounds.Size.Y = 32;
            }
            else
            {

                // Remember the current mouse position so we know where the user picked
                // up the control when a drag operation begins
                this.pickupX = x;
                this.pickupY = y;
                pickupBounds = this.Bounds;
            }
            base.OnMouseMoved(x, y);
        }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        protected override void OnMousePressed(MouseButtons button)
        {

            if (Item == null) return;//dont drag empty cells

            if (button == MouseButtons.Left)
            {
                this.beingDragged = true;
                ((IDropTarget)this.Parent).MouseHovering = false;
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
