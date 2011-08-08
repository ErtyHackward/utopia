using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Drawing;
using SharpDX;
using S33M3Engines.D3D;

namespace S33M3Engines.Sprites
{
    public class SpriteTexture
    {
        public ShaderResourceView Texture;
        public bool _textureDispose = true;
        public Texture2DDescription TextureDescr;
        public Matrix ScreenPosition;

        public SpriteTexture(Game game, string TexturePath)
        {
            Texture2D tex = Texture2D.FromFile<Texture2D>(game.GraphicDevice, TexturePath);
            CreateResource(game.GraphicDevice, tex, new Vector2((game.ActivCamera.Viewport.Width / 2) - (tex.Description.Width / 2), (game.ActivCamera.Viewport.Height) / 2 - (tex.Description.Height / 2)));

            game.D3dEngine.ViewPort_Updated += new D3DEngine.ViewPortUpdated(D3dEngine_ViewPort_Updated);
            tex.Dispose();
        }

        //Refresh Sprite Centering when the viewPort size change !
        void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            this.ScreenPosition.M41 = (viewport.Width / 2) - (TextureDescr.Width / 2);
            this.ScreenPosition.M42 = (viewport.Height / 2) - (TextureDescr.Height / 2);

            //ScreenPosition = Matrix.Translation((viewport.Width / 2) - (TextureDescr.Width / 2), (viewport.Height / 2) - (TextureDescr.Height / 2), 0);
        }

        public SpriteTexture(Device device, string TexturePath, Vector2 ScreenPosition)
        {
            Texture2D tex = Texture2D.FromFile<Texture2D>(device, TexturePath);
            CreateResource(device, tex, ScreenPosition);
            tex.Dispose();
        }

        public SpriteTexture(Device device, Texture2D texture, Vector2 ScreenPosition)
        {
            CreateResource(device, texture, ScreenPosition);
        }

        public SpriteTexture(Texture2D texture, ShaderResourceView textureShader, Vector2 ScreenPosition)
        {
            _textureDispose = false;
            Texture = textureShader;
            TextureDescr = texture.Description;
            this.ScreenPosition = Matrix.Translation(ScreenPosition.X, ScreenPosition.Y, 0);
        }

        public SpriteTexture(Texture2DDescription textureDescr, ShaderResourceView textureShader, Vector2 ScreenPosition)
        {
            _textureDispose = false;
            Texture = textureShader;
            TextureDescr = textureDescr;
            this.ScreenPosition = Matrix.Translation(ScreenPosition.X, ScreenPosition.Y, 0);
        }

        private void CreateResource(Device device, Texture2D texture, Vector2 ScreenPosition)
        {
            this.ScreenPosition = Matrix.Translation(ScreenPosition.X, ScreenPosition.Y, 0);
            Texture = new ShaderResourceView(device, texture);
            TextureDescr = texture.Description;
        }

        public void Dispose()
        {
            if(_textureDispose) Texture.Dispose();
        }
    }
}
