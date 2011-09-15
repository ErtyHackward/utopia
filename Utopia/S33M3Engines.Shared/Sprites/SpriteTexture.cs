﻿#region

using System;
using S33M3Engines.Shared.Delegates;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

#endregion

namespace S33M3Engines.Shared.Sprites
{
    public class SpriteTexture : IDisposable
    {
        public ShaderResourceView Texture;
        public bool _textureDispose = true;

        public int Width;
        public int Height;

        public Matrix ScreenPosition;

        /// <summary>
        /// otional index of texture in texture array
        /// </summary>
        public int Index; 

        public SpriteTexture(Device device, string texturePath, ref D3DEngineDelegates.ViewPortUpdated viewPortUpdtEvent,
                             Viewport currentViewPort)
        {
            Texture2D tex = Resource.FromFile<Texture2D>(device, texturePath);
            CreateResource(device, tex,
                           new Vector2((currentViewPort.Width/2) - (tex.Description.Width/2),
                                       (currentViewPort.Height)/2 - (tex.Description.Height/2)));

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
            Height = height
                ;
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
        }
    }
}