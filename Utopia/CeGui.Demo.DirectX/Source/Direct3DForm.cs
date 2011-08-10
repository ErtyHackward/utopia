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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using D3D = Microsoft.DirectX.Direct3D;

/// <summary>A window that uses Direct3D to draw its contents</summary>
/// <remarks>
///   <para>
///     This class can be used as a framework for quick D3D experimentation. It is not
///     intended to replace a full-blown rendering framework as it barely automates the
///     initialization of Direct3D and some maintenance work like handling window resizes.
///   </para>
///   <para>
///     Usage is simple: Just derive your own form from this one. Then override OnRender()
///     (and OnResetDevice() if you've got any unmanaged D3D resources) and you're ready
///     to go. The D3D device can be accessed as this.d3dDevice
///   </para>
/// </remarks>
public partial class Direct3DForm : Form {

  /// <summary>Initializes the window</summary>
  public Direct3DForm() {
    InitializeComponent();
  }

  /// <summary>Gets executed when the form is brought onto the screen</summary>
  /// <param name="e">Not used</param>
  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    // Set up the presentation parameters for our little demonstration
    presentParameters = new D3D.PresentParameters();
    presentParameters.Windowed = true;
    presentParameters.SwapEffect = D3D.SwapEffect.Discard;

    presentParameters.EnableAutoDepthStencil = true;
    presentParameters.AutoDepthStencilFormat = D3D.DepthFormat.D16;

    presentParameters.BackBufferFormat = D3D.Format.R5G6B5;
    presentParameters.BackBufferWidth = this.ClientSize.Width;
    presentParameters.BackBufferHeight = this.ClientSize.Height;

    // Everything is set up, so now we can create the Direct3D device
    d3dDevice = new D3D.Device(
      0,
      D3D.DeviceType.Hardware,
      this.Handle,
      D3D.CreateFlags.SoftwareVertexProcessing,
      presentParameters
    );
  }

  /// <summary>Executed when the window has been closed for good</summary>
  /// <param name="e">Not used</param>
  protected override void OnClosed(EventArgs e) {
    d3dDevice.Dispose();
  }

  /// <summary>We don't want to paint here to avoid flickering </summary>
  /// <param name="e">Additional parameters that control painting</param>
  protected override void OnPaintBackground(PaintEventArgs e) { }

  /// <summary>Paints the window contents</summary>
  /// <param name="e">Additional parameters that control painting</param>
  protected override void OnPaint(PaintEventArgs e) {

    // Prepare the scene for rendering
    d3dDevice.BeginScene();
    d3dDevice.Clear(
      D3D.ClearFlags.Target | D3D.ClearFlags.ZBuffer,
      SystemColors.Control,
      1.0f,
      0
    );

    // Call the user-override (or not) rendering method.
    OnRender(EventArgs.Empty);

    d3dDevice.EndScene();
    d3dDevice.Present();
  }

  /// <summary>Called when the window changes its size</summary>
  /// <param name="e">Not used</param>
  protected override void OnResize(EventArgs e) {
    base.OnResize(e);

    // When the window size changes the device needs to be reset. Otherwise D3D
    // would display the image using a stretch-blit operation making everything
    // look blurred and ugly.
    OnResetDevice();
  }

  /// <summary>Resets the Direct3D device</summary>
  protected virtual void OnResetDevice() {
    if(d3dDevice != null) {
      presentParameters.BackBufferWidth = this.ClientSize.Width;
      presentParameters.BackBufferHeight = this.ClientSize.Height;
      d3dDevice.Reset(presentParameters);
    }
  }

  /// <summary>Executed to render the Direct3D scene</summary>
  /// <param name="e">Not used</param>
  protected virtual void OnRender(EventArgs e) { }

  /// <summary>The Direct3D rendering device</summary>
  protected D3D.Device d3dDevice;

  /// <summary>Presentation settings for the active Direct3D device</summary>
  protected D3D.PresentParameters presentParameters;

}
