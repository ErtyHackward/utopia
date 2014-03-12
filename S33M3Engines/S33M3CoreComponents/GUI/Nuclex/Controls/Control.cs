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
using System.Linq;
using System.Windows.Forms.VisualStyles;
using S33M3DXEngine.Main;
using SharpDX;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using S33M3CoreComponents.GUI.Nuclex.Input;
using System.Windows.Forms;
using S33M3Resources.Structs;
using Color = SharpDX.Color;

namespace S33M3CoreComponents.GUI.Nuclex.Controls
{

    /// <summary>Represents an element in the user interface</summary>
    /// <remarks>
    ///   <para>
    ///     Controls are always arranged in a tree where each control except the one at
    ///     the root of the tree has exactly one owner (the one at the root has no owner).
    ///     The design actively prevents you from assigning a control as child to
    ///     multiple parents.
    ///   </para>
    ///   <para>
    ///     The controls in the S33M3CoreComponents.GUI.Nuclex library are fully independent of
    ///     their graphical representation. That means you can construct a dialog
    ///     without even having a graphics device in place, that you can move your
    ///     dialogs between different graphics devices and that you do not have to
    ///     even think about graphics device resets and similar trouble.
    ///   </para>
    /// </remarks>
    public partial class Control : BaseComponent
    {
        private ByteColor _color = SharpDX.Color.White;
        public bool ColorSet { get; private set; }

        public virtual int DrawGroupId { get; set; }

        private bool _isRendable = true;
        /// <summary>
        /// Will prevent the control the be rendered, but its children will be evaluated
        /// </summary>
        public bool IsRendable
        {
            get { return _isRendable; }
            set { _isRendable = value; }
        }

        private bool _isVisible = true;
        /// <summary>
        /// Will prevent a control to be rendered, the children won't be rendered too (even if visible !!!)
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        public ByteColor Color
        {
            get { return _color; }
            set { _color = value; ColorSet = true; }
        }

        /// <summary>
        /// If true the control will fire tooltip event
        /// </summary>
        public virtual bool ToolTipEnabled { get; set; }

        public override void BeforeDispose()
        {
            if (this.parent != null) this.RemoveFromParent();

            if (Clicked != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in Clicked.GetInvocationList())
                {
                    Clicked -= (EventHandler)d;
                }
            }
        }

        /// <summary>
        /// Occurs when the control get clicked
        /// </summary>
        public event EventHandler Clicked;

