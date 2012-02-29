using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Makes black background
    /// </summary>
    public class BlackBgComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;

        public BlackBgComponent(D3DEngine engine)
        {
            _engine = engine;
            DrawOrders.UpdateIndex(0, 0);
        }

        public override void Draw(int index)
        {
            _engine.Context.ClearRenderTargetView(_engine.RenderTarget, new Color4());
            base.Draw(index);
        }
    }
}
