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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using D3D = Microsoft.DirectX.Direct3D;
using DX = Microsoft.DirectX;

namespace CeGui.Demo.DirectX {

  /// <summary>A form used to host the CeGui demo gui in Direct3D</summary>
  /// <remarks>
  ///   This form hosts the CeGui windowing system and forwards relevant events
  ///   (key presses, mouse movements and button clicks) to CeGui. Since CeGui
  ///   displays its own mouse cursor, the window tries to capture the mouse
  ///   and disables the windows cursor.
  /// </remarks>
  public partial class CeGuiDemoForm : Direct3DForm {

    /// <summary>Initializes the CeGui demonstration form</summary>
    public CeGuiDemoForm() {
      InitializeComponent();

      // Hide the windows mouse cursor since CeGui draws its own one
      System.Windows.Forms.Cursor.Hide();
    }

    /// <summary>Executed when the form is loaded for the first time</summary>
    /// <param name="e">Not used</param>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      // Next, we need a renderer. CeGui is not bound to any graphics API and
      // the renderers are the glue that let CeGui interface with a graphics API
      // like DirectX (in this case) or a 3D engine like Axiom.
      this.guiRenderer = new CeGui.Renderers.Direct3D9.D3D9Renderer(d3dDevice, 4096);

      // Initialize the CeGui system. This should be the first method called before
      // using any of the CeGui routines.
      CeGui.GuiSystem.Initialize(this.guiRenderer);

      // All graphics used by any CeGui themes are stored in image files that are mapped
      // by a special XML description which tells CeGui what can be found where on the
      // images. Obviously, we need to load these resources
      //
      // Note that it is possible, and even usual, for these steps to
      // be done automatically via a "scheme" definition, or even from the
      // cegui.conf configuration file, however for completeness, and as an
      // example, virtually everything is being done manually in this example
      // code.
      loadCeGuiResources();
      setupDefaults();

      // Now that the system is initialised, we can actually create some UI elements,
      // for this first example, a full-screen 'root' window is set as the active GUI
      // sheet, and then a simple frame window will be created and attached to it.
      //
      // All windows and widgets are created via the WindowManager singleton.
      WindowManager winMgr = WindowManager.Instance;

      // Here we create a "DefaultWindow". This is a native type, that is, it does not
      // have to be loaded via a scheme, it is always available. One common use for the
      // DefaultWindow is as a generic container for other windows. Its size defaults
      // to 1.0f x 1.0f using the relative metrics mode, which means when it is set as
      // the root GUI sheet window, it will cover the entire display. The DefaultWindow
      // does not perform any rendering of its own, so is invisible.
      //
      // Create a DefaultWindow called 'Root'.
      this.rootGuiSheet = winMgr.CreateWindow("DefaultWindow", "Root") as GuiSheet;

      // Set the GUI root window (also known as the GUI "sheet"), so the gui we set up
      // will be visible.
      GuiSystem.Instance.GuiSheet = rootGuiSheet;

      // Now we will create something to play with. Our "video mode selector" is a small
      // CeGui form that will display a video mode selection dialog as it might
      // typically appear in computer games (if they had such a nice GUI system :))
      //
      // Create the dialog
      //this.videoModeSelector = new VideoModeSelectionForm(
      //  new CeGui.WidgetSets.Suave.SuaveGuiBuilder()
        //new CeGui.WidgetSets.Taharez.TLGuiBuilder()
        //new CeGui.WidgetSets.Windows.WLGuiBuilder()
      //);
      //((CeGui.Window)this.videoModeSelector).SetFont("WindowTitle");

      // Add the dialog as child to the root gui sheet. The root gui sheet is the desktop
      // and we've just added a window to it, so the window will appear on the desktop.
      // Logical, right?
      this.rootGuiSheet.AddChild(
        WindowManager.Instance.LoadWindowLayout("Content/Layouts/MainMenu.layout", this));
    }

