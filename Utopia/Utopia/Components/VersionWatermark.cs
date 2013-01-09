using System.Reflection;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace Utopia.Components
{
    /// <summary>
    /// Draws current game version on top of the screen
    /// </summary>
    public class VersionWatermark : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        SpriteFont _font;
        SpriteRenderer _spriteRender;
        private bool _update;

        public string WatermarkText { get; set; }

        public VersionWatermark(D3DEngine engine)
        {
            _engine = engine;
            WatermarkText = Assembly.GetEntryAssembly().GetName().Version.ToString();
            _update = true;
            DrawOrders.UpdateIndex(0, 100000);
        }

        public override void LoadContent(DeviceContext context)
        {
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Lucida Console", 12f, System.Drawing.FontStyle.Regular, true, _engine.Device);
            _spriteRender = ToDispose(new SpriteRenderer(_engine));
        }

        public override void Draw(DeviceContext context, int index)
        {
            if (!_update)
            {
                _spriteRender.ReplayLast(context);
                return;
            }

            _update = false;
            _spriteRender.Begin(false, context);

            var pos = new Vector2(5, 5);
            var color = new ByteColor(Color.White);
            color.A = 50;

            _spriteRender.DrawText(_font, WatermarkText, ref pos, ref color);
            _spriteRender.End(context);
        }
    }
}
