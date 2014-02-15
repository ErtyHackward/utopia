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

using System;
using System.Collections.Generic;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Renderers
{

    /// <summary>Renders button controls in a traditional flat style</summary>
    public class FlatButtonControlRenderer : IFlatControlRenderer<Controls.Desktop.ButtonControl>
    {

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
          Controls.Desktop.ButtonControl control, IFlatGuiGraphics graphics
        )
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

            if (stateIndex == 0)
            {
                if (control.CustomImageDisabled != null)
                    graphics.DrawCustomTexture(control.CustomImageDisabled, ref controlBounds);
                else if (control.CustomImage != null)
                {
                    graphics.DrawCustomTexture(control.CustomImage, ref controlBounds,0, control.DrawGroupId, new ByteColor(255,255,255,100));
                }
            }
            else if (stateIndex == 1 && control.CustomImage != null)
            {
                graphics.DrawCustomTexture(control.CustomImage, ref controlBounds);
            }
            else if (stateIndex == 2 && control.CustomImageHover != null)
            {
                graphics.DrawCustomTexture(control.CustomImageHover, ref controlBounds);
            }
            else if (stateIndex == 3 && control.CustomImageDown != null)
            {
                graphics.DrawCustomTexture(control.CustomImageDown, ref controlBounds);
            }
            else
            {
                // Draw the button's frame
                graphics.DrawElement(states[stateIndex], ref controlBounds);
            }

            if (control.CusomImageLabel != null)
            {
                var imgRect = controlBounds;

                imgRect.X += ( imgRect.Width - control.CusomImageLabel.Width ) / 2;
                imgRect.Y += ( imgRect.Height - control.CusomImageLabel.Height ) / 2;
                imgRect.Width = control.CusomImageLabel.Width;
                imgRect.Height = control.CusomImageLabel.Height;

                if (stateIndex == 3)
                    imgRect.Y += 1;

                if (stateIndex == 0)
                    graphics.DrawCustomTexture(control.CusomImageLabel, ref imgRect, 0, control.DrawGroupId, new ByteColor(255,255,255,100));
                else
                    graphics.DrawCustomTexture(control.CusomImageLabel, ref imgRect);
            }
            else if (!string.IsNullOrEmpty(control.Text))
            {
                if (control.CustomFont != null)
                {
                    ByteColor color = control.Color;
                    graphics.DrawString(control.CustomFont, ref controlBounds, control.Text, ref color, false, -1, FlatGuiGraphics.Frame.HorizontalTextAlignment.Center, FlatGuiGraphics.Frame.VerticalTextAlignment.Center);
                }
                else
                {
                    if (control.ColorSet)
                    {
                        ByteColor color = control.Color;
                        graphics.DrawString(states[stateIndex], control.TextFontId, ref controlBounds, control.Text, ref color, false);
                    }
                    else
                    {
                        graphics.DrawString(states[stateIndex], control.TextFontId, ref controlBounds, control.Text, false);
                    }
                }
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