        public void OnClicked()
        {
            var handler = Clicked;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Initializes a new control</summary>
        public Control() : this(false) { }

        /// <summary>Initializes a new control</summary>
        /// <param name="affectsOrdering">
        ///   Whether the control comes to the top of the hierarchy when clicked
        /// </param>
        /// <remarks>
        ///   <para>
        ///     The <paramref name="affectsOrdering" /> parameter should be set for windows
        ///     and other free-floating panels which exist in parallel and which the user
        ///     might want to put on top of their siblings by clicking them. If the user
        ///     clicks on a child control of such a panel/window control, the panel/window
        ///     control will also be moved into the foreground.
        ///   </para>
        ///   <para>
        ///     It should not be set for normal controls which usually have no overlap,
        ///     like buttons. Otherwise, a button placed on the desktop could overdraw a
        ///     window when the button is clicked. The behavior would be well-defined and
        ///     controlled, but the user probably doesn't expect this ;-)
        ///   </para>
        /// </remarks>
        protected Control(bool affectsOrdering)
        {
            this.affectsOrdering = affectsOrdering;

            this.children = new ParentingControlCollection(this);
        }

        /// <summary>Children of the control</summary>
        public Collection<Control> Children
        {
            get { return this.children; }
        }

        /// <summary>
        ///   True if clicking the control or its children moves the control into
        ///   the foreground of the drawing hierarchy
        /// </summary>
        public bool AffectsOrdering
        {
            get { return this.affectsOrdering; }
        }

        public bool IsClickTransparent { get; set; }

        /// <summary>Parent control this control is contained in</summary>
        /// <remarks>
        ///   Can be null, but this is only the case for free-floating controls that have
        ///   not been added into a Gui. The only control that really keeps this field
        ///   set to null whilst the Gui is active is the root control in the Gui class.
        /// </remarks>
        public Control Parent
        {
            get { return this.parent; }
        }

        public new object Tag { get; set; }
        public object Tag2 { get; set; }

        /// <summary>Name that can be used to uniquely identify the control</summary>
        /// <remarks>
        ///   This name acts as an unique identifier for a control. It primarily serves
        ///   as a means to programmatically identify the control and as a debugging aid.
        ///   Duplicate names are not allowed and will result in an exception being
        ///   thrown, the only exception is when the control's name is set to null.
        /// </remarks>
        public new string Name
        {
            get { return this.name; }
            set
            {

                // Don't do anything if we're given the same name we already have. This
                // is not a pure performance optimization, it also prevents the control
                // from reporting an name collision with itself in this special case :)
                if (value != this.name)
                {

                    // Look for name collisions with our siblings
                    Control parent = Parent;
                    if (parent != null)
                        if (parent.children.IsNameTaken(value))
                            throw new DuplicateNameException("Another control is already using this name");

                    // Everything seems to be ok, accept the new name
                    this.name = value;

                }
            }
        }

        /// <summary>Moves the control into the foreground</summary>
        public void BringToFront()
        {

            // Doing nothing if we don't have a parent is okay since in that case,
            // we're the root and we're the frontmost control in any case. If the user
            // calls BringToFront() on a control before he integrates it into the GUI
            // tree, this is expected behavior and only logical.
            Control control = this;
            while (!ReferenceEquals(control.parent, null))
            {
                ParentingControlCollection siblings = control.parent.children;
                siblings.MoveToStart(siblings.IndexOf(control));

                control = control.parent;
            }

        }

        /// <summary>
        ///   Obtains the absolute boundaries of the control in screen coordinates
        /// </summary>
        /// <returns>The control's absolute screen coordinate boundaries</returns>
        /// <remarks>
        ///   This method resolves the unified coordinates into absolute screen coordinates
        ///   that can be used to do hit-testing and rendering. The control is required to
        ///   be part of a GUI hierarchy that is assigned to a screen for this to work
        ///   since otherwise, there's no absolute coordinate frame into which the
        ///   unified coordinates could be resolved.
        /// </remarks>
        public RectangleF GetAbsoluteBounds()
        {

            // Is this the topmost control in the hierarchy (the desktop control)?
            if (ReferenceEquals(this.parent, null))
            {

                // Make sure the control is attached to a screen, otherwise, it's a free
                // control not living in any GUI hierarchy and thus, does not have
                // absolute bounds yet.
                if (ReferenceEquals(this.screen, null))
                {
                    throw new InvalidOperationException(
                      "Obtaining absolute bounds requires the control to be part of a screen"
                    );
                }

                // Transform the unified coordinate bounds into absolute pixel coordinates
                // for the screen's dimensions
                return this.Bounds.ToOffset(this.screen.Width, this.screen.Height);

            }
            else
            { // Control is the child of another control

                // Recursively determine the bounds of the parent control until we end up
                // at the desktop control (or not, if this is a free living hierarchy, in
                // which case the exception above will be triggered as soon as the top of
                // the hierarchy is reached)
                RectangleF parentBounds = this.parent.GetAbsoluteBounds();

                // Determine the controls absolute position based on the absolute
                // dimensions and position of the parent control
                RectangleF controlBounds = this.Bounds.ToOffset(
                  parentBounds.Width + this.Bounds.Size.X.ParentOffset, parentBounds.Height + this.Bounds.Size.Y.ParentOffset
                );
                controlBounds.Offset(parentBounds.X + this.Bounds.Location.X.ParentOffset, parentBounds.Y + this.Bounds.Location.Y.ParentOffset);

                // Done, controlBounds now contains the absolute screen coordinates of
                // the control's boundaries.
                return controlBounds;

            }

        }

        /// <summary>Called when an input command was sent to the control</summary>
        /// <param name="command">Input command that has been triggered</param>
        /// <returns>Whether the command has been processed by the control</returns>
        protected virtual bool OnCommand(Command command) { return false; }

        /// <summary>Called when a button on the gamepad has been pressed</summary>
        /// <param name="button">Button that has been pressed</param>
        /// <returns>
        ///   True if the button press was handled by the control, otherwise false.
        /// </returns>
        /// <remarks>
        ///   If the control indicates that it didn't handle the key press, it will not
        ///   receive the associated key release notification.
        /// </remarks>
        // protected virtual bool OnButtonPressed(Buttons button) { return false; }

        /// <summary>Called when a button on the gamepad has been released</summary>
        /// <param name="button">Button that has been released</param>
        //   protected virtual void OnButtonReleased(Buttons button) { }

        /// <summary>Called when the mouse position is updated</summary>
        /// <param name="x">X coordinate of the mouse cursor on the control</param>
        /// <param name="y">Y coordinate of the mouse cursor on the control</param>
        protected virtual void OnMouseMoved(float x, float y) { }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        /// <returns>Whether the control has processed the mouse press</returns>
        /// <remarks>
        ///   If this method states that a mouse press is processed by returning
        ///   true, that means the control did something with it and the mouse press
        ///   should not be acted upon by any other listener.
        /// </remarks>
        protected virtual void OnMousePressed(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button) { }

        /// <summary>Called when a mouse button has been released again</summary>
        /// <param name="button">Index of the button that has been released</param>
        protected virtual void OnMouseReleased(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button) { OnClicked(); }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected virtual void OnMouseLeft()
        {

        }

        /// <summary>
        ///   Called when the mouse has entered the control and is now hovering over it
        /// </summary>
        protected virtual void OnMouseEntered()
        {

        }

        /// <summary>Called when the mouse wheel has been rotated</summary>
        /// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
        protected virtual void OnMouseWheel(float ticks) { }

        /// <summary>Called when a key on the keyboard has been pressed down</summary>
        /// <param name="keyCode">Code of the key that was pressed</param>
        /// <returns>
        ///   True if the key press was handled by the control, otherwise false.
        /// </returns>
        /// <remarks>
        ///   If the control indicates that it didn't handle the key press, it will not
        ///   receive the associated key release notification. This means that if you
        ///   return false from this method, you should under no circumstances do anything
        ///   with the information - you will not know when the key is released again
        ///   and another control might pick it up, causing a second key response.
        /// </remarks>
        protected virtual bool OnKeyPressed(Keys keyCode) { return false; }

        /// <summary>Called when a key on the keyboard has been released again</summary>
        /// <param name="keyCode">Code of the key that was released</param>
        protected virtual void OnKeyReleased(Keys keyCode) { }

        /// <summary>GUI instance this control belongs to. Can be null.</summary>
        internal MainScreen Screen
        {
            get { return this.screen; }
        }

        /// <summary>Called when a command was sent to the control</summary>
        /// <param name="command">Command to be injected</param>
        /// <returns>Whether the command has been processed</returns>
        internal bool ProcessCommand(Command command)
        {

            switch (command)
            {

                // These are not supported on the control level
                case Command.SelectPrevious:
                case Command.SelectNext:
                    {
                        return false;
                    }

                // These can be handled by user code if he so wishes
                case Command.Up:
                case Command.Down:
                case Command.Left:
                case Command.Right:
                case Command.Accept:
                case Command.Cancel:
                    {
                        return OnCommand(command);
                    }

                // Value not contained in enumation - should not be happening!
                default:
                    {
                        throw new ArgumentException("Invalid command", "command");
                    }
            }
        }

        /// <summary>Assigns a new parent to the control</summary>
        /// <param name="parent">New parent to assign to the control</param>
        internal void SetParent(Control parent)
        {
            this.parent = parent;

            // Have we been assigned to a parent?
            if (this.parent != null)
            {

                // If this ownership change transferred us to a different gui, we will
                // have to migrate our visual and also the visuals of all our children.
                if (!ReferenceEquals(this.screen, parent.screen))
                    SetScreen(parent.screen);

            }
            else
            { // No parent, we're now officially an orphan ;)

                // Orphans don't have screens!
                SetScreen(null);

            }
        }

        /// <summary>Assigns a new GUI to the control</summary>
        /// <param name="gui">New GUI to assign to the control</param>
        internal void SetScreen(MainScreen gui)
        {
            this.screen = gui;

            this.children.SetScreen(gui);
        }

        /// <summary>Control the mouse is currently over</summary>
        internal protected Control MouseOverControl
        {
            get { return this.mouseOverControl; }
        }

        /// <summary>Control that currently captured incoming input</summary>
        internal protected Control ActivatedControl
        {
            get { return this.activatedControl; }
        }

        public void RemoveFromParent()
        {
            this.parent.Children.Remove(this);
            this.SetParent(null);
        }

        /// <summary>
        /// Updates layout of childs controls
        /// </summary>
        public void UpdateLayout()
        {
            _layoutManager.LayoutControls(this);
        }

        /// <summary>
        /// Automatically locates the control relatively to the parent
        /// </summary>
        /// <param name="ds"></param>
        public void Locate(ControlDock ds)
        {
            if (ds.HasFlag(ControlDock.HorisontalLeft))
            {
                Bounds.Location.X = parent.Bounds.Location.X.Offset;
            }
            if (ds.HasFlag(ControlDock.HorisontalCenter))
            {
                Bounds.Location.X = parent.Bounds.Location.X + (parent.Bounds.Size.X - Bounds.Size.X) / 2;
            }
            if (ds.HasFlag(ControlDock.HorisontalRight))
            {
                Bounds.Location.X = parent.Bounds.Location.X.Offset + (parent.Bounds.Size.X.Offset - Bounds.Size.X.Offset);
            }
            if (ds.HasFlag(ControlDock.VerticalTop))
            {
                Bounds.Location.Y = parent.Bounds.Location.Y.Offset;
            }
            if (ds.HasFlag(ControlDock.VerticalCenter))
            {
                Bounds.Location.Y = parent.Bounds.Location.Y.Offset + (parent.Bounds.Size.Y.Offset - Bounds.Size.Y.Offset) / 2;
            }
            if (ds.HasFlag(ControlDock.VerticalBottom))
            {
                Bounds.Location.Y = parent.Bounds.Location.Y + (parent.Bounds.Size.Y - Bounds.Size.Y);
            }
        }

        private static ControlLayout _layoutManager = new ControlLayout();

        /// <summary>Location and extents of the control</summary>
        public UniRectangle Bounds;

        /// <summary>Control this control is contained in</summary>
        private Control parent;
        /// <summary>GUI instance this control has been added to. Can be null.</summary>
        private MainScreen screen;
        /// <summary>Name of the control instance (for programmatic identification)</summary>
        private string name;
        /// <summary>Whether this control can obtain the input focus</summary>
        private bool affectsOrdering;
        /// <summary>Child controls belonging to this control</summary>
        /// <remarks>
        ///   Child controls are any controls that belong to this control. They don't
        ///   neccessarily need to be situated in this control's client area, but
        ///   their positioning will be relative to the parent's location.
        /// </remarks>
        private ParentingControlCollection children;

        public ControlLayoutFlags LayoutFlags;

        public Vector2 LeftTopMargin = new Vector2(5, 5);

        public Vector2 RightBottomMargin = new Vector2(5, 5);

        /// <summary>
        /// Vertical and horisontal space between controls
        /// </summary>
        public Vector2 ControlsSpacing = new Vector2(5, 5);

        /// <summary>
        /// Limit for controls with free sides
        /// </summary>
        public Vector2 MinimumSize = new Vector2(20, 10);

        public VerticalAlignment ChildsAlignment = VerticalAlignment.Top;
    }

