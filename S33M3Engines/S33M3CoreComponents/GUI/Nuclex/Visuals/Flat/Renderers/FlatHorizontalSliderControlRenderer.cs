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



namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Renderers
{

    /// <summary>Renders horizontal sliders in a traditional flat style</summary>
    public class FlatHorizontalSliderControlRenderer :
      IFlatControlRenderer<Controls.Desktop.HorizontalSliderControl>
    {

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
          Controls.Desktop.HorizontalSliderControl control, IFlatGuiGraphics graphics
        )
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            float thumbWidth = controlBounds.Width * control.ThumbSize;
            float thumbX = (controlBounds.Width - thumbWidth) * control.ThumbPosition;

            graphics.DrawElement("rail.horizontal",ref controlBounds);

            RectangleF thumbBounds = new RectangleF(
              controlBounds.Left + thumbX, controlBounds.Top, thumbWidth, controlBounds.Height
            );

            if (control.ThumbDepressed)
            {
                graphics.DrawElement("slider.horizontal.depressed",ref thumbBounds);
            }
            else if (control.MouseOverThumb)
            {
                graphics.DrawElement("slider.horizontal.highlighted",ref thumbBounds);
            }
            else
            {
                graphics.DrawElement("slider.horizontal.normal",ref thumbBounds);
            }
        }
    }
}
