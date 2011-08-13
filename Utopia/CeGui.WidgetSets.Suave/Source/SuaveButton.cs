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

/// <summary>A button using the 'suave' look</summary>
[ AlternateWidgetName("SuaveLook.SuaveButton") ]
public class SuaveButton : PushButton {

  /// <summary>Color of the button when it is in its normal state</summary>
  protected static Colour NormalImageColor = new Colour(0xFFFFFFFF);
  /// <summary>Color of the text on the button when it is in its normal state</summary>
  protected static Colour DefaultNormalTextColor = new Colour(0xFF000000);
  /// <summary>Color of the button when it is in its normal state</summary>
  protected static Colour DisabledImageColor = new Colour(0xFFFFFFFF);
  /// <summary>Color of the text on the button when it is in its normal state</summary>
  protected static Colour DefaultDisabledTextColor = new Colour(0xFFFCFCFC);

  /// <summary>Name of the imageset to use for rendering</summary>
  protected const string ImagesetName = "SuaveLook";

  /// <summary>The image for the top left corner of the button</summary>
  protected const string NormalTopLeftImageName = "Button-Normal-TopLeft";  
  /// <summary>The image for the left side of the button</summary>
  protected const string NormalLeftImageName = "Button-Normal-Left";  
  /// <summary>The image for the bottom left corner of the button</summary>
  protected const string NormalBottomLeftImageName = "Button-Normal-BottomLeft";  
  /// <summary>The image for the top side of the button</summary>
  protected const string NormalTopImageName = "Button-Normal-Top";  
  /// <summary>The image for the center area of the button</summary>
  protected const string NormalCenterImageName = "Button-Normal-Center";  
  /// <summary>The image for the bottom line of the button</summary>
  protected const string NormalBottomImageName = "Button-Normal-Bottom";  
  /// <summary>The image for the top right corner of the button</summary>
  protected const string NormalTopRightImageName = "Button-Normal-TopRight";  
  /// <summary>The image for the right side of the button</summary>
  protected const string NormalRightImageName = "Button-Normal-Right";  
  /// <summary>The image for the bottom right corner of the button</summary>
  protected const string NormalBottomRightImageName = "Button-Normal-BottomRight";  
  
  /// <summary>The image for the top left corner of the button</summary>
  protected const string HoverTopLeftImageName = "Button-Hover-TopLeft";  
  /// <summary>The image for the left side of the button</summary>
  protected const string HoverLeftImageName = "Button-Hover-Left";  
  /// <summary>The image for the bottom left corner of the button</summary>
  protected const string HoverBottomLeftImageName = "Button-Hover-BottomLeft";  
  /// <summary>The image for the top side of the button</summary>
  protected const string HoverTopImageName = "Button-Hover-Top";  
  /// <summary>The image for the center area of the button</summary>
  protected const string HoverCenterImageName = "Button-Hover-Center";  
  /// <summary>The image for the bottom line of the button</summary>
  protected const string HoverBottomImageName = "Button-Hover-Bottom";  
  /// <summary>The image for the top right corner of the button</summary>
  protected const string HoverTopRightImageName = "Button-Hover-TopRight";  
  /// <summary>The image for the right side of the button</summary>
  protected const string HoverRightImageName = "Button-Hover-Right";  
  /// <summary>The image for the bottom right corner of the button</summary>
  protected const string HoverBottomRightImageName = "Button-Hover-BottomRight";  
  
