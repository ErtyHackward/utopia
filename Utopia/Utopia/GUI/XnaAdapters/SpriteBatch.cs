using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using Rectangle = System.Drawing.Rectangle;
using Utopia.Shared.Structs;
using S33M3Engines.Sprites;
using S33M3Engines.D3D;

namespace Nuclex.UserInterface.Visuals.Flat
{
    public class SpriteBatch
    {

        SpriteRenderer _renderer;

        public GraphicsDevice GraphicsDevice { get; private set; }

        //ouch I need to pass Game down the hierarchy into nuclex ui
        public SpriteBatch(Device device, Game game)
        {
            GraphicsDevice = new GraphicsDevice(device);
            _renderer = new SpriteRenderer();
            _renderer.Initialize(game);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Draw(Texture2D texture2D, Rectangle destRect, Rectangle srcRect, Color color)
        {
            SpriteTexture tex = new SpriteTexture(GraphicsDevice.device, texture2D, Vector2.Zero);

            Matrix transform = Matrix.Scaling((float)destRect.Width / srcRect.Width, (float)destRect.Height / srcRect.Height, 0) *
                               Matrix.Translation(destRect.Left, destRect.Top, 0);

            System.Drawing.RectangleF src = new System.Drawing.RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            _renderer.Render(tex, ref transform, new Color4(color.ToVector4()), src);
        }

        //only used by custom UI for unique item icon ( not optimized, even item icons should be packed in one texture,
        // or maybe with new dx11 stuff like textureArray      
        internal void Draw(Texture2D customTex, Rectangle destinationRegion, Color color)
        {
            throw new NotImplementedException();
        }

        internal void DrawString(SpriteFont spriteFont, string text, Vector2 pos, Color color)
        {
            Matrix transform = Matrix.Translation(pos.X, pos.Y, 0);

            _renderer.RenderText(spriteFont, text, transform, new Color4(color.ToVector4()));//TODO color vs color4
        }

        internal void End()
        {
            _renderer.End();
        }


        internal void Begin()
        {
            _renderer.Begin();
        }
    }
}
