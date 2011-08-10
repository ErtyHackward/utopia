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
using System.Collections;
using System.Text;

using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace CeGui.Renderers.Direct3D9 {

  /// <summary>A texture in managed DirectX 2.0 on DirectX 9.0c</summary>
  public class D3D9Texture : CeGui.Texture, IDisposable {

    /// <summary>Initializes the texture</summary>
    /// <param name="owner">Renderer who created this texture.</param>
    public D3D9Texture(Renderer owner)
      : base(owner) {

      // Grab a ref to the device
      device = ((D3D9Renderer)owner).D3DDevice;

    }

    /// <summary>
    ///   Direct3D support method that must be called prior to a Reset() call
    ///   on the Direct3D device
    /// </summary>
    public void PreD3DReset() {

      // Textures not based on files are in the managed pool, so we do
      // not worry about those.
      if(this.filename != string.Empty)
        Dispose();

    }

    /// <summary>
    ///   Direct3D support method that must be called after a Reset() call on the
    ///   Direct3D device
    /// </summary>
    public void PostD3DReset() {

      // Textures not based on files are in the managed pool, so we do
      // not worry about those.
      if(this.filename != string.Empty)
        LoadFromFile(this.filename);

    }

    /// <summary>Loads a texture from the specified file</summary>
    /// <param name="fileName">Name of the image file to load</param>
    public override void LoadFromFile(string fileName) {
      Dispose();

      this.texture = D3D.TextureLoader.FromFile(device, fileName);
      this.filename = fileName;

      // grab the inferred dimensions of the texture
      D3D.SurfaceDescription desc = texture.GetLevelDescription(0);
      this.width = desc.Width;
      this.height = desc.Height;
    }

    /// <summary>Loads a texture file from a stream (could be in memory)</summary>
    /// <param name="buffer">Stream holding the image data to load into this texture.</param>
    /// <param name="bufferWidth">Width of the image data (in pixels).</param>
    /// <param name="bufferHeight">Height of the image data (in pixels).</param>
    public override void LoadFromMemory(
      System.IO.Stream buffer, int bufferWidth, int bufferHeight
    ) {
      Dispose();

      this.texture = D3D.TextureLoader.FromStream(
        device, buffer, bufferWidth, bufferHeight,
        1, D3D.Usage.None, D3D.Format.A8R8G8B8, D3D.Pool.Managed,
        D3D.Filter.Point, D3D.Filter.Point, 0
      );
      this.filename = string.Empty;

      // grab the inferred dimensions of the texture
      D3D.SurfaceDescription desc = texture.GetLevelDescription(0);
      this.width = desc.Width;
      this.height = desc.Height;
    }

    /// <summary>Explicitely releases all resources belonging to the instance</summary>
    public void Dispose() {
      if(texture != null) {
        texture.Dispose();
        texture = null;
      }
    }

    /// <summary>The Direct3D texture object</summary>
    public D3D.Texture D3DTexture {
      get { return texture; }
    }

    /// <summary>Reference to our underlying D3D texture</summary>
    protected D3D.Texture texture;
    /// <summary>The device in use by the Renderer who created this texture</summary>
    protected D3D.Device device;
    /// <summary>Filename of the image the texture was loaded from, if any</summary>
    protected string filename;

  }

} // namespace CeGui.Renderers.Direct3D9
