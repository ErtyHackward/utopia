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

namespace S33M3Engines.Sprites.GUI
{
    /// <summary>
    /// CeBui class for wrapping a SpriteTexture
    /// </summary>
    public class SpriteGuiTexture : CeGui.Texture, IDisposable
    {
        #region Private variable
        protected SpriteTexture _texture;
        protected string _filename;
        protected Game _game;
        #endregion

        #region Public properties/variable
        public SpriteTexture SpriteTexture { get { return _texture; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Texture's owner</param>
        public SpriteGuiTexture(Renderer owner)
            : base(owner)
        {
            _game = ((SpriteGuiRenderer)owner).Game;
        }

        #region Public methods
        /// <summary>
        /// Create the texture from a file
        /// </summary>
        /// <param name="fileName">Path to the file</param>
        public override void LoadFromFile(string fileName)
        {
            Dispose();

            _texture = new SpriteTexture(((SpriteGuiRenderer)owner).Game, fileName);
            _filename = fileName;

            // grab the inferred dimensions of the texture
            this.width = _texture.TextureDescr.Width;
            this.height = _texture.TextureDescr.Height;
        }

        /// <summary>
        /// Create the file from a Bitmap
        /// </summary>
        /// <param name="bitmap">The Bitmap object</param>
        /// <param name="bufferWidth">Texture Width</param>
        /// <param name="bufferHeight">Texture Height</param>
        public override void LoadFromBitMap(Bitmap bitmap, int bufferWidth, int bufferHeight)
        {
            // Lock the bitmap for direct memory access
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bufferWidth, bufferHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Create a D3D texture, initalized with the bitmap data  
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = bufferWidth;
            texDesc.Height = bufferHeight;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = Format.B8G8R8A8_UNorm;
            texDesc.SampleDescription = new SampleDescription(1, 0);
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;

            DataRectangle data = new DataRectangle(bufferWidth * 4, new DataStream(bmData.Scan0, 4 * bufferWidth * bufferHeight, true, false));

            Texture2D texture = new Texture2D(_game.GraphicDevice, texDesc, data);

            bitmap.UnlockBits(bmData);

            //Create the view on the Texture buffer
            ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            srDesc.Format = Format.B8G8R8A8_UNorm;
            srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };

            ShaderResourceView _srView = new ShaderResourceView(_game.GraphicDevice, texture, srDesc);

            //Assigne the created SpriteTexture to the internal variable
            _texture = new SpriteTexture(texture, _srView, new SharpDX.Vector2(0, 0));

            texture.Dispose();

            this.width = bufferWidth;
            this.height = bufferHeight;
        }

        /// <summary>
        /// Create the texture from a Stream object
        /// !!!! If bitmap the Alpha value will be lost !!!!
        /// </summary>
        /// <param name="buffer">The texture under stream format</param>
        /// <param name="bufferWidth">Texture Width</param>
        /// <param name="bufferHeight">Texture Height</param>
        public override void LoadFromMemory(System.IO.Stream buffer, int bufferWidth, int bufferHeight)
        {

            Texture2D d3dTexture = Texture2D.FromStream<Texture2D>(_game.GraphicDevice, buffer, (int)buffer.Length);

            this.width = bufferWidth;
            this.height = bufferHeight;
            _filename = string.Empty;

            _texture = new SpriteTexture(_game.GraphicDevice, d3dTexture, SharpDX.Vector2.Zero);
        }

        /// <summary>
        /// Dispose binded DX resources
        /// </summary>
        public void Dispose()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }
        #endregion
    }
}
