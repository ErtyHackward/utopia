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

using System; using SharpDX;
using System.Collections.Generic;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Arcade
{
    /// <summary>Panel that can host other controls, similar to a window</summary>
    public class PanelControl : Control
    {
        private string _frameName = "panel";

        public string FrameName
        {
            get { return _frameName; }
            set { _frameName = value; }
        }

        public bool HidedPanel { get; set; }
    }
}