    [Flags]
    public enum ControlDock
    {
        None = 0x0,
        VerticalTop = 0x1,
        VerticalBottom = 0x2,
        VerticalCenter = 0x4,
        HorisontalLeft = 0x8,
        HorisontalRight = 0x10,
        HorisontalCenter = 0x20
    }

    [Flags]
    public enum ControlLayoutFlags
    {
        None = 0x0,
        FreeWidth = 0x1,
        FreeHeight = 0x2,
        WholeRow = 0x4,
        WholeRowCenter = WholeRow | Center,
        Skip = 0x10,
        Center = 0x20
    }

    public class ControlLayout
    {
        public ControlLayout()
        {
            Rows = new List<ControlsRow>();
        }

        public List<ControlsRow> Rows { get; set; }

        public void LayoutControls(Control parent)
        {
            RectangleF parentBounds;
            if (parent.Screen == null)
            {
                parentBounds = parent.Bounds.ToOffset(0,0);
            }
            else
            {
                parentBounds = parent.GetAbsoluteBounds();
            }

            var controlsSpace = new Vector2(parentBounds.Width - parent.LeftTopMargin.X - parent.RightBottomMargin.X, parentBounds.Height - parent.LeftTopMargin.Y - parent.RightBottomMargin.Y);

            Rows.Clear();
            var currentRow = new ControlsRow(controlsSpace.X);
            Rows.Add(currentRow);

            // put controls in rows
            foreach (var control in parent.Children)
            {
                if (control.LayoutFlags.HasFlag(ControlLayoutFlags.Skip))
                    continue;

                if (control.LayoutFlags.HasFlag(ControlLayoutFlags.WholeRow))
                {
                    if (currentRow.Controls.Count > 0)
                    {
                        currentRow = new ControlsRow(controlsSpace.X);
                        Rows.Add(currentRow);
                    }

                    currentRow.AddControl(control);

                    currentRow = new ControlsRow(controlsSpace.X);
                    Rows.Add(currentRow);
                }
                else
                {
                    if (!currentRow.AddControl(control))
                    {
                        currentRow = new ControlsRow(controlsSpace.X);
                        Rows.Add(currentRow);
                        currentRow.AddControl(control);
                    }
                }
            }

            if (currentRow.Controls.Count == 0)
                Rows.Remove(currentRow);

            var minHeight = Rows.Sum(r => r.Height) + Rows.Count * parent.ControlsSpacing.Y;
            var freeRows = Rows.Count(r => r.FreeHeight);
            if (controlsSpace.Y > minHeight && freeRows > 0)
            {
                // we have additinal space, distribute it
                var addition = (controlsSpace.Y - minHeight) / freeRows;

                foreach (var row in Rows.Where(r => r.FreeHeight))
                {
                    row.SetHeight(row.Height + addition);
                }
            }

            // update controls 
            var y = parent.LeftTopMargin.Y;

            if (parent.ChildsAlignment == VerticalAlignment.Center)
            {
                var bounds = parent.GetAbsoluteBounds();
                y = ( bounds.Height - minHeight ) / 2;
            }

            foreach (var controlsRow in Rows)
            {
                var x = parent.LeftTopMargin.X;

                foreach (var control in controlsRow.Controls)
                {
                    if (control.LayoutFlags.HasFlag(ControlLayoutFlags.Center))
                    {
                        control.Bounds.Left = x + (controlsSpace.X - controlsRow.Width) / 2;
                    }
                    else
                    {
                        control.Bounds.Left = x;
                    }
                    control.Bounds.Top = y;

                    x += control.Bounds.Size.X.Offset + parent.ControlsSpacing.X;
                }

                y += controlsRow.Height + parent.ControlsSpacing.Y;
            }

        }
    }

