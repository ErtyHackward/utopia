using System;
using System.Drawing;
using SharpDX;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Component to show something while game loading
    /// </summary>
    public class LoadingComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        SpriteRenderer _spriteRender;
        SpriteFont _font;
        SpriteTexture _materia;
        string _loadingText = "Loading...";
        DateTime _lastCheck;
        int _points;
        ByteColor color = Colors.White;

        public LoadingComponent(D3DEngine engine)
        {
            _engine = engine;
            DrawOrders.UpdateIndex(0, int.MaxValue-100);
        }

        public override void LoadContent(DeviceContext context)
        {
            _font = new SpriteFont();
            _font.Initialize("Lucida Console", 16f, FontStyle.Bold, true, _engine.Device);
            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_engine);

            //_materia = new SpriteTexture(_engine.Device, "Images\\materia.png", new Vector2());
            _lastCheck = DateTime.Now;
        }

        public override void Update(GameTime timeSpend)
        {
            if ((DateTime.Now - _lastCheck).TotalSeconds > 0.5)
            {
                if (_points++ > 3) _points = 0;
                _loadingText = "Loading";

                for (int i = 0; i < _points; i++)
                {
                    _loadingText += ".";
                }
                _lastCheck = DateTime.Now;
            }

            base.Update(timeSpend);
        }

        public override void Draw(DeviceContext context, int index)
        {
            _spriteRender.Begin(context, false);
            context.ClearRenderTargetView(_engine.RenderTarget, new Color4(0, 0, 0, 1));
            //_spriteRender.Draw(_materia, new SharpDX.Rectangle(100, 100, 357, 324), Color.White);
            _spriteRender.DrawText(_font, _loadingText, new Vector2(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 100), color);
            _spriteRender.End();
        }
    }
}
