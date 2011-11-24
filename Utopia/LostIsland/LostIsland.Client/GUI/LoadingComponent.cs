using System;
using System.Diagnostics;
using System.Drawing;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Sprites;
using SharpDX;
using Color = Utopia.Shared.Structs.Color;

namespace LostIsland.Client.GUI
{
    public class LoadingComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        SpriteRenderer _spriteRender;
        SpriteFont _font;
        SpriteTexture _materia;
        string _loadingText = "Loading...";
        DateTime lastCheck;
        int _points = 0;

        public LoadingComponent(D3DEngine engine)
        {
            _engine = engine;
            DrawOrders.UpdateIndex(0, int.MaxValue);
        }

        public override void LoadContent()
        {
            _font = new SpriteFont();
            _font.Initialize("Lucida Console", 16f, FontStyle.Bold, true, _engine.Device);
            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_engine);

            _materia = new SpriteTexture(_engine.Device, "Images\\materia.png", new SharpDX.Vector2());
            lastCheck = DateTime.Now;
        }

        public override void Update(ref GameTime timeSpent)
        {
            if ((DateTime.Now - lastCheck).TotalSeconds > 0.5)
            {
                if (_points++ > 3) _points = 0;
                _loadingText = "Loading";

                for (int i = 0; i < _points; i++)
                {
                    _loadingText += ".";
                }
                lastCheck = DateTime.Now;
            }

            

            base.Update(ref timeSpent);
        }

        public override void Draw(int index)
        {
            _spriteRender.Begin(false);

            _engine.Context.ClearRenderTargetView(_engine.RenderTarget, new Color4(1,1,1,1));
            _spriteRender.Draw(_materia, new SharpDX.Rectangle(100, 100, 357, 324), Color.White);
            _spriteRender.DrawText(_font, _loadingText, new Vector2(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 100), Color.Black);
            _spriteRender.End();
        }
    }
}
