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
using System.Text;



namespace Nuclex.UserInterface.Input {

  /// <summary>Input commands that can be sent to a control</summary>
  /// <remarks>
  ///   <para>
  ///     The Nuclex GUI library is designed to work even when none of the usual
  ///     input devices are available. In this case, the entire GUI is controlled
  ///     through command keys, which might for example directly be linked to
  ///     the buttons of a gamepad.
  ///   </para>
  ///   <para>
  ///     It is, of course, still the responsibility of the developer to design
  ///     GUIs in a simple and easy to navigate style. When building GUIs that
  ///     are intended be used without a mouse, it is best not to use complex
  ///     controls like lists or text input boxes.
  ///   </para>
  /// </remarks>
  public enum Command {

    /// <summary>Accept the current selection (Ok button, Enter key)</summary>
    Accept,
    /// <summary>Cancel the current selection (Cancel button, Escape key)</summary>
    Cancel,
    /// <summary>Advance focus to the next control (Tab key)</summary>
    SelectNext,
    /// <summary>Advance focus to the previous control (Shift+Tab key)</summary>
    SelectPrevious,
    /// <summary>Go up or focus control above (Cursor Up key)</summary>
    Up,
    /// <summary>Go down or focus control below (Cursor Down key)</summary>
    Down,
    /// <summary>Go left or focus control left (Cursor Left key)</summary>
    Left,
    /// <summary>Go right or focus control right (Cursor Right key)</summary>
    Right

  }

} // namespace Nuclex.UserInterface.Input
