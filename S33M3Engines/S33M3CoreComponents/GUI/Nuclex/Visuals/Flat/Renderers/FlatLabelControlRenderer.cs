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
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using SharpDX;
using S33M3Resources.Structs;



namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Renderers
{

    /// <summary>Renders label controls in a traditional flat style</summary>
    public class FlatLabelControlRenderer :
      IFlatControlRenderer<Controls.LabelControl>
    {

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(Controls.LabelControl control, IFlatGuiGraphics graphics)
        {
            var absoluteBounds = control.GetAbsoluteBounds();

            if (control.CustomFont != null)
            {
                ByteColor color = control.Color;
                graphics.DrawString(control.CustomFont, ref absoluteBounds, control.Text, ref color, control.Autosizing);
            }
            else
            {
                string styleFrame;

                if (control.IsHeaderFont) styleFrame = "labelHeader";
                else
                {
                    switch (control.FontStyle)
                    {
                        case System.Drawing.FontStyle.Bold:
                            styleFrame = "labelBold";
                            break;
                        case System.Drawing.FontStyle.Italic:
                            styleFrame = "labelItalic";
                            break;
                        case System.Drawing.FontStyle.Regular:
                            styleFrame = "label";
                            break;
                        case System.Drawing.FontStyle.Underline:
                            styleFrame = "labelUnderline";
                            break;
                        default:
                            styleFrame = "label";
                            break;
                    }
                }

                if (control.ColorSet)
                {
                    ByteColor color = control.Color;
                    graphics.DrawString(styleFrame, ref absoluteBounds, control.Text, ref color, control.Autosizing);
                }
                else
                {
                    graphics.DrawString(styleFrame, ref absoluteBounds, control.Text, control.Autosizing);
                }
            }
        }

    }
}
