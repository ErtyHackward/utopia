using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3Engines.Sprites;

namespace Nuclex.UserInterface.Visuals.Flat
{
    public class SpriteBatch
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public SpriteBatch(Device device) {
            GraphicsDevice = new GraphicsDevice(device);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Draw(Texture2D texture2D, Rectangle destinationRegion, Rectangle rectangle, Color color)
        {
            throw new NotImplementedException();
        }

        internal void Draw(Texture2D customTex, Rectangle destinationRegion, Color color)
        {
            throw new NotImplementedException();
        }

        internal void DrawString(SpriteFont spriteFont, string text, Vector2 vector2, Color color)
        {
            throw new NotImplementedException();
        }

        internal void End()
        {
            throw new NotImplementedException();
        }



        internal void Begin()
        {
            throw new NotImplementedException();
        }
    }
}