    /// <summary>Loads dynamic resources</summary>
    private void loadCeGuiResources() {

      // Widget sets are collections of widgets that provide the widget classes defined
      // in CeGui (like PushButton, CheckBox and so on) with their own distinctive look
      // (like a theme) and possibly even custom behavior.
      //
      // Here we load all compiled widget sets we can find in the current directory. This
      // is done to demonstrate how you could add widget set dynamically to your
      // application. Other possibilities would be to hardcode the widget set an
      // application uses or determining the assemblies to load from a configuration file.
      string[] assemblyFiles = System.IO.Directory.GetFiles(
        System.IO.Directory.GetCurrentDirectory(), "CeGui.WidgetSets.*.dll"
      );
      foreach(string assemblyFile in assemblyFiles) {
        WindowManager.Instance.AttachAssembly(
          System.Reflection.Assembly.LoadFile(assemblyFile)
        );
      }

      // Imagesets area a collection of named areas within a texture or image file. Each
      // area becomes an Image, and has a unique name by which it can be referenced. Note
      // that an Imageset would normally be specified as part of a scheme file, although
      // as this example is demonstrating, it is not a requirement.
      //
      // Again, we load all image sets we can find, this time searching the resources folder.
      string[] imageSetFiles = System.IO.Directory.GetFiles(
        System.IO.Directory.GetCurrentDirectory() + "\\Resources", "*.imageset"
      );
      foreach(string imageSetFile in imageSetFiles)
        ImagesetManager.Instance.CreateImageset(imageSetFile);

    }

    /// <summary>Configures the default cursor and font for CeGui</summary>
    private void setupDefaults() {

      // When the gui imagery side of thigs is set up, we should load in a font.
      // You should always load in at least one font, this is to ensure that there
      // is a default available for any gui element which needs to draw text.
      // The first font you load is automatically set as the initial default font,
      // although you can change the default later on if so desired.  Again, it is
      // possible to list fonts to be automatically loaded as part of a scheme, so
      // this step may not usually be performed explicitly.
      //
      // Fonts are loaded via the FontManager singleton.
      FontManager.Instance.CreateFont("Default", "Arial", 9, FontFlags.None);
      FontManager.Instance.CreateFont("WindowTitle", "Arial", 12, FontFlags.Bold);
      GuiSystem.Instance.SetDefaultFont("Default");

      // The next thing we do is to set a default mouse cursor image.  This is
      // not strictly essential, although it is nice to always have a visible
      // cursor if a window or widget does not explicitly set one of its own.
      //
      // This is a bit hacky since we're assuming the SuaveLook image set, referenced
      // below, will always be available.
      GuiSystem.Instance.SetDefaultMouseCursor(
        ImagesetManager.Instance.GetImageset("SuaveLook").GetImage("Mouse-Arrow")
      );

    }

    /// <summary>Executed to render the Direct3D scene</summary>
    /// <param name="e">Not used</param>
    protected override void OnRender(EventArgs e) {

      // Set up the D3D render states so nothing is culled, no lighting calculations
      // are performed (which would make everything look black because of the absence 
      // of a light source in this example).
      d3dDevice.SetRenderState(D3D.RenderStates.CullMode, (int)D3D.Cull.None);
      d3dDevice.SetRenderState(D3D.RenderStates.Lighting, false);

      // Configure alpha blending. CeGui skins are allowed to use full alpha-blending
      // and without enabling the alpha channel, ugly borders would appear around any shape
      // that makes use of transparency (think of the mouse cursor, for example).
      d3dDevice.SetRenderState(D3D.RenderStates.AlphaBlendEnable, true);
      d3dDevice.SetRenderState(D3D.RenderStates.SourceBlend, (int)D3D.Blend.SourceAlpha);
      d3dDevice.SetRenderState(D3D.RenderStates.DestinationBlend, (int)D3D.Blend.InvSourceAlpha);

      // TODO: CeGui actually uses the z buffer. In the end, this needs to be set to true
      //       and everything has to work properly!
      d3dDevice.SetRenderState(D3D.RenderStates.ZEnable, false);

      CeGui.GuiSystem.Instance.RenderGui();
    }

    /// <summary>Called by the Direct3DForm class when the device needs to be reset</summary>
    protected override void OnResetDevice() {

      // Important: Tell the D3D CeGui renderer that we're about to reset the D3D device.
      // The Reset() call would fail otherwise because the all D3D resources need to be
      // released before calling Reset().
      if(this.guiRenderer != null)
        this.guiRenderer.PreD3DReset();

      // Let the Direct3DForm class perform the reset
      base.OnResetDevice();

      // Just as Important: Tell the D3D CeGui renderer that the reset is done. The renderer
      // will then recreate any resources (textures and vertex buffer) it requires. If you
      // do not call this after resetting the device, bad things will happen ;)
      if(this.guiRenderer != null)
        this.guiRenderer.PostD3DReset();

    }

      /// <summary>
      /// 
      /// </summary>
    public bool IsClosing {
      get {
        return closing;
      }
    }

