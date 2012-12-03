using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.GUI.Nuclex;

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
                    if (control.CustomBackgroundHover != null)
                    {
                        if (control.CustomBackgroundAutosize)
                        {
                            graphics.DrawCustomTexture(control.CustomBackgroundHover, ref controlBounds, 0, control.DrawGroupId);
                        }
                        else
                        {
                            var dx = (controlBounds.Width - control.CustomBackgroundHover.Width) / 2;
                            var dy = (controlBounds.Height - control.CustomBackgroundHover.Height) / 2;

                            var bgBounds = controlBounds;

                            bgBounds.X += dx;
                            bgBounds.Y += dy;
                            bgBounds.Width = control.CustomBackgroundHover.Width;
                            bgBounds.Height = control.CustomBackgroundHover.Height;

                            graphics.DrawCustomTexture(control.CustomBackgroundHover, ref bgBounds, 0, control.DrawGroupId);
                        }
                    }
                    else
                        graphics.DrawElement("button.highlighted",ref controlBounds);
                }
                else
                {

                    if (control.CustomBackground != null)
                    {
                        if (control.CustomBackgroundAutosize)
                        {
                            graphics.DrawCustomTexture(control.CustomBackground, ref controlBounds, 0, control.DrawGroupId);
                        }
                        else
                        {
                            var dx = (controlBounds.Width - control.CustomBackground.Width) / 2;
                            var dy = (controlBounds.Height - control.CustomBackground.Height) / 2;

                            var bgBounds = controlBounds;

                            bgBounds.X += dx;
                            bgBounds.Y += dy;
                            bgBounds.Width = control.CustomBackground.Width;
                            bgBounds.Height = control.CustomBackground.Height;

                            graphics.DrawCustomTexture(control.CustomBackground, ref bgBounds, 0, control.DrawGroupId);
                        }
                    }
                    else graphics.DrawElement("button.normal",ref controlBounds, control.DrawGroupId);
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
                    graphics.DrawCustomTexture(tex, ref texBounds, textureArrayIndex, control.DrawIconsGroupId);
                }
                else
                {
                    var displayName = control.Slot.Item.Name;

                    graphics.DrawString("button.normal", 0, ref controlBounds, displayName, false, -1, control.DrawGroupId);
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
                graphics.DrawString("slot.items.shadow", 0, ref textPosition, itemsCount, false, -1, control.DrawGroupId);
                graphics.DrawString("slot.items", 0, ref textPosition, itemsCount, false, -1, control.DrawGroupId);
            }

            #endregion
            
        }
    }
}