  /// <summary>The image for the top left corner of the button</summary>
  protected const string PushedTopLeftImageName = "Button-Pushed-TopLeft";  
  /// <summary>The image for the left side of the button</summary>
  protected const string PushedLeftImageName = "Button-Pushed-Left";  
  /// <summary>The image for the bottom left corner of the button</summary>
  protected const string PushedBottomLeftImageName = "Button-Pushed-BottomLeft";  
  /// <summary>The image for the top side of the button</summary>
  protected const string PushedTopImageName = "Button-Pushed-Top";  
  /// <summary>The image for the center area of the button</summary>
  protected const string PushedCenterImageName = "Button-Pushed-Center";  
  /// <summary>The image for the bottom line of the button</summary>
  protected const string PushedBottomImageName = "Button-Pushed-Bottom";  
  /// <summary>The image for the top right corner of the button</summary>
  protected const string PushedTopRightImageName = "Button-Pushed-TopRight";  
  /// <summary>The image for the right side of the button</summary>
  protected const string PushedRightImageName = "Button-Pushed-Right";  
  /// <summary>The image for the bottom right corner of the button</summary>
  protected const string PushedBottomRightImageName = "Button-Pushed-BottomRight";  
    /// <summary>
    /// 
    /// </summary>
  protected const int CustomImageLayer = 1;
    /// <summary>
    /// 
    /// </summary>
  protected const int LabelLayer = 2;

  /// <summary>Initializes a new 'suave' look button instance</summary>
  /// <param name="type">TODO: No idea what it's used for</param>
  /// <param name="name">The name of the new button</param>
  public SuaveButton(string type, string name) : base(type, name) {

    autoscaleImages = true;
    useStandardImagery = true;
    useNormalImage = false;
    useHoverImage = false;
    usePushedImage = false;
    useDisabledImage = false;

    StoreFrameSizes();

    // setup images and frames
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    // Set up the normal look of the button
    normalFrame.SetImages(
      imageSet.GetImage(NormalTopLeftImageName),
      imageSet.GetImage(NormalTopRightImageName),
      imageSet.GetImage(NormalBottomLeftImageName),
      imageSet.GetImage(NormalBottomRightImageName),
      imageSet.GetImage(NormalLeftImageName),
      imageSet.GetImage(NormalTopImageName),
      imageSet.GetImage(NormalRightImageName),
      imageSet.GetImage(NormalBottomImageName)
    );
    normalBackground = imageSet.GetImage(NormalCenterImageName);

    // Set up the mouse-over look of the button
    this.hoverFrame.SetImages(
      imageSet.GetImage(HoverTopLeftImageName),
      imageSet.GetImage(HoverTopRightImageName),
      imageSet.GetImage(HoverBottomLeftImageName),
      imageSet.GetImage(HoverBottomRightImageName),
      imageSet.GetImage(HoverLeftImageName),
      imageSet.GetImage(HoverTopImageName),
      imageSet.GetImage(HoverRightImageName),
      imageSet.GetImage(HoverBottomImageName)
    );
    this.hoverBackground = imageSet.GetImage(HoverCenterImageName);

    // Set up the pushed look of the button
    pushedFrame.SetImages(
      imageSet.GetImage(PushedTopLeftImageName),
      imageSet.GetImage(PushedTopRightImageName),
      imageSet.GetImage(PushedBottomLeftImageName),
      imageSet.GetImage(PushedBottomRightImageName),
      imageSet.GetImage(PushedLeftImageName),
      imageSet.GetImage(PushedTopImageName),
      imageSet.GetImage(PushedRightImageName),
      imageSet.GetImage(PushedBottomImageName)
    );
    this.pushedBackground = imageSet.GetImage(PushedCenterImageName);

    // set the default colors for text

    NormalTextColor = DefaultNormalTextColor;
    HoverTextColor = DefaultNormalTextColor;
    PushedTextColor = DefaultNormalTextColor;
    DisabledTextColor = DefaultDisabledTextColor; 
  }
    /// <summary>
    /// 
    /// </summary>
  protected void StoreFrameSizes() {
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    frameLeftSize = imageSet.GetImage(NormalLeftImageName).Width;
    frameRightSize = imageSet.GetImage(NormalRightImageName).Width;
    frameTopSize = imageSet.GetImage(NormalTopImageName).Height;
    frameBottomSize = imageSet.GetImage(NormalBottomImageName).Height;
  }

  #region Base Members

  #region Properties

