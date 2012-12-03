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
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Input;
using System.Windows.Forms;
using S33M3CoreComponents.GUI.Nuclex.Controls.Interfaces;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop.Interfaces;
using S33M3Resources.Structs;
using S33M3CoreComponents.Unsafe;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Desktop
{

    public class InputKeyCatchControl : Control, IKeyPressLookUp, IFocusable
    {
        public override void BeforeDispose()
        {
            if (KeyChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in KeyChanged.GetInvocationList())
                {
                    KeyChanged -= (EventHandler)d;
                }
            }
        }

        public event EventHandler KeyChanged;

        /// <summary>
        /// Gets or sets an optional background image
        /// </summary>
        public SpriteTexture CustomBackground { get; set; }


        public ByteColor HasFocusBackColor = new ByteColor(205, 92, 92);

        /// <summary>
        /// Gets or sets an image to be displayed when the control have no text and no focus
        /// </summary>
        public SpriteTexture CustomHintImage { get; set; }

        /// <summary>
        /// Gets or sets an optional custom font of the Input control
        /// </summary>
        public SpriteFont CustomFont { get; set; }

        /// <summary>Initializes a new text input control</summary>
        public InputKeyCatchControl()
        {
            this.singleCharArray = new char[1];
            this.text = new StringBuilder(64);

            this.Enabled = true;
        }

        /// <summary>Text that is being displayed on the control</summary>
        public string Text
        {
            get { return this.text.ToString(); }
            set
            {
                this.text.Clear();
                this.text.Append(value);
                this.Color = new ByteColor(0, 0, 0);
                if (KeyChanged != null) KeyChanged(this, null);
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
       
        /// <summary>Called when the mouse position is updated</summary>
        /// <param name="x">X coordinate of the mouse cursor on the control</param>
        /// <param name="y">Y coordinate of the mouse cursor on the control</param>
        protected override void OnMouseMoved(float x, float y)
        {
            this.mouseX = x;
            this.mouseY = y;
        }

        protected override void OnMouseLeft()
        {
        }

        public void ProcessPressKeyLookUp(Keys keyCode)
        {
            //Process the key only If I have the focus !
            if (HasFocus)
            {
                processKeyDown(keyCode);
            }
        }

        private void processKeyDown(Keys keyCode)
        {
            //Get selected cell
            bool isLShift, isRShift, isLControl, isRControl;
            string ModifierStr = string.Empty;
            isLShift = (UnsafeNativeMethods.GetKeyState(0xA0) & 0x80) != 0; // VK_LSHIFT    
            isRShift = (UnsafeNativeMethods.GetKeyState(0xA1) & 0x80) != 0; // VK_RSHIFT        
            isLControl = (UnsafeNativeMethods.GetKeyState(162) & 0x80) != 0; // VK_LCONTROL              
            isRControl = (UnsafeNativeMethods.GetKeyState(0xA3) & 0x80) != 0; // VK_RCONTROL              

            if (isLShift) ModifierStr += ModifierStr.Length > 0 ? "+ LShiftKey" : "LShiftKey";
            if (isRShift) ModifierStr += ModifierStr.Length > 0 ? "+ RShiftKey" : "RShiftKey";
            if (isLControl) ModifierStr += ModifierStr.Length > 0 ? "+ LControlKey" : "LControlKey";
            if (isRControl) ModifierStr += ModifierStr.Length > 0 ? "+ RControlKey" : "RControlKey";

            string Keypressed = keyCode.ToString();

            //If it is only a Modifier key pressed
            if (Keypressed.Contains("Key"))
            {
                this.Text = ModifierStr;
            }
            else
            {
                Keypressed = Keypressed.Split(',')[0];
                if (ModifierStr.Length > 0) ModifierStr = " + " + ModifierStr;
                this.Text = Keypressed + ModifierStr;
            }
        }

        /// <summary>Whether the control can currently obtain the input focus</summary>
        public bool CanGetFocus
        {
            get { return this.Enabled; }
        }

        /// <summary>Whether user interaction with the control is allowed</summary>
        public bool Enabled;
        /// <summary>
        ///   Can be set by renderers to enable cursor positioning by the mouse
        /// </summary>
        public IOpeningLocator OpeningLocator;
        /// <summary>Array used to store characters before they are appended</summary>
        private char[/*1*/] singleCharArray;
        /// <summary>Text the user has entered into the text input control</summary>
        private StringBuilder text;
        /// <summary>X coordinate of the last known mouse position</summary>
        private float mouseX;
        /// <summary>Y coordinate of the last known mouse position</summary>
        private float mouseY;


    }
}
