using SharpDX;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3DXEngine;

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

        public override void Draw(DeviceContext context, int index)
        {
            context.ClearRenderTargetView(_engine.RenderTarget, new Color4());
            base.Draw(context, index);
        }
    }
}
