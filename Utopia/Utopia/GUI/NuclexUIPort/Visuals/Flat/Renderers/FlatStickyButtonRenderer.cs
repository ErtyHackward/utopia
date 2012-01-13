using Nuclex.UserInterface.Visuals.Flat;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;

namespace Utopia.GUI.NuclexUIPort.Visuals.Flat.Renderers
{
    public class FlatStickyButtonRenderer : IFlatControlRenderer<StickyButtonControl>
    {
        public void Render(StickyButtonControl control, IFlatGuiGraphics graphics)
        {
            var controlBounds = control.GetAbsoluteBounds();

            // Determine the style to use for the button
            var stateIndex = 0;
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

            if (control.Sticked)
                stateIndex = 3;

            // Draw the button's frame
            graphics.DrawElement(States[stateIndex], controlBounds);

            // If there's text assigned to the button, draw it into the button
            if (!string.IsNullOrEmpty(control.Text))
            {
                graphics.DrawString(States[stateIndex], controlBounds, control.Text);
            }
        }

        /// <summary>Names of the states the button control can be in</summary>
        /// <remarks>
        ///   Storing this as full strings instead of building them dynamically prevents
        ///   any garbage from forming during rendering.
        /// </remarks>
        private static readonly string[] States = new[]
                                                      {
                                                          "button.disabled",
                                                          "button.normal",
                                                          "button.highlighted",
                                                          "button.depressed"
                                                      };
    }
}
