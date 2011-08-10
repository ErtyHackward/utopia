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

/// <summary>
/// Summary description for SuaveCheckbox.
/// </summary>
[ AlternateWidgetName("SuaveLook.SuaveCheckbox") ]
public class SuaveCheckbox : Checkbox {

  /// <summary>Name of the imageset to use for rendering</summary>
  const string ImagesetName = "SuaveLook";

  /// <summary>Name of the image to use for the unchecked normal state</summary>
  const string NormalUncheckedImageName = "Checkbox-Normal-Unchecked";
  /// <summary>Name of the image to use for the checked normal state</summary>
  const string NormalCheckedImageName = "Checkbox-Normal-Checked";
  /// <summary>Name of the image to use for the mixed normal state</summary>
  const string NormalMixedImageName = "Checkbox-Normal-Mixed";

  /// <summary>Name of the image to use for the unchecked hover state</summary>
  const string HoverUncheckedImageName = "Checkbox-Hover-Unchecked";
  /// <summary>Name of the image to use for the checked hover state</summary>
  const string HoverCheckedImageName = "Checkbox-Hover-Checked";
  /// <summary>Name of the image to use for the mixed hover state</summary>
  const string HoverMixedImageName = "Checkbox-Hover-Mixed";

  /// <summary>Name of the image to use for the unchecked pushed state</summary>
  const string PushedUncheckedImageName = "Checkbox-Pushed-Unchecked";
  /// <summary>Name of the image to use for the checked pushed state</summary>
  const string PushedCheckedImageName = "Checkbox-Pushed-Checked";
  /// <summary>Name of the image to use for the mixed pushed state</summary>
  const string PushedMixedImageName = "Checkbox-Pushed-Mixed";

  /// <summary>Name of the image to use for the unchecked disabled state</summary>
  const string DisabledUncheckedImageName = "Checkbox-Disabled-Unchecked";
  /// <summary>Name of the image to use for the checked disabled state</summary>
  const string DisabledCheckedImageName = "Checkbox-Disabled-Checked";
  /// <summary>Name of the image to use for the mixed disabled state</summary>
  const string DisabledMixedImageName = "Checkbox-Disabled-Mixed";

  /// <summary>
  ///   Pixel padding value for text label (space between image and text label).
  /// </summary>
  const float LabelPadding = 4.0f;

  /// <summary>States the check box can be in</summary>
  private enum States : int {
    /// <summary>Check box does not have the check mark set</summary>
    Unchecked = 0,
    /// <summary>Check box is checked</summary>
    Checked = 1,
    /// <summary>Mixed state (neither checked nor unchecked)</summary>
    Mixed = 2
  }

  /// <summary>Modes in which the check box can be drawn</summary>
  private enum DrawModes : int {
    /// <summary>Normal (sitting around and doing nothing)</summary>
    Normal = 0,
    /// <summary>Used to display that the check box has input focus</summary>
    Hover = 1,
    /// <summary>Used while the check box is button-pushed</summary>
    Pushed = 2,
    /// <summary>Check box is disabled</summary>
    Disabled = 3
  }

  /// <summary>Initializes the check box</summary>
  /// <param name="name"></param>
  /// <param name="type"></param>
  public SuaveCheckbox(string type, string name) : base(type, name) {
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    // Setup cache of image pointers
    this.checkboxImages = new Image[3, 4]; // Array: States x DrawModes

    // Images for the unchecked state
    this.checkboxImages[0, 0] = imageSet.GetImage(NormalUncheckedImageName);
    this.checkboxImages[0, 1] = imageSet.GetImage(HoverUncheckedImageName);
    this.checkboxImages[0, 2] = imageSet.GetImage(PushedUncheckedImageName);
    this.checkboxImages[0, 3] = imageSet.GetImage(DisabledUncheckedImageName);

    // Images for the checked state
    this.checkboxImages[1, 0] = imageSet.GetImage(NormalCheckedImageName);
    this.checkboxImages[1, 1] = imageSet.GetImage(HoverCheckedImageName);
    this.checkboxImages[1, 2] = imageSet.GetImage(PushedCheckedImageName);
    this.checkboxImages[1, 3] = imageSet.GetImage(DisabledCheckedImageName);

    // Images for the mixed state
    this.checkboxImages[2, 0] = imageSet.GetImage(NormalMixedImageName);
    this.checkboxImages[2, 1] = imageSet.GetImage(HoverMixedImageName);
    this.checkboxImages[2, 2] = imageSet.GetImage(PushedMixedImageName);
    this.checkboxImages[2, 3] = imageSet.GetImage(DisabledMixedImageName);
  }

  /// <summary>Helper method for rendering the check box in any mode</summary>
  /// <param name="drawMode">Mode in which to draw the check box</param>
  /// <param name="z">Z value to use</param>
  private void drawCheckbox(DrawModes drawMode, float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped.
    if(clipper.Width == 0)
      return;

    States state = isChecked ? States.Checked : States.Unchecked;
    if(!this.isEnabled)
      drawMode = DrawModes.Disabled;

    // Select the image to be drawn for the check box
    Image image = this.checkboxImages[(int)state, (int)drawMode];

    // Get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    // Draw the images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top + (absRect.Height - image.Height) * 0.5f, z);


    Colour color;
    if(!this.isEnabled) {
        color = DisabledTextColor;
    } else {
        color = NormalTextColor;
    }
    color.Alpha = EffectiveAlpha;

    // calculate colors to use.
    ColourRect colors = new ColourRect(color, color, color, color);



    // HACK: Find out why I need this and Paul doesn't
    pos.z = GuiSystem.Instance.Renderer.GetZLayer(1);

    image.Draw(pos, clipper, colors);

    // Draw label text
    absRect.Top += (absRect.Height - this.Font.LineSpacing) * 0.5f;
    // TODO: Added padding, verify
    absRect.Left += (image.Width + 3);

    this.Font.DrawText(
      this.Text, absRect,
      GuiSystem.Instance.Renderer.GetZLayer(1), clipper, HorizontalTextFormat.Left, colors
    );
  }

  /// <summary>Render the checkbox in the 'normal' state</summary>
  /// <param name="z">Z value for rendering</param>
  protected override void DrawNormal(float z) {
    drawCheckbox(DrawModes.Normal, z);
  }

  /// <summary>Render the checkbox in the 'pushed' state</summary>
  /// <param name="z">Z value for rendering</param>
  protected override void DrawPushed(float z) {
    drawCheckbox(DrawModes.Pushed, z);
  }

  /// <summary>Render the checkbox in the 'hover' state</summary>
  /// <param name="z">Z value for rendering</param>
  protected override void DrawHover(float z) {
    drawCheckbox(DrawModes.Hover, z);
  }

  /// <summary>Images used to draw the checkbox</summary>
  protected Image[
    /* Unchecked, Checked, Mixed */,
    /* Normal, Hover, Pushed, Disabled */
  ] checkboxImages;

}

} // namespace CeGui.WidgetSets.Suave
