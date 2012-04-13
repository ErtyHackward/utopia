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
using System.Collections.Generic;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Desktop
{

    /// <summary>Control displaying an option the user can toggle on and off</summary>
    public class OptionControl : PressableControl
    {
        public override void BeforeDispose()
        {
            if (Changed != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in Changed.GetInvocationList())
                {
                    Changed -= (EventHandler)d;
                }
            }
        }

        /// <summary>Will be triggered when the choice is changed</summary>
        public event EventHandler Changed;

        /// <summary>Called when the button is pressed</summary>
        protected override void OnPressed()
        {
            this.Selected = !this.Selected;
            OnChanged();
        }

        /// <summary>Triggers the changed event</summary>
        protected virtual void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        /// <summary>Text that will be shown on the button</summary>
        public string Text;

        /// <summary>Whether the option is currently selected</summary>
        public bool Selected;

    }
}
