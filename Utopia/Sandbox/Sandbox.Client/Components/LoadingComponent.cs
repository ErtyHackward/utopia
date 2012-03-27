using System;
using System.Drawing;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Component to show something while game loading
    /// </summary>
    public class LoadingComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private ImageControl _loadingLabel;
        private SpriteTexture _stLoading;

        public LoadingComponent(D3DEngine engine, MainScreen screen) : base(engine, screen)
        {
            _engine = engine;
            _screen = screen;

            _stLoading = ToDispose(LoadTexture(engine, "Images\\loading.png"));

        }

        public override void Initialize()
        {
            _loadingLabel = new ImageControl();
            _loadingLabel.Image = _stLoading;

            
        }

        private bool active = false;

        public override void EnableComponent()
        {
            if (active) return;

            active = true;
            _screen.Desktop.Children.Add(_loadingLabel);
            Resize(_engine.ViewPort);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            active = false;
            _screen.Desktop.Children.Remove(_loadingLabel);
            base.DisableComponent();
        }

        protected override void EngineViewPortUpdated(Viewport viewport, Texture2DDescription newBackBuffer)
        {
            base.EngineViewPortUpdated(viewport, newBackBuffer);

            Resize(viewport);
        }

        private void Resize(Viewport viewport)
        {
            _loadingLabel.Bounds = new UniRectangle((viewport.Width - 148) / 2 + 50, (viewport.Height - _headerHeight - 58) / 2 + _headerHeight, 148, 58);
        }
    }
}
