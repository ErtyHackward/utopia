using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;

using System.Diagnostics;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    public class ButtonItemControl : ButtonControl, IDropTarget
    {

        public Item Item { get; set; }
        public Item RightItem { get; set; }


        public bool IsLink { get; set; }

        public EquipmentSlotType InventorySlot { get; set; }

        public bool Highlight { get; set; } // to render as the hasfocus without really giving focus

        public ButtonItemControl(Item item)
            : base()
        {
            Item = item;
            //Text = item.name;
            InventorySlot = EquipmentSlotType.Bags;//no need for a toolbar slot, all items are accepted
        }

        protected override void OnMouseEntered()
        {
            base.OnMouseEntered();
            if (MouseOverControl != null) Debug.WriteLine("---> " + MouseOverControl.Name);

        }

        public void Link(Item itemToLink)
        {
            //a way to remove an assigned tool and avoid double tool exploit
            if (itemToLink == this.Item)
                this.Item = null;
            else if (itemToLink == this.RightItem)
                this.RightItem = null;
            else
            {
                if (this.Item == null)
                {
                    this.Item = itemToLink;
                }
                else
                {
                    this.RightItem = itemToLink;
                }
            }
        }
    }
}
