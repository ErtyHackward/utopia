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
using System.Drawing;
using CeGui;
using CeGui.Widgets;

namespace CeGui.WidgetSets.Suave {

/// <summary>A window's title bar using the 'suave' look</summary>
[ AlternateWidgetName("SuaveLook.SuaveTitleBar") ]
public class SuaveTitleBar : TitleBar {

  /// <summary>Name of the imageset to use for rendering</summary>
  protected const string ImagesetName = "SuaveLook";

  /// <summary>The image for the left cap of the title bar</summary>
  protected const string LeftEndSectionImageName = "TitleBar-Active-Left";
  /// <summary>The image for the middle section of the title bar</summary>
  protected const string CenterSectionImageName = "TitleBar-Active-Center";
  /// <summary>The image for the right cap of the title bar</summary>
  protected const string RightEndSectionImageName = "TitleBar-Active-Right";

  // Colors
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ActiveColor = new Colour(0xFFFFFFFF);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour InactiveColor = new Colour(0xFFEFEFEF);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour CaptionColor = new Colour(0xFFFBBA6C);
    /// <summary>
    /// 
    /// </summary>
  protected const int TextLayer = 1;

    /// <summary>
    /// 
    /// </summary>
  protected Image leftImage;
    /// <summary>
    /// 
    /// </summary>
  protected Image topImage;
    /// <summary>
    /// 
    /// </summary>
  protected Image bottomImage;
    /// <summary>
    /// 
    /// </summary>
  protected Image rightImage;
    /// <summary>
    /// 
    /// </summary>
  protected Image centerImage;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
  public SuaveTitleBar(string type, string name) : base(type, name) {
    // get images
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    leftImage = imageSet.GetImage(LeftEndSectionImageName);
    centerImage = imageSet.GetImage(CenterSectionImageName);
    rightImage = imageSet.GetImage(RightEndSectionImageName);

    //SetMouseCursor(imageSet.GetImage(NormalCursorImageName));

    AlwaysOnTop = false;
  }
    /// <summary>
    /// 
    /// </summary>
  public override Rect PixelRect {
    get {
      // clip to screen if we have no grand-parent
      if(parent == null || parent.Parent == null) {
        return GuiSystem.Instance.Renderer.Rect.GetIntersection(UnclippedPixelRect);
      }
        // clip to grand parent
    else {
        return parent.Parent.InnerRect.GetIntersection(UnclippedPixelRect);
      }
    }
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="z"></param>
  protected override void DrawSelf(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    // calculate the colors to use
    Colour color = Parent.IsActive ? ActiveColor : InactiveColor;
    ColourRect colors = new ColourRect(color);
    colors.SetAlpha(EffectiveAlpha);

    // calculate widths for the title bar segments
    float leftWidth = leftImage.Width;
    float rightWidth = rightImage.Width;
    float midWidth = absRect.Width - (leftWidth + rightWidth);
    //float topHeight = topImage.Height;
    //float bottomHeight = bottomImage.Height;
    //float midHeight = absRect.Height - (topHeight + bottomHeight);

    // draw the titlebar images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF sz = new SizeF(leftWidth, absRect.Height);
    leftImage.Draw(pos, sz, clipper, colors);
    pos.x += sz.Width;

    sz.Width = midWidth;
    centerImage.Draw(pos, sz, clipper, colors);
    pos.x += sz.Width;

    sz.Width = rightWidth;
    rightImage.Draw(pos, sz, clipper, colors);

    // draw the title text
    if(Font != null) {
      colors = new ColourRect(CaptionColor);
      colors.SetAlpha(EffectiveAlpha);

      Rect textClipper = new Rect(clipper.Left, clipper.Top, clipper.Right, clipper.Bottom);
      textClipper.Width = midWidth;
      textClipper = clipper.GetIntersection(textClipper);

      pos.x = absRect.Left + leftWidth;
      pos.y = absRect.Top + ((absRect.Height - Font.LineSpacing) / 2);
      pos.z = GuiSystem.Instance.Renderer.GetZLayer(TextLayer);

      Font.DrawText(parent.Text, pos, textClipper, colors);
    }
  }

}

} // namespace CeGui.WidgetSets.Suave
