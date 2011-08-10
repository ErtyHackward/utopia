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

/// <summary>A frame window using the 'suave' look</summary>
[ AlternateWidgetName("SuaveLook.SuaveFrameWindow") ]
public class SuaveFrameWindow : FrameWindow {
  #region Constants

  /// <summary>Name of the imageset to use for rendering</summary>
  protected const string ImagesetName = "SuaveLook";

  /// <summary>The image for the top left corner of the window</summary>
  protected const string ActiveTopLeftImageName = "Window-Active-TopLeft";
  /// <summary>The image for the left side of the window</summary>
  protected const string ActiveLeftImageName = "Window-Active-Left";
  /// <summary>The image for the bottom left corner of the window</summary>
  protected const string ActiveBottomLeftImageName = "Window-Active-BottomLeft";
  /// <summary>The image for the top side of the window</summary>
  protected const string ActiveTopImageName = "Window-Active-Top";
  /// <summary>The image for the top side of the window</summary>
  protected const string ActiveCenterImageName = "Window-Active-Center";
  /// <summary>The image for the bottom side of the window</summary>
  protected const string ActiveBottomImageName = "Window-Active-Bottom";
  /// <summary>The image for the top right corner of the window</summary>
  protected const string ActiveTopRightImageName = "Window-Active-TopRight";
  /// <summary>The image for the right side of the window</summary>
  protected const string ActiveRightImageName = "Window-Active-Right";
  /// <summary>The image for the bottom right corner of the window</summary>
  protected const string ActiveBottomRightImageName = "Window-Active-BottomRight";

  /// <summary>
  /// 
  /// </summary>
  protected const string CloseButtonNormalImageName = "CloseButton-Normal";
    /// <summary>
    /// 
    /// </summary>
  protected const string CloseButtonHoverImageName = "CloseButton-Hover";
    /// <summary>
    /// 
    /// </summary>
  protected const string CloseButtonPushedImageName = "CloseButton-Pushed";

  // cursor images
    /// <summary>
    /// 
    /// </summary>
  protected const string NormalCursorImageName = "Mouse-Arrow";
    /// <summary>
    /// 
    /// </summary>
  protected const string NorthSouthCursorImageName = "Mouse-Size90";
    /// <summary>
    /// 
    /// </summary>
  protected const string EastWestCursorImageName = "Mouse-Size0";
    /// <summary>
    /// 
    /// </summary>
  protected const string NWestSEastCursorImageName = "Mouse-Size135";
    /// <summary>
    /// 
    /// </summary>
  protected const string NEastSWestCursorImageName = "Mouse-Size45";

  // window type stuff
    /// <summary>
    /// 
    /// </summary>
  protected const string TitlebarType = "SuaveLook/SuaveTitlebar";
    /// <summary>
    /// 
    /// </summary>
  protected const string CloseButtonType = "SuaveLook/SuaveCloseButton";

  // layout constants
    /// <summary>
    /// 
    /// </summary>
  protected const float TitlebarXOffset = 0;
    /// <summary>
    /// 
    /// </summary>
  protected const float TitlebarYOffset = 0;
    /// <summary>
    /// 
    /// </summary>
  protected const float TitlebarTextPadding = 6;
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ActiveColor = new Colour(0xFFA7C7FF);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour InactiveColor = new Colour(0xFFEFEFEF);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ClientTopLeftColor = new Colour(0xFFDFDFF5);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ClientTopRightColor = new Colour(0xFFDFEFF5);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ClientBottomLeftColor = new Colour(0xFFF4F3F5);
    /// <summary>
    /// 
    /// </summary>
  protected static Colour ClientBottomRightColor = new Colour(0xFFF0F0F5);

