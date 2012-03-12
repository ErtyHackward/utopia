using System;
using S33M3_CoreComponents.GUI.Nuclex.Visuals.Flat;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using S33M3_CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using SharpDX;

namespace Utopia.GUI.NuclexUIPort.Visuals.Flat.Renderers
{
    public class FlatColorButtonControlRenderer : IFlatControlRenderer<ColorButtonControl>
    {
        public void Render(ColorButtonControl control, IFlatGuiGraphics graphics)
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
            graphics.DrawElement(States[stateIndex],ref controlBounds);

            var colorBounds = controlBounds;
            colorBounds.X += 3;
            colorBounds.Y += 3;
            colorBounds.Width -= 6;
            colorBounds.Height -= 6;

            graphics.DrawElement(States[4],ref colorBounds,ref control.Color);
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
                                                          "button.depressed",
                                                          "button.color"
                                                      };
    }
}
