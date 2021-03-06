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
using S33M3CoreComponents.GUI.Nuclex.Input;
using System.Windows.Forms;
using S33M3CoreComponents.GUI.Nuclex.Controls.Interfaces;

namespace S33M3CoreComponents.GUI.Nuclex.Controls
{
    /// <summary>User interface element the user can push down</summary>
    public abstract class PressableControl : Control, IFocusable
    {

        /// <summary>Initializes a new command control</summary>
        public PressableControl()
        {
            this.Enabled = true;
        }

        /// <summary>Whether the mouse pointer is hovering over the control</summary>
        public bool MouseHovering
        {
            get { return this.mouseHovering; }
            set { this.mouseHovering = value; }//XXX Simon mod
        }

        /// <summary>Whether the pressable control is in the depressed state</summary>
        public virtual bool Depressed
        {
            get
            {
                bool mousePressed = (this.mouseHovering && this.pressedDownByMouse);
                return
                  mousePressed ||
                  this.pressedDownByKeyboard ||
                  this.pressedDownByKeyboardShortcut ||
                  this.pressedDownByGamepadShortcut;
            }
        }

        /// <summary>Whether the control currently has the input focus</summary>
        public bool HasFocus
        {
            get
            {
                return
                  (Screen != null) &&
                  ReferenceEquals(Screen.FocusedControl, this);
            }
        }

        /// <summary>
        ///   Called when the mouse has entered the control and is now hovering over it
        /// </summary>
        protected override void OnMouseEntered()
        {
            this.mouseHovering = true;
        }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected override void OnMouseLeft()
        {
            // Intentionally not calling OnActivated() here because the user has moved
            // the mouse away from the command while holding the mouse button down -
            // a common trick under windows to last-second-abort the clicking of a button
            this.mouseHovering = false;
        }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        protected override void OnMousePressed(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            if (this.Enabled)
            {
                if (button == S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons.Left)
                {
                    this.pressedDownByMouse = true;
                }
            }
        }

        /// <summary>Called when a mouse button has been released again</summary>
        /// <param name="button">Index of the button that has been released</param>
        protected override void OnMouseReleased(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            if (button == S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons.Left)
            {
                this.pressedDownByMouse = false;

                // Only trigger the pressed event if the mouse was released over the control.
                // The user can move the mouse cursor away from the control while still holding
                // the mouse button down to do the well-known last-second-abort.
                if (this.mouseHovering && this.Enabled)
                {

                    // If this was the final input device holding down the control, meaning it's
                    // not depressed any longer, this counts as a click and we trigger
                    // the notification!
                    if (!Depressed)
                    {
                        OnPressed();
                    }

                }
            }
        }

        /// <summary>Called when a key on the keyboard has been pressed down</summary>
        /// <param name="keyCode">Code of the key that was pressed</param>
        /// <returns>
        ///   True if the key press was handled by the control, otherwise false.
        /// </returns>
        protected override bool OnKeyPressed(Keys keyCode)
        {
            if (HasFocus)
            {
                if (keyCode == Keys.Space)
                {
                    this.pressedDownByKeyboard = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Called when a key on the keyboard has been released again</summary>
        /// <param name="keyCode">Code of the key that was released</param>
        protected override void OnKeyReleased(Keys keyCode)
        {
            if (this.pressedDownByKeyboard)
            {
                if (keyCode == Keys.Space)
                {
                    this.pressedDownByKeyboard = false;
                    if (!Depressed)
                    {
                        OnPressed();
                    }
                }
            }
        }

        /// <summary>Called when the control is pressed</summary>
        /// <remarks>
        ///   If you were to implement a button, for example, you could trigger a 'Pressed'
        ///   event here are call a user-provided delegate, depending on your design.
        /// </remarks>
        protected virtual void OnPressed() { }

        /// <summary>Whether the control can currently obtain the input focus</summary>
        bool IFocusable.CanGetFocus
        {
            get { return this.Enabled; }
        }

        /// <summary>Whether the user can interact with the choice</summary>
        public bool Enabled;

        /// <summary>Whether the command is pressed down using the space key</summary>
        private bool pressedDownByKeyboard = false;
        /// <summary>Whether the command is pressed down using the keyboard shortcut</summary>
        private bool pressedDownByKeyboardShortcut = false;
        /// <summary>Whether the command is pressed down using the game pad shortcut</summary>
        private bool pressedDownByGamepadShortcut = false;
        /// <summary>Whether the command is pressed down using the mouse</summary>
        private bool pressedDownByMouse = false;
        /// <summary>Whether the mouse is hovering over the command</summary>
        private bool mouseHovering;


    }
}
