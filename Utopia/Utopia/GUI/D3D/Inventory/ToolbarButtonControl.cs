using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;

using System.Diagnostics;
using SharpDX;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    public class ToolbarButtonControl : ButtonControl, IDropTarget
    {
        private readonly PlayerCharacter _player;

        public ToolbarSlot ToolbarSlot;       

        public bool Highlight { get; set; }

        public IItem LeftItem
        {
            get { return _player.LookupItem(ToolbarSlot.Left); }
        }

        public IItem RightItem
        {
            get { return _player.LookupItem(ToolbarSlot.Right); }
        }

// to render as the hasfocus without really giving focus

        public ToolbarButtonControl(PlayerCharacter player,ToolbarSlot slot)
            : base()
        {
            _player = player;
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
            uint itemId = slotToLink.Item.EntityId;

            if (ToolbarSlot.Left == itemId)
            {
                //a way to remove an assigned tool and avoid double tool exploit
                ToolbarSlot.Left = 0;
            }
            else if (ToolbarSlot.Right == itemId)
            {
                ToolbarSlot.Right = 0;
            } 
            else
            {
                if (ToolbarSlot.Left == 0)
                {
                    Text = itemId.ToString();
                    ToolbarSlot.Left = itemId;
                }
                else
                {
                    ToolbarSlot.Right = itemId;
                }
            }

            Text = LeftItem == null ? "" : LeftItem.DisplayName;//TODO remove all text based early tests
            Text += Text != "" ? "|":"";
            Text += RightItem == null ? "": RightItem.DisplayName; 
        }
    }
}