    /// <summary>Executed when a mouse button is pressed down</summary>
    /// <param name="e">Contains informations about which mouse button was pressed</param>
    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseDown(e);
        GuiSystem.Instance.InjectMouseDown(e.Button);
    }

    /// <summary>Executed when a mouse button is released again</summary>
    /// <param name="e">Contains informations about which mouse button was release</param>
    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseUp(e);
        GuiSystem.Instance.InjectMouseUp(e.Button);
    }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
    [GuiEvent()]
    public void OnExitClicked(object sender, CeGui.GuiEventArgs e) {
      closing = true;
      //this.Close();
    }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
    [GuiEvent()]
    public void OnShowVideoModeSelectionClicked(object sender, CeGui.GuiEventArgs e) {
      // Now we will create something to play with. Our "video mode selector" is a small
      // CeGui form that will display a video mode selection dialog as it might
      // typically appear in computer games (if they had such a nice GUI system :))
      //
      // Create the dialog
      VideoModeSelectionForm videoModeSelector = new VideoModeSelectionForm(
        new CeGui.WidgetSets.Suave.SuaveGuiBuilder()
      //new CeGui.WidgetSets.Taharez.TLGuiBuilder()
      //new CeGui.WidgetSets.Windows.WLGuiBuilder()
      );
      ((CeGui.Window)videoModeSelector).SetFont("WindowTitle");

      // Add the dialog as child to the root gui sheet. The root gui sheet is the desktop
      // and we've just added a window to it, so the window will appear on the desktop.
      // Logical, right?
      this.rootGuiSheet.AddChild(videoModeSelector);
    }

    /// <summary>Executed when the mouse is moved above the dialog</summary>
    /// <param name="e">Contains informations about the mouse movement</param>
    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
      base.OnMouseMove(e);

      // Determine the center of the form
      Point center = this.Location + new Size(this.Size.Width / 2, this.Size.Height / 2);

      // Now find out where the mouse cursor is in relation to the form's center
      Point delta = new Point(
        System.Windows.Forms.Cursor.Position.X - center.X,
        System.Windows.Forms.Cursor.Position.Y - center.Y
      );

      // Reset the mouse pointer to the form's center. Only do this if required, otherwise
      // this might create an endless recursion
      if(System.Windows.Forms.Cursor.Position != center)
        System.Windows.Forms.Cursor.Position = center;

      // Forward the relative movement to CeGui
      GuiSystem.Instance.InjectMouseMove(delta.X, delta.Y);
      GuiSystem.Instance.InjectMouseWheel(e.Delta);
    }

    /// <summary>Executed when a key is pressed down</summary>
    /// <param name="e">Contains informations about which key has been pressed</param>
    protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
      base.OnKeyDown(e);

      if(e.KeyCode.Equals(Keys.Escape)) {
        closing = true;
      }
      else {
        GuiSystem.Instance.InjectKeyDown(e.KeyCode);
      }
    }

    /// <summary>Executed when a key is released again</summary>
    /// <param name="e">Contains informations about which key has been released</param>
    protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) {
      base.OnKeyUp(e);

      GuiSystem.Instance.InjectKeyUp(e.KeyCode);
    }

    /// <summary>Executed when a character has been entered using the keyboard</summary>
    /// <param name="e">Contains informations about which character has been entered</param>
    protected override void OnKeyPress(KeyPressEventArgs e) {
      base.OnKeyPress(e);

      GuiSystem.Instance.InjectChar(e.KeyChar);
    }

    /*
      void later() {

        // The widgets that we will be using for this sample are the TaharezLook widgets,
        // and to enable us to use this 'skin' we must load the xml specification - which
        // within cegui is known as a "looknfeel" file.
        //
        // We load the looknfeel via the WidgetLookManager singleton.
        //WidgetLookManager::getSingleton().parseLookNFeelSpecification("TaharezLook.looknfeel");

        // The final step of basic initialisation that is usually peformed is
        // registering some widgets with the system via a scheme file.  The scheme
        // basically states the name of a dynamically loaded module that contains the
        // widget classes that we wish to use.  As stated previously, a scheme can actually
        // conatin much more information, though for the sake of this first example, we
        // load a scheme which only contains what is required to register some widgets.
        //
        // Use the SchemeManager singleton to load in a scheme that registers widgets
        // for TaharezLook.
        //SchemeManager::getSingleton().loadScheme("TaharezLookWidgets.scheme");

      }
    */

    /// <summary>Renderer that is used to render the CeGui form through the D3D API</summary>
    private CeGui.Renderers.Direct3D9.D3D9Renderer guiRenderer;
    /// <summary>The root GUI sheet (sort of like the windows "desktop" window)</summary>
    private GuiSheet rootGuiSheet;
    /// <summary>CeGui form we are hosting and rendering</summary>
    //private VideoModeSelectionForm videoModeSelector;
    private bool closing = false;
  }

} // namespace CeGui.Demo.DirectX
