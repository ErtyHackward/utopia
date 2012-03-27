using System.Drawing;
using RectangleF = S33M3CoreComponents.GUI.Nuclex.RectangleF;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using SharpDX;
using S33M3Resources.Structs;

namespace Utopia.Editor
{
    public class PaletteButtonControlRenderer : IFlatControlRenderer<PaletteButtonControl> {

        public void Render(PaletteButtonControl control, IFlatGuiGraphics graphics)
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            // Determine the style to use for the button
            int stateIndex = 0;
            if (control.Enabled)
            {
                if (control.Depressed)
                {
                    stateIndex = 3;
                }
                else if (control.MouseHovering || control.HasFocus)
                {
                    stateIndex = 2;
                }
                else
                {
                    stateIndex = 1;
                }
            }

            // Draw the button's frame
            ByteColor color = control.Color;
            graphics.DrawElement(states[stateIndex], ref controlBounds, ref color);

            RectangleF innerBounds = controlBounds;
            innerBounds.Inflate(-1f, -1f);

            if (control.Texture != null)
                graphics.DrawCustomTexture(control.Texture, ref innerBounds);

            // If there's text assigned to the button, draw it into the button
            if (!string.IsNullOrEmpty(control.Text))
            {
                graphics.DrawString(states[stateIndex], 0, ref controlBounds, control.Text, true);
            }
        }

    /// <summary>Names of the states the button control can be in</summary>
    /// <remarks>
    ///   Storing this as full strings instead of building them dynamically prevents
    ///   any garbage from forming during rendering.
    /// </remarks>
    private static readonly string[] states = new string[] {
      "button.disabled",
      "button.normal",
      "button.highlighted",
      "button.depressed"
    };
    }
}