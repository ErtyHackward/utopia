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
using S33M3CoreComponents.GUI.Nuclex.Input;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop.Interfaces;


namespace S33M3CoreComponents.GUI.Nuclex.Controls.Desktop
{

    /// <summary>Base class for a slider that can be moved using the mouse</summary>
    /// <remarks>
    ///   Implements the common functionality for a slider moving either the direction
    ///   of the X or the Y axis (but not both). Derive any scroll bar-like controls
    ///   from this class to simplify their implementation.
    /// </remarks>
    public abstract class SliderControl : Control
    {
        public override void Dispose()
        {
            if (Moved != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in Moved.GetInvocationList())
                {
                    Moved -= (EventHandler)d;
                }
            }

            base.Dispose();
        }

        public int ThumbMinValue { get; set; }
        public int ThumbMaxValue { get; set; }
        public bool ThumbSmoothMovement { get; set; }
        private int _value;

        public int Value
        {
            get { return _value; }
            set { _value = value; SetSliderThumb(value); }
        }

        public virtual void SetSliderThumb(int value)
        {
            ThumbPosition = (float)(value - ThumbMinValue) / (float)(ThumbMaxValue - ThumbMinValue);
        }

        /// <summary>Triggered when the slider has been moved</summary>
        public event EventHandler Moved;

        /// <summary>Initializes a new slider control</summary>
        public SliderControl()
        {
            this.ThumbPosition = 0.0f;
            this.ThumbSize = 1.0f;
        }

        /// <summary>whether the mouse is currently hovering over the thumb</summary>
        public bool MouseOverThumb
        {
            get { return this.mouseOverThumb; }
        }

        /// <summary>Whether the pressable control is in the depressed state</summary>
        public virtual bool ThumbDepressed
        {
            get
            {
                return
                  this.pressedDown &&
                  this.mouseOverThumb;
            }
        }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        protected override void OnMousePressed(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                RectangleF thumbRegion = GetThumbRegion();
                if (thumbRegion.Contains(this.pickupX, this.pickupY))
                {
                    this.pressedDown = true;

                    this.pickupX -= thumbRegion.Left;
                    this.pickupY -= thumbRegion.Top;
                }
            }
        }

        /// <summary>Called when a mouse button has been released again</summary>
        /// <param name="button">Index of the button that has been released</param>
        protected override void OnMouseReleased(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                this.pressedDown = false;
            }
        }

        /// <summary>Called when the mouse position is updated</summary>
        /// <param name="x">X coordinate of the mouse cursor on the control</param>
        /// <param name="y">Y coordinate of the mouse cursor on the control</param>
        protected override void OnMouseMoved(float x, float y)
        {
            if (this.pressedDown)
            {

                //RectangleF bounds = GetAbsoluteBounds();
                MoveThumb(x - this.pickupX, y - this.pickupY);

            }
            else
            {
                this.pickupX = x;
                this.pickupY = y;
            }

            this.mouseOverThumb = GetThumbRegion().Contains(x, y);
        }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected override void OnMouseLeft()
        {
            this.mouseOverThumb = false;
        }

        /// <summary>Fires the slider's Moved event</summary>
        protected virtual void OnMoved()
        {
            if (Moved != null)
            {
                Moved(this, EventArgs.Empty);
            }
            if (ThumbSmoothMovement)
            {
                _value = ((int)S33M3CoreComponents.Maths.MathHelper.Lerp(ThumbMinValue, ThumbMaxValue, ThumbPosition));
            }
            else
            {
                Value = ((int)S33M3CoreComponents.Maths.MathHelper.Lerp(ThumbMinValue, ThumbMaxValue, ThumbPosition));
            }
        }

        /// <summary>Moves the thumb to the specified location</summary>
        /// <returns>Location the thumb will be moved to</returns>
        protected abstract void MoveThumb(float x, float y);

        /// <summary>Obtains the region covered by the slider's thumb</summary>
        /// <returns>The region covered by the slider's thumb</returns>
        protected abstract RectangleF GetThumbRegion();

        /// <summary>Can be set by renderers to allow the control to locate its thumb</summary>
        public IThumbLocator ThumbLocator;
        /// <summary>Fraction of the slider filled by the thumb (0.0 .. 1.0)</summary>
        public float ThumbSize;
        /// <summary>Position of the thumb within the slider (0.0 .. 1.0)</summary>
        public float ThumbPosition;

        /// <summary>Whether the mouse cursor is hovering over the thumb</summary>
        private bool mouseOverThumb;
        /// <summary>Whether the slider's thumb is currently in the depressed state</summary>
        private bool pressedDown;
        /// <summary>X coordinate at which the thumb was picked up</summary>
        private float pickupX;
        /// <summary>Y coordinate at which the thumb was picked up</summary>
        private float pickupY;


    }
}