  #endregion Constants

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="type"></param>
  public SuaveFrameWindow(string type, string name) : base(type, name) {

    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    // setup frame images
    frame.SetImages(
      imageSet.GetImage(ActiveTopLeftImageName),
      imageSet.GetImage(ActiveTopRightImageName),
      imageSet.GetImage(ActiveBottomLeftImageName),
      imageSet.GetImage(ActiveBottomRightImageName),
      imageSet.GetImage(ActiveLeftImageName),
      imageSet.GetImage(ActiveTopImageName),
      imageSet.GetImage(ActiveRightImageName),
      imageSet.GetImage(ActiveBottomImageName)
    );

    StoreFrameSizes();

    // setup client area clearing brush
    clientBrush.Image = imageSet.GetImage(ActiveCenterImageName);
    clientBrush.Position = new PointF(frameLeftSize, frameTopSize);
    clientBrush.HorizontalFormat = HorizontalImageFormat.Stretched;
    clientBrush.VerticalFormat = VerticalImageFormat.Stretched;
    clientBrush.SetColors(ClientTopLeftColor, ClientTopRightColor,
                                  ClientBottomLeftColor, ClientBottomRightColor);

    // setup cursor images for this window
    SetMouseCursor(imageSet.GetImage(NormalCursorImageName));
    sizingCursorNS = imageSet.GetImage(NorthSouthCursorImageName);
    sizingCursorEW = imageSet.GetImage(EastWestCursorImageName);
    sizingCursorNWSE = imageSet.GetImage(NWestSEastCursorImageName);
    sizingCursorNESW = imageSet.GetImage(NEastSWestCursorImageName);
  }

  #endregion Constructor

  /// <summary>
  ///		Store the sizes for the frame edges.
  /// </summary>
  protected void StoreFrameSizes() {
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    frameLeftSize = imageSet.GetImage(ActiveLeftImageName).Width;
    frameRightSize = imageSet.GetImage(ActiveRightImageName).Width;
    frameTopSize = imageSet.GetImage(ActiveTopImageName).Height;
    frameBottomSize = imageSet.GetImage(ActiveBottomImageName).Height;
  }
    /// <summary>
    /// 
    /// </summary>
  protected void UpdateFrameColors() {
    Colour color = IsActive ? ActiveColor : InactiveColor;
    frame.SetColors(new ColourRect(color));
    frame.Colors.SetAlpha(EffectiveAlpha);
  }

  /// <summary>
  ///		Return a Rect object that describes, unclipped, the inner rectangle for this window.	
  /// </summary>
  public override Rect UnclippedInnerRect {
    get {
      Rect tempRect = UnclippedPixelRect;

      if(FrameEnabled) {
        PointF pos = frame.Position;

        tempRect.Left += pos.X + frameLeftSize;
        tempRect.Right -= frameRightSize;
        tempRect.Top += pos.Y + frameTopSize;
        tempRect.Bottom -= frameBottomSize;
      }

      return tempRect;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override PushButton CreateCloseButton() {
    SuaveCloseButton button = (SuaveCloseButton)WindowManager.Instance.CreateWindow(
        "SuaveLook.SuaveCloseButton", name + "_auto_PushButton");

    button.StandardImageryEnabled = false;
    button.CustomImageryAutoSized = true;

    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    // setup close button imagery
    RenderableImage image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonNormalImageName);
    image.SetColors(new ColourRect(new Colour(0xFFFFFFFF)));
    button.SetNormalImage(image);

    image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonNormalImageName);
    image.SetColors(new ColourRect(new Colour(0x7F3FAFAF)));
    button.SetDisabledImage(image);

    image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonHoverImageName);
    image.SetColors(new ColourRect(new Colour(0xFFFFFFFF)));
    button.SetHoverImage(image);

