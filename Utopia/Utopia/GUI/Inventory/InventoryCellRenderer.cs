using S33M3_CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3_CoreComponents.Sprites;
using S33M3_CoreComponents.GUI.Nuclex;

namespace Utopia.GUI.Inventory
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
                    graphics.DrawElement("button.highlighted",ref controlBounds);
                }
                else
                {
                    graphics.DrawElement("button.normal",ref controlBounds);
                }
            }
            #endregion
            
            #region Item icon
            if (control.Slot != null && !control.Slot.IsEmpty)
            {
                SpriteTexture tex;
                int textureArrayIndex;
                control.IconFactory.Lookup(control.Slot.Item, out tex, out textureArrayIndex);
                if (tex != null)
                {
                    const int innerBorder = 3;
                    var texBounds = new RectangleF(
                        controlBounds.X + innerBorder, 
                        controlBounds.Y + innerBorder, 
                        controlBounds.Width - innerBorder * 2, 
                        controlBounds.Height - innerBorder * 2
                        );
                    graphics.DrawCustomTexture(tex, ref texBounds, textureArrayIndex);
                }
                else
                {
                    var displayName = control.Slot.Item.DisplayName;

                    graphics.DrawString("button.normal", ref controlBounds, displayName, false);
                }
            }
            #endregion

            #region Items count

            if (control.Slot != null && control.Slot.ItemsCount > 1)
            {
                var itemsCount = control.Slot.ItemsCount.ToString();

                var textSize = graphics.MeasureString("slot.items",ref controlBounds, itemsCount);

                var h = textSize.Height;
                var w = textSize.Width;

                var textPosition = new RectangleF(controlBounds.X + controlBounds.Width - textSize.Width - 6,
                                                  controlBounds.Y + controlBounds.Height - textSize.Height,
                                                  w,
                                                  h);
                graphics.DrawString("slot.items.shadow",ref textPosition, itemsCount, false);
                graphics.DrawString("slot.items", ref textPosition, itemsCount, false);
            }

            #endregion
            
        }
    }
}