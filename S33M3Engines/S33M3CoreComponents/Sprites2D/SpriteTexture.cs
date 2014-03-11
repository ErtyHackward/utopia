using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D;
using System.Drawing.Imaging;
using System.Drawing;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Rectangle = SharpDX.Rectangle;
using S33M3DXEngine.Effects.HLSLFramework;

namespace S33M3CoreComponents.Sprites2D
{
    public class SpriteTexture : BaseComponent
    {
        private D3DEngine _d3dEngine;
        public ShaderResourceView Texture;
        public ByteColor ColorModifier;

        public int Width;
        public int Height;

        public Rectangle ScreenPosition;

        /// <summary>
        /// Use this Constructor the Force the centering of the spriteTexture to the ViewPort !
        /// </summary>
        /// <param name="device"></param>
        /// <param name="texturePath"></param>
        /// <param name="viewPortUpdtEvent"></param>
        /// <param name="currentViewPort"></param>
        public SpriteTexture(Device device, string texturePath,
                             D3DEngine d3dEngine,
                             ViewportF currentViewPort)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath);
            CreateResource(device, tex,
                           new Vector2I((int)(currentViewPort.Width / 2) - (tex.Description.Width / 2),
                                        (int)(currentViewPort.Height) / 2 - (tex.Description.Height / 2)),
                           tex.Description.Format);

            _d3dEngine = d3dEngine;
            _d3dEngine.ScreenSize_Updated += D3dEngine_ScreenSize_Updated;
            Width = tex.Description.Width;
            Height = tex.Description.Height;

            tex.Dispose();
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3dEngine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            ScreenPosition = new Rectangle((int)(viewport.Width / 2) - (Width / 2), (int)(viewport.Height / 2) - (Height / 2), Width, Height);
        }

        public SpriteTexture(Device device, string texturePath) : this(device, texturePath, Vector2I.Zero)
        {

        }

        public SpriteTexture(Device device, string texturePath, Vector2I screenPosition)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath);
            CreateResource(device, tex, screenPosition, tex.Description.Format);
            tex.Dispose();
        }

        public SpriteTexture(Device device, string texturePath, Vector2I screenPosition, ImageLoadInformation imageLoadParam)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath, imageLoadParam);
            CreateResource(device, tex, screenPosition, tex.Description.Format);
            tex.Dispose();
        }

        public SpriteTexture(Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException("texture");

            CreateResource(texture.Device, texture, Vector2I.Zero, texture.Description.Format);
        }

        public SpriteTexture(Texture2D texture, SharpDX.DXGI.Format customFormat)
        {
            if (texture == null) throw new ArgumentNullException("texture");

            CreateResource(texture.Device, texture, Vector2I.Zero, customFormat);
        }

        public SpriteTexture(Device device, Texture2D texture, Vector2I screenPosition)
        {
            CreateResource(device, texture, screenPosition, texture.Description.Format);
        }

        public SpriteTexture(Texture2D texture, ShaderResourceView textureShader, Vector2 screenPosition)
        {
            Texture = textureShader;
            Width = texture.Description.Width;
            Height = texture.Description.Height;
        }

        public SpriteTexture(int width, int height, ShaderResourceView textureShader, Vector2 screenPosition)
        {
            Texture = textureShader;
            Width = width;
            Height = height;
        }

        public SpriteTexture(Device device, Bitmap image, Vector2 screenPosition, SharpDX.DXGI.Format bitmapFormat)
        {
            Width = image.Width;
            Height = image.Height;

            // Lock the bitmap for direct memory access
            BitmapData bmData = image.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Create a D3D texture, initalized with the bitmap data  
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = Width;
            texDesc.Height = Height;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = bitmapFormat;
            texDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;

            DataRectangle data = new DataRectangle(new DataStream(bmData.Scan0, 4 * Width * Height, true, false).DataPointer, Width * 4);

            Texture2D texture2d = new Texture2D(device, texDesc, data);
            image.UnlockBits(bmData);

            //Texture2D.ToFile<Texture2D>(device.ImmediateContext, texture2d, ImageFileFormat.Bmp, "d:\\test2.bmp");

            ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            srDesc.Format = bitmapFormat;
            //srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            //srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };
            srDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
            srDesc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource() { MostDetailedMip = 0, MipLevels = 1, FirstArraySlice = 0, ArraySize = 1 };

            Texture = ToDispose(new ShaderResourceView(device, texture2d, srDesc));
            texture2d.Dispose();
        }

        public unsafe SpriteTexture(Device device, Bitmap image, Vector2 screenPosition, bool B8G8R8A8_UNormSupport)
        {
            SharpDX.DXGI.Format bitmapFormat;
            B8G8R8A8_UNormSupport = !B8G8R8A8_UNormSupport;
            if (!B8G8R8A8_UNormSupport)
            {
                bitmapFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            }
            else
            {
                bitmapFormat = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            }

            Width = image.Width;
            Height = image.Height;

            // Lock the bitmap for direct memory access
            BitmapData bmData = image.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Swith the Red and Blue channel if needed !
            if (!B8G8R8A8_UNormSupport)
            {
                uint* byteData = (uint*)bmData.Scan0;

                // Switch bgra <-> rgba
                for (int i = 0; i < image.Width * image.Height; i++)
                {
                    byteData[i] = (byteData[i] & 0x000000ff) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }

                byteData = null;
            }

            // Create a D3D texture, initalized with the bitmap data  
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = Width;
            texDesc.Height = Height;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = bitmapFormat;
            texDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;

            DataRectangle data = new DataRectangle(new DataStream(bmData.Scan0, 4 * Width * Height, true, false).DataPointer, Width * 4);

            Texture2D texture2d = new Texture2D(device, texDesc, data);
            image.UnlockBits(bmData);

            //Texture2D.ToFile<Texture2D>(device.ImmediateContext, texture2d, ImageFileFormat.Bmp, "d:\\test2.bmp");

            ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            srDesc.Format = bitmapFormat;
            //srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            //srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };
            srDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
            srDesc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource() { MostDetailedMip = 0, MipLevels = 1, FirstArraySlice = 0, ArraySize = 1 };

            Texture = ToDispose(new ShaderResourceView(device, texture2d, srDesc));
            texture2d.Dispose();
        }

        private void CreateResource(Device device, Texture2D texture, Vector2I screenPosition, SharpDX.DXGI.Format format)
        {

            //By default all textures will need to be single array texture

            ShaderResourceViewDescription viewDesc = new ShaderResourceViewDescription()
            {
                Format = format,
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = texture.Description.MipLevels,
                    FirstArraySlice = 0,
                    ArraySize = 1
                }
            };

            Texture = ToDispose(new ShaderResourceView(device, texture, viewDesc));
            Width = texture.Description.Width;
            Height = texture.Description.Height;

            ScreenPosition = new Rectangle(screenPosition.X, screenPosition.Y, Width, Height);
        }

        public override void BeforeDispose()
        {
            if (_d3dEngine != null) _d3dEngine.ScreenSize_Updated -= D3dEngine_ScreenSize_Updated;
        }
    }
}
