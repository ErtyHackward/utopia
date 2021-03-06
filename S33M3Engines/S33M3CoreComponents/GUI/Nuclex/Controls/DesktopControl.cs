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

using System; using SharpDX;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using S33M3CoreComponents.GUI.Nuclex.Input;

namespace S33M3CoreComponents.GUI.Nuclex.Controls
{

    /// <summary>Control used to represent the desktop</summary>
    internal class DesktopControl : Control
    {

        /// <summary>Initializes a new control</summary>
        public DesktopControl() { }

        /// <summary>True if the mouse is currently hovering over a GUI element</summary>
        public bool IsMouseOverGui
        {
            get
            {
                if (base.MouseOverControl == null)
                {
                    return false;
                }
                else
                {
                    return !ReferenceEquals(base.MouseOverControl, this);
                }
            }
        }

        public void ShowAll()
        {
            foreach (var comp in Children) { comp.IsVisible = true; }
        }

        public void HideAll()
        {
            foreach (var comp in Children) { comp.IsVisible = false; }
        }

        /// <summary>Whether the GUI holds ownership of the input devices</summary>
        public bool IsInputCaptured
        {
            get
            {
                if (base.ActivatedControl == null)
                {
                    return false;
                }
                else
                {
                    return !ReferenceEquals(base.ActivatedControl, this);
                }
            }
        }

    }
}
