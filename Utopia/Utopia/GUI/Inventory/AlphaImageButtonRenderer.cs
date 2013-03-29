using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;

namespace Utopia.GUI.Inventory
{
    public class AlphaImageButtonRenderer : IFlatControlRenderer<AlphaImageButtonControl>
    {
        private enum ButtonState
        {
            Default,
            Down,
            Hover,
            Disabled
        }

        public void Render(AlphaImageButtonControl control, IFlatGuiGraphics graphics)
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            // Determine the style to use for the button
            var stateIndex = ButtonState.Disabled;
            if (control.Enabled)
            {
                if (control.Depressed)
                {
                    stateIndex = ButtonState.Down;
                }
                else if (control.MouseHovering || control.HasFocus)
                {
                    stateIndex = ButtonState.Hover;
                }
                else
                {
                    stateIndex = ButtonState.Default;
                }
            }

            float alpha;
            SpriteTexture tex;
            switch (stateIndex)
            {
                case ButtonState.Default:
                    alpha = control.AlphaDefault;
                    tex = control.CustomImage;
                    break;
                case ButtonState.Down:
                    alpha = control.AlphaDown;
                    tex = control.CustomImageDown;
                    break;
                case ButtonState.Hover:
                    alpha = control.AlphaHover;
                    tex = control.CustomImageHover;
                    break;
                case ButtonState.Disabled:
                    alpha = control.AlphaDisabled;
                    tex = control.CustomImageDisabled;
                    break;
                default:
                    alpha = 0f;
                    tex = null;
                    break;
            }

            if (alpha == 0f || tex == null)
                return;

            alpha = MathHelper.Clamp(alpha, 0, 1);

            graphics.DrawCustomTexture(tex, ref controlBounds, 0, control.DrawGroupId, new ByteColor(255, 255, 255, (int)(255 * alpha)));
            
            if (control.CusomImageLabel != null)
            {
                var imgRect = controlBounds;

                imgRect.X += (imgRect.Width - control.CusomImageLabel.Width) / 2;
                imgRect.Y += (imgRect.Height - control.CusomImageLabel.Height) / 2;
                imgRect.Width = control.CusomImageLabel.Width;
                imgRect.Height = control.CusomImageLabel.Height;

                if (stateIndex == ButtonState.Down)
                    imgRect.Y += 1;

                graphics.DrawCustomTexture(control.CusomImageLabel, ref imgRect, 0, control.DrawGroupId, new ByteColor(255, 255, 255, (int)(255 * alpha)));
            }
        }
    }
}