﻿#region CPL License
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

    /// <summary>Renders window controls in a traditional flat style</summary>
    public class FlatWindowControlRenderer :
        IFlatControlRenderer<Controls.Desktop.WindowControl>
    {
        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
            Controls.Desktop.WindowControl control, IFlatGuiGraphics graphics
            )
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();
            if (control.CustomWindowImage == null)
                graphics.DrawElement("window", ref controlBounds);
            else
            {
                graphics.DrawCustomTexture(control.CustomWindowImage, ref controlBounds, 0, control.DrawGroupId);
            }

            if (control.Title != null)
            {
                graphics.DrawString("window", 0, ref controlBounds, control.Title, false);
            }
        }

    }
}
