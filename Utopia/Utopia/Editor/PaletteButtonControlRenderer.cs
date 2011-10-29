using System.Drawing;
using Nuclex.UserInterface.Visuals.Flat;
using Nuclex.UserInterface.Visuals.Flat.Renderers;
using RectangleF = Nuclex.UserInterface.RectangleF;

namespace Utopia.Editor
{
    public class PaletteButtonControlRenderer : IFlatControlRenderer<PaletteButtonControl> {

    public void Render(
     PaletteButtonControl control, IFlatGuiGraphics graphics
    ) {
      RectangleF controlBounds = control.GetAbsoluteBounds();

      // Determine the style to use for the button
      int stateIndex = 0;
      if(control.Enabled) {
        if(control.Depressed) {
          stateIndex = 3;
        } else if(control.MouseHovering || control.HasFocus) {
          stateIndex = 2;
        } else {
          stateIndex = 1;
        }
      }

      // Draw the button's frame
        graphics.DrawElement(states[stateIndex], controlBounds, control.Color);

        RectangleF innerBounds = controlBounds;
        innerBounds.Inflate(-1f, -1f);

        if (control.Texture != null)
            graphics.DrawCustomTexture(control.Texture, innerBounds);

        // If there's text assigned to the button, draw it into the button
      if(!string.IsNullOrEmpty(control.Text)) {
        graphics.DrawString(states[stateIndex], controlBounds, control.Text);
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