    image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonPushedImageName);
    image.SetColors(new ColourRect(new Colour(0xFFFFFFFF)));
    button.SetPushedImage(image);

    button.Alpha = 0.5f;
    button.MetricsMode = MetricsMode.Absolute;
    button.AlwaysOnTop = true;

    return button;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override TitleBar CreateTitleBar() {
    // create a titlebar to use for this frame window
    TitleBar window = (TitleBar)WindowManager.Instance.CreateWindow(
        "SuaveLook.SuaveTitleBar", name + "_auto_Titlebar");

    window.MetricsMode = MetricsMode.Absolute;
    window.Position = new PointF(TitlebarXOffset, TitlebarYOffset);

    return window;
  }

  /// <summary>
  /// 
  /// </summary>
  public override void LayoutComponentWidgets() {
    // set the size of the titlebar
    SizeF titleSize = new SizeF();
    if(titleBar.Font != null)
      titleSize.Height = titleBar.Font.LineSpacing + TitlebarTextPadding;
    else
      titleSize.Height = 12 + TitlebarTextPadding;

    titleSize.Width = this.IsRolledUp ? absOpenSize.Width : absArea.Width;

    titleBar.Size = titleSize;

    // set the size of the close button
    float closeSize = titleSize.Height * 0.66f;
    closeButton.Size = new SizeF(closeSize, closeSize);

    float closeX = titleSize.Width - closeSize - titleSize.Height * 0.125f;
    float closeY = TitlebarYOffset + ((titleSize.Height - closeSize) / 2);
    closeButton.Position = new PointF(closeX, closeY);
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="z"></param>
  protected override void DrawSelf(float z) {
    // get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    CeGui.Vector3 pos = new CeGui.Vector3(absRect.Left, absRect.Top, z);

    clientBrush.Draw(pos, InnerRect);

    if(FrameEnabled) {
      frame.Draw(pos, PixelRect);
    }
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
  protected override void OnSized(GuiEventArgs e) {
    // MUST call base class handler no matter what.  This is now required 100%
    base.OnSized(e);

    Rect area = UnclippedPixelRect;
    SizeF newSize = new SizeF(area.Width, area.Height);

    // adjust frame and client area rendering objects so that the title bar and close button appear half in and half-out of the frame.
    float frameOffset = 0;

    // if title bar is active, close button is the same height.
    if(TitleBarEnabled) {
      frameOffset = titleBar.UnclippedPixelRect.Height;
    }// if no title bar, measure the close button instead.
  else if(CloseButtonEnabled) {
      frameOffset = closeButton.UnclippedPixelRect.Height;
    }

    // move frame into position
    PointF pos = new PointF(0, frameOffset);
    frame.Position = pos;

    // adjust the size of the frame
    newSize.Height -= frameOffset;
    frame.Size = newSize;

    // adjust position for client brush
    pos.Y += (TitleBarEnabled || FrameEnabled) ? 0 : frameTopSize;

    // modify size of client so it is within the frame
    if(FrameEnabled) {
      pos.X += frameLeftSize;
      newSize.Width -= (frameLeftSize + frameRightSize);
      newSize.Height -= frameBottomSize;

      if(TitleBarEnabled) {
        newSize.Height -= frameTopSize;
      }
    }

    clientBrush.Size = newSize;
    clientBrush.Position = pos;
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
  protected override void OnAlphaChanged(GuiEventArgs e) {
    base.OnAlphaChanged(e);

    frame.Colors.SetAlpha(EffectiveAlpha);
    clientBrush.Colors.SetAlpha(EffectiveAlpha);
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
  protected override void OnActivated(WindowEventArgs e) {
    base.OnActivated(e);
    UpdateFrameColors();
    closeButton.Alpha = 1.0f;
  }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
  protected override void OnDeactivated(WindowEventArgs e) {
    base.OnDeactivated(e);
    UpdateFrameColors();
    closeButton.Alpha = 0.5f;
  }

  /// <summary>Handles the frame for the window</summary>
  protected RenderableFrame frame = new RenderableFrame();
  /// <summary>Handles the client clearing brush for the window</summary>
  protected RenderableImage clientBrush = new RenderableImage();

  /// <summary>Width of the left frame edge in pixels</summary>
  protected float frameLeftSize;
  /// <summary>Width of the right frame edge in pixels</summary>
  protected float frameRightSize;
  /// <summary>Height of the top frame edge in pixels</summary>
  protected float frameTopSize;
  /// <summary>Height of the bottom frame edge in pixels</summary>
  protected float frameBottomSize;

}

} // namespace CeGui.WidgetSets.Suave
