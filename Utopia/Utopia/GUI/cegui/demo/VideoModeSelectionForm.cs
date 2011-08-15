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
using System.Collections.Generic;
using System.Text;

#region class CeGui.Form (helper that makes CeGui behave almost like Windows.Forms)

namespace CeGui {

/// <summary>This makes CeGui behave almost like Windows.Forms :)</summary>
public abstract class Form {

  /// <summary>Initializes the CeGui form class</summary>
  /// <param name="guiBuilder">Gui builder to use for creating widgets</param>
  /// <param name="name">The unique name of this form instance</param>
  public Form(GuiBuilder guiBuilder, string name) {
    this.guiBuilder = guiBuilder;
    this.frame = this.guiBuilder.CreateFrameWindow(name);
  }

  /// <summary>Provides implicit conversion to the CeGui.Window class</summary>
  /// <param name="form">CeGui.Form to be converted</param>
  /// <returns>The CeGui frame window represented by the given CeGui.Form</returns>
  /// <remarks>
  ///   This method enables you to pass your form directly to the CeGui functions
  ///   as if it were the FrameWindow itself. Makes the whole process look a lot
  ///   more like Windows.Forms.
  /// </remarks>
  public static implicit operator CeGui.Window(Form form) {
    return form.frame;
  }

  /// <summary>The CeGui gui builder used to created widgets on this form</summary>
  protected CeGui.GuiBuilder guiBuilder;
  /// <summary>Frame window that this form is representing</summary>
  protected CeGui.Widgets.FrameWindow frame;

}

} // namespace CeGui

#endregion

namespace CeGui.Demo.DirectX {

/// <summary>Example form for the CeGui demonstration application</summary>
public class VideoModeSelectionForm : CeGui.Form {

  /// <summary>Initializes the example form</summary>
  /// <param name="guiBuilder">
  ///   Gui builder that is used to create the widgets on this form
  /// </param>
  public VideoModeSelectionForm(GuiBuilder guiBuilder) : base(guiBuilder, "exampleForm") {
    initializeComponent();
/*
    videoModeList.AddItem("640x480");
    videoModeList.AddItem("800x600");
    videoModeList.AddItem("1024x768");
    videoModeList.AddItem("1280x1024");
    videoModeList.AddItem("1600x1200");
*/
  }

  /// <summary>Handles clicks on the ok button of the dialog</summary>
  /// <param name="sender">Button that has been clicked</param>
  /// <param name="e">Not used</param>
  private void okButtonClicked(object sender, GuiEventArgs e) {
    // moo!
  }

  /// <summary>Handles clicks on the cancel button of the dialog</summary>
  /// <param name="sender">Button that has been clicked</param>
  /// <param name="e">Not used</param>
  private void cancelButtonClicked(object sender, GuiEventArgs e) {
      close();
  }

  /// <summary>Handles clicks on the close button of the dialog</summary>
  /// <param name="sender">Close button that has been clicked</param>
  /// <param name="e">Not used</param>
  private void formCloseClicked(object sender, WindowEventArgs e) {
      close();
   }

  private void close()
  {
      //HACK may be a better way to close a cegui window, but i see no harm in this !
      this.frame.Parent.RemoveChild(this.frame.ID);
  }

  #region Not really Windows Form Designer generated code ;)

  /// <summary>Initializes the childs contained on this window</summary>
  private void initializeComponent() {

    this.guiBuilder.CreateImage();

    this.okButton = this.guiBuilder.CreateButton("okButton");
    this.cancelButton = this.guiBuilder.CreateButton("cancelButton");
    this.fullscreenOption = this.guiBuilder.CreateCheckbox("fullscreenOption");
    //this.videoModeList = newElement<WidgetSets.Windows.WLComboBox>("videoModeList");
    //
    // okButton
    //
    this.okButton.Text = "Ok";
    this.okButton.MetricsMode = MetricsMode.Relative;
    this.okButton.Position = new System.Drawing.PointF(100.0f / 320.0f, 190.0f / 240.0f);
    this.okButton.Size = new System.Drawing.SizeF(96.0f / 320.0f, 32.0f / 240.0f);
    this.okButton.Clicked += new GuiEventHandler(okButtonClicked);
    //
    // cancelButton
    //
    this.cancelButton.Text = "Cancel";
    this.cancelButton.MetricsMode = MetricsMode.Relative;
    this.cancelButton.Position = new System.Drawing.PointF(200.0f / 320.0f, 190.0f / 240.0f);
    this.cancelButton.Size = new System.Drawing.SizeF(96.0f / 320.0f, 32.0f / 240.0f);
    this.cancelButton.Clicked += new GuiEventHandler(cancelButtonClicked);
    //
    // fullscreenOption
    //
    this.fullscreenOption.Text = "Full screen mode";
    this.fullscreenOption.MetricsMode = MetricsMode.Relative;
    this.fullscreenOption.Position = new System.Drawing.PointF(20.0f / 320.0f, 80.0f / 240.0f);
    this.fullscreenOption.Size = new System.Drawing.SizeF(96.0f / 320.0f, 32.0f / 240.0f);
    //
    // videoModeList
    //
    //this.videoModeList.MetricsMode = MetricsMode.Relative;
    //this.videoModeList.Position = new System.Drawing.PointF(0.01f, 0.01f);
    //this.videoModeList.Size = new System.Drawing.SizeF(0.5f, 0.5f);
    //
    // ExampleDialog
    //
    this.frame.AddChild(this.okButton);
    this.frame.AddChild(this.cancelButton);
    this.frame.AddChild(this.fullscreenOption);
    //frame.AddChild(this.videoModeList);
    this.frame.Text = "Example Dialog";
    this.frame.MetricsMode = MetricsMode.Absolute;
    this.frame.Position = new System.Drawing.PointF(100.0f, 100.0f);
    this.frame.Size = new System.Drawing.SizeF(320.0f, 240.0f);
    this.frame.MinimumSize = new System.Drawing.SizeF(320.0f, 240.0f);
    this.frame.CloseClicked += new WindowEventHandler(formCloseClicked);
  }

  #endregion

  private CeGui.Widgets.PushButton okButton;
  private CeGui.Widgets.PushButton cancelButton;
  //private CeGui.Widgets.ComboBox videoModeList;
  private CeGui.Widgets.Checkbox fullscreenOption;

}

} // namespace CeGui.SimpleExample