  /// <summary>
  ///		Get/Set whether of not custom button image areas are auto-scaled to the size of the button.
  /// </summary>
  /// <value>
  ///		true if client specified custom image areas are re-sized when the button size changes.  
  ///		false if image areas will remain unchanged when the button is sized.
  /// </value>
  public bool CustomImageryAutoSized {
    get {
      return autoscaleImages;
    }
    set {
      // if we are enabling auto-sizing, scale images for current size
      if((value == true) && (value != autoscaleImages)) {
        Rect area = new Rect(0, 0, absArea.Width, absArea.Height);

        normalImage.Rect = area;
        hoverImage.Rect = area;
        pushedImage.Rect = area;
        disabledImage.Rect = area;

        RequestRedraw();
      }

      autoscaleImages = value;
    }
  }

  /// <summary>
  ///		Get/Set whether or not rendering of the standard imagery is enabled.
  /// </summary>
  /// <value>true if the standard button imagery will be rendered, false if no standard rendering will be performed.</value>
  public bool StandardImageryEnabled {
    get {
      return useStandardImagery;
    }
    set {
      if(useStandardImagery != value) {
        useStandardImagery = value;
        RequestRedraw();
      }
    }
  }

  #endregion

  #region Methods

  /// <summary>
  ///		Set the details of the image to render for the button in the normal state.
  /// </summary>
  /// <param name="image">
  ///		RenderableImage object with all the details for the image.  Note that an internal copy of the Renderable image is made and
  ///		ownership of <paramref name="image"/> remains with client code.  If this parameter is NULL, rendering of an image for this 
  ///		button state is disabled.
  /// </param>
  public void SetNormalImage(RenderableImage image) {
    if(image == null) {
      useNormalImage = false;
    } else {
      useNormalImage = true;
      normalImage = image;
    }
  }

  /// <summary>
  ///		Set the details of the image to render for the button in the hover state.
  /// </summary>
  /// <param name="image">
  ///		RenderableImage object with all the details for the image.  Note that an internal copy of the Renderable image is made and
  ///		ownership of <paramref name="image"/> remains with client code.  If this parameter is NULL, rendering of an image for this 
  ///		button state is disabled.
  /// </param>
  public void SetHoverImage(RenderableImage image) {
    if(image == null) {
      useHoverImage = false;
    } else {
      useHoverImage = true;
      hoverImage = image;
    }
  }

  /// <summary>
  ///		Set the details of the image to render for the button in the pushed state.
  /// </summary>
  /// <param name="image">
  ///		RenderableImage object with all the details for the image.  Note that an internal copy of the Renderable image is made and
  ///		ownership of <paramref name="image"/> remains with client code.  If this parameter is NULL, rendering of an image for this 
  ///		button state is disabled.
  /// </param>
  public void SetPushedImage(RenderableImage image) {
    if(image == null) {
      usePushedImage = false;
    } else {
      usePushedImage = true;
      pushedImage = image;
    }
  }

