#region LGPL License
/*************************************************************************
    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*************************************************************************/
#endregion LGPL License

using System;
using CeGui;
using CeGui.Widgets;

namespace CeGui.WidgetSets.Suave {

/// <summary>Gui builder and widget factory for the suave CEGUI theme</summary>
[ AlternateWidgetName("SuaveLook.SuaveGuiBuilder") ]
public class SuaveGuiBuilder : GuiBuilder {

  /// <summary>Initializes the suave gui builder</summary>
  public SuaveGuiBuilder() {
    Name = "Suave";
    ImagesetName = "SuaveLook";
  }

  /// <summary>Creates an uninitialized PushButton</summary>
  /// <param name="name">Name of the button</param>
  /// <returns>The new, uninitialized PushButton</returns>
  public override PushButton CreateButton(string name) {
    return (SuaveButton)WindowManager.Instance.CreateWindow("SuaveLook.SuaveButton", name);
  }

  /// <summary>Creates an uninitialized Checkbox</summary>
  /// <param name="name">Name of the check box</param>
  /// <returns>The new, uninitialized Checkbox</returns>
  public override Checkbox CreateCheckbox(string name) {
    return (SuaveCheckbox)WindowManager.Instance.CreateWindow("SuaveLook.SuaveCheckbox", name);
  }

  /// <summary>Creates an uninitialized RadioButton</summary>
  /// <param name="name">Name of the radio button</param>
  /// <returns>The new, uninitialized RadioButton</returns>
  public override RadioButton CreateRadioButton(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized ComboBox</summary>
  /// <param name="name">Name of the combo box</param>
  /// <returns>The new, uninitialized ComboBox</returns>
  public override ComboBox CreateComboBox(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized ListBox</summary>
  /// <param name="name">Name of the list box</param>
  /// <returns>The new, uninitialized ListBox</returns>
  public override Listbox CreateListBox(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized Grid</summary>
  /// <param name="name">Name of the grid</param>
  /// <returns>The new, uninitialized Grid</returns>
  public override MultiColumnList CreateGrid(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized EditBox</summary>
  /// <param name="name">Name of the edit box</param>
  /// <returns>The new, uninitialized EditBox</returns>
  public override EditBox CreateEditBox(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized FrameWindow</summary>
  /// <param name="name">Name of the frame window</param>
  /// <returns>The new, uninitialized FrameWindow</returns>
  public override FrameWindow CreateFrameWindow(string name) {
    return (SuaveFrameWindow)WindowManager.Instance.CreateWindow("SuaveLook.SuaveFrameWindow", name);
  }

  /// <summary>Creates an uninitialized ProgressBar</summary>
  /// <param name="name">Name of the progress bar</param>
  /// <returns>The new, uninitialized ProgressBar</returns>
  public override ProgressBar CreateProgressBar(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized Slider</summary>
  /// <param name="name">Name of the slider</param>
  /// <returns>The new, uninitialized Slider</returns>
  public override Slider CreateSlider(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized TitleBar</summary>
  /// <param name="name">Name of the title bar</param>
  /// <returns>The new, uninitialized TitleBar</returns>
  public override TitleBar CreateTitleBar(string name) {
    return new SuaveTitleBar("", name);
  }

  /// <summary>Creates an uninitialized Scrollbar</summary>
  /// <param name="name">Name of the scroll bar</param>
  /// <returns>The new, uninitialized Scrollbar</returns>
  public override Scrollbar CreateVertScrollbar(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized Scrollbar</summary>
  /// <param name="name">Name of the scroll bar</param>
  /// <returns>The new, uninitialized Scrollbar</returns>
  public override Scrollbar CreateHorzScrollbar(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized ListHeader</summary>
  /// <param name="name">Name of the list header</param>
  /// <returns>The new, uninitialized ListHeader</returns>
  public override ListHeader CreateListHeader(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized ListHeaderSegment</summary>
  /// <param name="name">Name of the list header segment</param>
  /// <returns>The new, uninitialized ListHeaderSegment</returns>
  public override ListHeaderSegment CreateListHeaderSegment(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

  /// <summary>Creates an uninitialized ComboDropList</summary>
  /// <param name="name">Name of the combo drop list</param>
  /// <returns>The new, uninitialized ComboDropList</returns>
  public override ComboDropList CreateComboDropList(string name) {
    throw new NotImplementedException("Not implemented yet, sorry");
  }

}

} // namespace CeGui.WidgetSets.Suave
