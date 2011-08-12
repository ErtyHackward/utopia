using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CeGui;
using S33M3Engines.Sprites;
using SharpDX.Direct3D11;
using S33M3Engines.D3D;
using SharpDX.DXGI;
using SharpDX;
using System.Drawing.Imaging;
using System.Drawing;
using Rectangle = System.Drawing.Rectangle;
using SharpDX.Direct3D;

namespace Utopia.GUI.cegui
{
    public class SpriteGuiTexture : CeGui.Texture, IDisposable
    {

        protected SpriteTexture _texture;
        protected string _filename;

        protected Game _game;

        public SpriteTexture SpriteTexture { get { return _texture; } }


        public SpriteGuiTexture(Renderer owner)
            : base(owner)
        {
            _game = ((SpriteGuiRendererOptimized)owner).Game;
        }

        public override void LoadFromFile(string fileName)
        {
            Dispose();

            _texture = new SpriteTexture(((SpriteGuiRendererOptimized)owner).Game, fileName);
            _filename = fileName;

            // grab the inferred dimensions of the texture
            this.width = _texture.TextureDescr.Width;
            this.height = _texture.TextureDescr.Height;
        }

        public override void LoadFromMemory(System.IO.Stream buffer, int bufferWidth, int bufferHeight)
        {
            ////TEST 1 Methods similar to the one I use with my font generator => Not working still no alpha in the used texture ...!

            //// Lock the bitmap for direct memory access
            //Bitmap textBitmap = new Bitmap(buffer);

            //BitmapData bmData = textBitmap.LockBits(new Rectangle(0, 0, bufferWidth, bufferHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //// Create a D3D texture, initalized with the bitmap data  
            //Texture2DDescription texDesc = new Texture2DDescription();
            //texDesc.Width = bufferWidth;
            //texDesc.Height = bufferHeight;
            //texDesc.MipLevels = 1;
            //texDesc.ArraySize = 1;
            //texDesc.Format = Format.B8G8R8A8_UNorm;
            //texDesc.SampleDescription = new SampleDescription(1, 0);
            //texDesc.Usage = ResourceUsage.Immutable;
            //texDesc.BindFlags = BindFlags.ShaderResource;
            //texDesc.CpuAccessFlags = CpuAccessFlags.None;
            //texDesc.OptionFlags = ResourceOptionFlags.None;

            //DataRectangle data = new DataRectangle(bufferWidth * 4, new DataStream(bmData.Scan0, 4 * bufferWidth * bufferHeight, true, false));

            //Texture2D texture = new Texture2D(_game.GraphicDevice, texDesc, data);

            //textBitmap.UnlockBits(bmData);

            //ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            //srDesc.Format = Format.B8G8R8A8_UNorm;
            //srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            //srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };

            //ShaderResourceView _srView = new ShaderResourceView(_game.GraphicDevice, texture, srDesc);

            //_texture = new SpriteTexture(texture, _srView, SharpDX.Vector2.Zero);
            //this.width = bufferWidth;
            //this.height = bufferHeight;

            //===============================================================================



            //TEST 2 ==> Not Working, even if the generated file contains Alpha !
            //Bitmap textBitmap = new Bitmap(buffer);
            //textBitmap.Save(@"e:\test.bmp");
            //Texture2D d3dTexture = Texture2D.FromFile<Texture2D>(_game.GraphicDevice, @"e:\test.bmp");

            Texture2D d3dTexture = Texture2D.FromStream<Texture2D>(_game.GraphicDevice, buffer, (int)buffer.Length);

            /* directx9 :
                        this.texture = D3D.TextureLoader.FromStream(
                 device, buffer, bufferWidth, bufferHeight,
                 1, D3D.Usage.None, D3D.Format.A8R8G8B8, D3D.Pool.Managed,
                 D3D.Filter.Point, D3D.Filter.Point, 0
               );*/
            _filename = string.Empty;


            // grab the inferred dimensions of the texture
            Texture2DDescription desc = d3dTexture.Description;
            this.width = desc.Width;
            this.height = desc.Height;

            _texture = new SpriteTexture(_game.GraphicDevice, d3dTexture, SharpDX.Vector2.Zero);
        }

        public void Dispose()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }
    }
}
