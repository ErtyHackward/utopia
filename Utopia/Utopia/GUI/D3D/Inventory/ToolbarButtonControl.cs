﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;

using System.Diagnostics;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    public class ToolbarButtonControl : ButtonControl, IDropTarget
    {

        public ToolbarSlot ToolbarSlot;       

        public bool Highlight { get; set; }

        public IItem LeftItem
        {
            get { return ToolbarSlot.Item; }
        }

        public IItem RightItem
        {
            get { return ToolbarSlot.Item; }//TODO right item in toolbarSlot
        }

// to render as the hasfocus without really giving focus

        public ToolbarButtonControl(ToolbarSlot slot)
            : base()
        {
            ToolbarSlot = slot;    
        }

     

        protected override void OnMouseEntered()
        {
            base.OnMouseEntered();
            //if (MouseOverControl != null) 
            //    Debug.WriteLine("---> " + MouseOverControl.Name);

        }

        public void Link(ContainedSlot slotToLink)
        {
            Vector2I pos = slotToLink.GridPosition;

            if (ToolbarSlot.Left==pos)
            {
                //a way to remove an assigned tool and avoid double tool exploit
                ToolbarSlot.Left = null;
            } else if (ToolbarSlot.Right==pos)
            {
                ToolbarSlot.Right = null;
            } 
            else
            {
                if (ToolbarSlot.Left == null)
                {
                    Text = pos.ToString();
                    ToolbarSlot.Left = pos;
                }
                else
                {
                    ToolbarSlot.Right = pos;
                }
            }
        }
    }
}
