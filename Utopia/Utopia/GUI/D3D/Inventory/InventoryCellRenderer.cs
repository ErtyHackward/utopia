#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using Nuclex.UserInterface.Visuals.Flat;
using Nuclex.UserInterface;

namespace Utopia.GUI.D3D.Inventory
{
    public class InventoryCellRenderer : IFlatControlRenderer<InventoryCell>
    {
        public void Render(InventoryCell control, IFlatGuiGraphics graphics)
        {
            var controlBounds = control.GetAbsoluteBounds();

            #region Backgroung
            if (control.DrawCellBackground)
            {
                if (control.MouseHovering || control.IsCellSelected)
                {
                    graphics.DrawElement("button.highlighted", controlBounds);
                }
                else
                {
                    graphics.DrawElement("button.normal", controlBounds);
                }
            }
            #endregion
            
            #region Item icon
            if (control.Slot != null && !control.Slot.IsEmpty)
            {
                var tex = control.IconFactory.Lookup(control.Slot.Item);
                if (tex != null)
                {
                    const int innerBorder = 3;
                    var texBounds = new RectangleF(
                        controlBounds.X + innerBorder, 
                        controlBounds.Y + innerBorder, 
                        controlBounds.Width - innerBorder * 2, 
                        controlBounds.Height - innerBorder * 2
                        );
                    graphics.DrawCustomTexture(tex, texBounds, tex.Index);
                }
                else
                {
                    var displayName = control.Slot.Item.DisplayName;

                    graphics.DrawString("button.normal", controlBounds, displayName);
                }
            }
            #endregion

            #region Items count

            if (control.Slot != null && control.Slot.ItemsCount > 1)
            {
                var itemsCount = control.Slot.ItemsCount.ToString();

                var textSize = graphics.MeasureString("slot.items", controlBounds, itemsCount);

                var h = textSize.Height;
                var w = textSize.Width;

                var textPosition = new RectangleF(controlBounds.X + controlBounds.Width - textSize.Width - 6,
                                                  controlBounds.Y + controlBounds.Height - textSize.Height,
                                                  w,
                                                  h);
                graphics.DrawString("slot.items.shadow", textPosition, itemsCount);
                graphics.DrawString("slot.items", textPosition, itemsCount);
            }

            #endregion
            
        }
    }
}