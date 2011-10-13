#region

using System;
using S33M3Engines.Shared.Delegates;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace S33M3Engines.Shared.Sprites
{
    public class SpriteTexture : IDisposable
    {
        private D3DEngineDelegates.ViewPortUpdated _viewPortUpdtEvent;

        public ShaderResourceView Texture;
        public bool _textureDispose = true;

        public int Width;
        public int Height;

        public Matrix ScreenPosition;

        /// <summary>
        /// otional index of texture in texture array
        /// </summary>
        public int Index; 

        /// <summary>
        /// Use this Constructor the Force the centering of the spriteTexture to the ViewPort !
        /// </summary>
        /// <param name="device"></param>
        /// <param name="texturePath"></param>
        /// <param name="viewPortUpdtEvent"></param>
        /// <param name="currentViewPort"></param>
        public SpriteTexture(Device device, string texturePath, ref D3DEngineDelegates.ViewPortUpdated viewPortUpdtEvent,
                             Viewport currentViewPort)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath);
            CreateResource(device, tex,
                           new Vector2((currentViewPort.Width/2) - (tex.Description.Width/2),
                                       (currentViewPort.Height)/2 - (tex.Description.Height/2)));

            _viewPortUpdtEvent = viewPortUpdtEvent;
            viewPortUpdtEvent += D3dEngine_ViewPort_Updated;
            Width = tex.Description.Width;
            Height = tex.Description.Height;

            tex.Dispose();
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            ScreenPosition.M41 = (viewport.Width/2) - (Width/2);
            ScreenPosition.M42 = (viewport.Height/2) - (Height/2);
        }

        public SpriteTexture(Device device, string texturePath, Vector2 screenPosition)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath);
            CreateResource(device, tex, screenPosition);
            tex.Dispose();
        }

        public SpriteTexture(Device device, Texture2D texture, Vector2 screenPosition)
        {
            CreateResource(device, texture, screenPosition);
        }

        public SpriteTexture(Texture2D texture, ShaderResourceView textureShader, Vector2 screenPosition)
        {
            _textureDispose = false;
            Texture = textureShader;
            Width = texture.Description.Width;
            Height = texture.Description.Height;
            ScreenPosition = Matrix.Translation(screenPosition.X, screenPosition.Y, 0);
        }

        public SpriteTexture(int width, int height, ShaderResourceView textureShader, Vector2 screenPosition)
        {
            _textureDispose = false;
            Texture = textureShader;
            Width = width;
            Height = height;
            ScreenPosition = Matrix.Translation(screenPosition.X, screenPosition.Y, 0);
        }

        public SpriteTexture(Device device, Bitmap image, Vector2 screenPosition, SharpDX.DXGI.Format bitmapFormat)
        {
            _textureDispose = true;

            image.Save("d:\\test.bmp");

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

            Texture = new ShaderResourceView(device, texture2d, srDesc);
            texture2d.Dispose();

            ScreenPosition = Matrix.Translation(screenPosition.X, screenPosition.Y, 0);
        }

        private void CreateResource(Device device, Texture2D texture, Vector2 screenPosition)
        {
            ScreenPosition = Matrix.Translation(screenPosition.X, screenPosition.Y, 0);
            //By default all textures will need to be single array texture

            ShaderResourceViewDescription viewDesc = new ShaderResourceViewDescription()
            {
                Format = texture.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = texture.Description.MipLevels,
                    FirstArraySlice = 0,
                    ArraySize = 1
                }
            };

            Texture = new ShaderResourceView(device, texture, viewDesc);
            Width = texture.Description.Width;
            Height = texture.Description.Height;
        }

        public void Dispose()
        {
            if (_textureDispose) Texture.Dispose();
            if(_viewPortUpdtEvent != null) _viewPortUpdtEvent -= D3dEngine_ViewPort_Updated;
        }
    }
}