  /// <summary>
  ///		Set the details of the image to render for the button in the disabled state.
  /// </summary>
  /// <param name="image">
  ///		RenderableImage object with all the details for the image.  Note that an internal copy of the Renderable image is made and
  ///		ownership of <paramref name="image"/> remains with client code.  If this parameter is NULL, rendering of an image for this 
  ///		button state is disabled.
  /// </param>
  public void SetDisabledImage(RenderableImage image) {
    if(image == null) {
      useDisabledImage = false;
    } else {
      useDisabledImage = true;
      disabledImage = image;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawNormal(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate the colors to use
    ColourRect colors = new ColourRect(
      NormalImageColor, NormalImageColor, NormalImageColor, NormalImageColor
    );
    colors.SetAlpha(EffectiveAlpha);

    // render standard button imagery if required.
    if(useStandardImagery) {

      // draw background image
      Rect bkRect = absRect;
      bkRect.Left += frameLeftSize;
      bkRect.Right -= frameRightSize;
      bkRect.Top += frameTopSize;
      bkRect.Bottom -= frameBottomSize;
      this.normalBackground.Draw(bkRect, z, clipper, colors);

      // draw frame
      normalFrame.Draw(new Vector3(absRect.Left, absRect.Top, z), clipper);
    }

    // render clients custom image if that is required.
    if(useNormalImage) {
      normalImage.Colors.SetAlpha(EffectiveAlpha);
      Vector3 imgPos = new Vector3(absRect.Left, absRect.Top, GuiSystem.Instance.Renderer.GetZLayer(CustomImageLayer));
      normalImage.Draw(imgPos, clipper);
    }

    // draw label text
    if(Font != null) {
      absRect.Top += (absRect.Height - Font.LineSpacing) / 2;
      colors = new ColourRect(base.NormalTextColor);
      colors.SetAlpha(EffectiveAlpha);
      Font.DrawText(Text, absRect, GuiSystem.Instance.Renderer.GetZLayer(LabelLayer), clipper, HorizontalTextFormat.Center, colors);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawHover(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate the colors to use
    ColourRect colors = new ColourRect(
      NormalImageColor, NormalImageColor, NormalImageColor, NormalImageColor
    );
    colors.SetAlpha(EffectiveAlpha);

    // render standard button imagery if required.
    if(useStandardImagery) {
      // draw background image
      Rect bkRect = absRect;
      bkRect.Left += frameLeftSize;
      bkRect.Right -= frameRightSize;
      bkRect.Top += frameTopSize;
      bkRect.Bottom -= frameBottomSize;
      this.hoverBackground.Draw(bkRect, z, clipper, colors);

      // draw frame
      hoverFrame.Draw(new Vector3(absRect.Left, absRect.Top, z), clipper);
    }

    // render clients custom image if that is required.
    if(useHoverImage) {
      hoverImage.Colors.SetAlpha(EffectiveAlpha);
      Vector3 imgPos = new Vector3(absRect.Left, absRect.Top, GuiSystem.Instance.Renderer.GetZLayer(CustomImageLayer));
      hoverImage.Draw(imgPos, clipper);
    }

    // draw label text
    if(Font != null) {
      absRect.Top += (absRect.Height - Font.LineSpacing) / 2;
      colors = new ColourRect(hoverColor);
      colors.SetAlpha(EffectiveAlpha);
      Font.DrawText(Text, absRect, GuiSystem.Instance.Renderer.GetZLayer(LabelLayer), clipper, HorizontalTextFormat.Center, colors);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawPushed(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate the colors to use
    ColourRect colors = new ColourRect(
      NormalImageColor, NormalImageColor, NormalImageColor, NormalImageColor
    );
    colors.SetAlpha(EffectiveAlpha);

    // render standard button imagery if required.
    if(useStandardImagery) {
      // draw background image
      Rect bkRect = absRect;
      bkRect.Left += frameLeftSize;
      bkRect.Right -= frameRightSize;
      bkRect.Top += frameTopSize;
      bkRect.Bottom -= frameBottomSize;
      this.pushedBackground.Draw(bkRect, z, clipper, colors);

      // draw frame
      pushedFrame.Draw(new Vector3(absRect.Left, absRect.Top, z), clipper);
    }

    // render clients custom image if that is required.
    if(usePushedImage) {
      pushedImage.Colors.SetAlpha(EffectiveAlpha);
      Vector3 imgPos = new Vector3(absRect.Left, absRect.Top, GuiSystem.Instance.Renderer.GetZLayer(CustomImageLayer));
      pushedImage.Draw(imgPos, clipper);
    }

    // draw label text
    if(Font != null) {
      absRect.Top += (absRect.Height - Font.LineSpacing) / 2;
      colors = new ColourRect(pushedColor);
      colors.SetAlpha(EffectiveAlpha);
      Font.DrawText(Text, absRect, GuiSystem.Instance.Renderer.GetZLayer(LabelLayer), clipper, HorizontalTextFormat.Center, colors);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawDisabled(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate the colors to use
    ColourRect colors = new ColourRect(
      NormalImageColor, NormalImageColor, NormalImageColor, NormalImageColor
    );
    colors.SetAlpha(EffectiveAlpha);

    // render standard button imagery if required.
    if(useStandardImagery) {
      // draw background image
      Rect bkRect = absRect;
      bkRect.Left += frameLeftSize;
      bkRect.Right -= frameRightSize;
      bkRect.Top += frameTopSize;
      bkRect.Bottom -= frameBottomSize;
      background.Draw(bkRect, z, clipper, colors);

      // draw frame
      normalFrame.Draw(new Vector3(absRect.Left, absRect.Top, z), clipper);
    }

    // render clients custom image if that is required.
    if(useDisabledImage) {
      disabledImage.Colors.SetAlpha(EffectiveAlpha);
      Vector3 imgPos = new Vector3(absRect.Left, absRect.Top, GuiSystem.Instance.Renderer.GetZLayer(CustomImageLayer));
      disabledImage.Draw(imgPos, clipper);
    }

    // draw label text
    if(Font != null) {
      absRect.Top += (absRect.Height - Font.LineSpacing) / 2;
      colors = new ColourRect(disabledColor);
      colors.SetAlpha(EffectiveAlpha);
      Font.DrawText(Text, absRect, GuiSystem.Instance.Renderer.GetZLayer(LabelLayer), clipper, HorizontalTextFormat.Center, colors);
    }
  }

  #endregion

  #endregion

  #region Window Members

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnSized(GuiEventArgs e) {
    // default processing
    base.OnSized(e);

    SizeF absSize = AbsoluteSize;

    // update frame size
    normalFrame.Size = absSize;
    hoverFrame.Size = absSize;
    pushedFrame.Size = absSize;

    // scale user images if required
    if(autoscaleImages) {
      Rect area = new Rect(0, 0, absSize.Width, absSize.Height);

      normalImage.Rect = area;
      hoverImage.Rect = area;
      pushedImage.Rect = area;
      disabledImage.Rect = area;
    }

    e.Handled = true;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnAlphaChanged(GuiEventArgs e) {
    base.OnAlphaChanged(e);

    normalFrame.Colors.SetAlpha(EffectiveAlpha);
    hoverFrame.Colors.SetAlpha(EffectiveAlpha);
    pushedFrame.Colors.SetAlpha(EffectiveAlpha);
  }

  #endregion

  /// <summary>
  /// 
  /// </summary>
  protected bool autoscaleImages;
  /// <summary>
  /// 
  /// </summary>
  protected bool useStandardImagery;
  /// <summary>
  /// 
  /// </summary>
  protected bool useNormalImage;
  /// <summary>
  /// 
  /// </summary>
  protected bool useHoverImage;
  /// <summary>
  /// 
  /// </summary>
  protected bool usePushedImage;
  /// <summary>
  /// 
  /// </summary>
  protected bool useDisabledImage;

  /// <summary>
  /// 
  /// </summary>
  protected RenderableImage normalImage = new RenderableImage();
  /// <summary>
  /// 
  /// </summary>
  protected RenderableImage hoverImage = new RenderableImage();
  /// <summary>
  /// 
  /// </summary>
  protected RenderableImage pushedImage = new RenderableImage();
  /// <summary>
  /// 
  /// </summary>
  protected RenderableImage disabledImage = new RenderableImage();

  /// <summary>
  /// 
  /// </summary>
  protected RenderableFrame normalFrame = new RenderableFrame();
  /// <summary>
  /// 
  /// </summary>
  protected RenderableFrame hoverFrame = new RenderableFrame();
  /// <summary>
  /// 
  /// </summary>
  protected RenderableFrame pushedFrame = new RenderableFrame();

  /// <summary>
  /// 
  /// </summary>
  protected float frameLeftSize;
  /// <summary>
  /// 
  /// </summary>
  protected float frameTopSize;
  /// <summary>
  /// 
  /// </summary>
  protected float frameRightSize;
  /// <summary>
  /// 
  /// </summary>
  protected float frameBottomSize;

  /// <summary>
  /// 
  /// </summary>
  protected Image background;


  /// <summary>Image used to fill the center area of the button in normal mode</summary>
  protected Image normalBackground;
  /// <summary>Image used to fill the center area of the button in mouse-over mode</summary>
  protected Image hoverBackground;
  /// <summary>Image used to fill the center area of the button when it is pushed</summary>
  protected Image pushedBackground;

}

} // namespace CeGui.WidgetSets.Windows