    public class ControlsRow
    {
        public float Width;
        public float MaxWidth;
        public float Height;
        public bool FreeHeight;

        public ControlsRow(float maxWidth)
        {
            MaxWidth = maxWidth;
            Controls = new List<Control>();
        }

        public List<Control> Controls { get; set; }

        public bool AddControl(Control c)
        {
            if (Controls.Count > 0 && Width + c.Bounds.Size.X.Offset > MaxWidth)
            {
                return false;
            }

            if (c.LayoutFlags.HasFlag(ControlLayoutFlags.FreeHeight))
            {
                FreeHeight = true;
                c.Bounds.Size.Y = c.MinimumSize.Y;
            }

            Height = Math.Max(Height, c.Bounds.Size.Y.Offset);
            Width += c.Bounds.Size.X.Offset;

            Controls.Add(c);

            return true;
        }

        internal void SetHeight(float p)
        {
            foreach (var control in Controls)
            {
                if (control.LayoutFlags.HasFlag(ControlLayoutFlags.FreeHeight))
                {
                    control.Bounds.Size.Y = p;
                }
            }

            Height = p;
        }
    }

    public static class NuclexHelper
    {
        /// <summary>
        /// Gets a control with specified name and type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="childrens"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(this Collection<Control> childrens, string name) where T : class
        {
            foreach (var children in childrens)
            {
                if (children is T && children.Name == name) return children as T;
            }
            return null;
        }
    }
}
