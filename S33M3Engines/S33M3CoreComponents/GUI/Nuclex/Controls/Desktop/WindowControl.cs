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
using S33M3CoreComponents.Sprites2D;
using SharpDX;
using System.Collections.Generic;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Desktop
{

    /// <summary>A window for hosting other controls</summary>
    public class WindowControl : DraggableControl
    {
        /// <summary>
        /// Optional texture to use instead of strandart window
        /// </summary>
        public SpriteTexture CustomWindowImage;

        /// <summary>Initializes a new window control</summary>
        public WindowControl()
            : base(true)
        {
            LeftTopMargin = new Vector2(10, 28);
        }

        /// <summary>Closes the window</summary>
        public void Close()
        {
            if (IsOpen)
            {
                Parent.Children.Remove(this);
            }
        }

        /// <summary>Whether the window is currently open</summary>
        public bool IsOpen
        {
            get { return Screen != null; }
        }

        /// <summary>Whether the window can be dragged with the mouse</summary>
        public new bool EnableDragging
        {
            get { return base.EnableDragging; }
            set { base.EnableDragging = value; }
        }

        /// <summary>Text in the title bar of the window</summary>
        public string Title;

        /// <summary>
        /// Shows form at default position
        /// </summary>
        /// <param name="screen"></param>
        public void Show(MainScreen screen)
        {
            screen.Desktop.Children.Add(this);
        }

        /// <summary>
        /// Shows windows at screen center
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="viewport"></param>
        public void Show(MainScreen screen, SharpDX.Direct3D11.Viewport viewport)
        {
            Bounds.Location.X = (viewport.Width - Bounds.Size.X.Offset) / 2;
            Bounds.Location.Y = (viewport.Height - Bounds.Size.Y.Offset) / 2;

            Show(screen);
        }
    }
}
